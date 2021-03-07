using System;
using System.Numerics;
using System.Windows.Media;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Hits a target point at a target time towards a target direction.<br/>
    /// Target must be no higher than 300uu unless you're feeling lucky. 
    /// </summary>
    public class JumpShot : IShotRoutine
    {
        /// <summary>
        /// The direction we intend to hit the ball in
        /// </summary>
        private Vector3 shotVector;

        /// <summary>
        /// The point we hit the ball at
        /// </summary>
        private Vector3 intercept;

        /// <summary>
        /// The point we dodge at.<br/>
        /// 173 is the 93uu ball radius + a bit more to account for the car's hitbox
        /// </summary>
        private Vector3 dodgePoint;

        /// <summary>
        /// Ratio is how aligned the car is. Low ratios (&lt;0.5) aren't likely to be hit properly.
        /// </summary>
        private float ratio;

        /// <summary>
        /// Whether the car should attempt this backwards
        /// </summary>
        private float direction;

        /// <summary>
        /// Intercept speed not implemented
        /// </summary>
        private float speedDesired;

        /// <summary>
        /// Controls how soon car will jump based on acceleration required. max 584<br/>
        /// Bigger = later, which allows more time to align with shot vector<br/>
        /// Smaller = sooner
        /// </summary>
        private float jumpThreshold = 400;

        private bool jumping = false;
        private bool dodging = false;
        private int counter = 0;

        private float p;
        private float y;

        public JumpShot(
            Vector3 ballLocation, float interceptTime, Vector3 shotVector, float ratio,
            float direction = 1, float speed = 2300
        )
        {
            BallLocation = ballLocation;
            InterceptTime = interceptTime;
            this.shotVector = shotVector;
            this.ratio = ratio;
            this.direction = direction;
            speedDesired = speed;
        }

        public void Run(GoslingAgent agent)
        {
            var rawTimeRemaining = InterceptTime - agent.Time;
            // Capping raw_time_remaining above 0 to prevent division problems
            var timeRemaining = Utils.Cap(rawTimeRemaining, 0.001f, 10);
            var carToBall = BallLocation - agent.Me.Location;
            // Whether we are to the left or right of the shot vector
            var sideOfShot = Utils.Sign(Vector3.Dot(Vector3.Cross(shotVector, Vector3.UnitZ), carToBall));

            var carToDodgePoint = dodgePoint - agent.Me.Location;
            var carToDodgePerp = Vector3.Cross(carToDodgePoint, new Vector3(0, 0, sideOfShot)); // Perpendicular
            var distanceRemaining = carToDodgePoint.Length();

            var speedRequired = distanceRemaining / timeRemaining;
            var accelerationRequired = Utils.Backsolve(dodgePoint, agent.Me, timeRemaining, !jumping ? 0 : 650);
            var localAccelerationRequired = agent.Me.Local(accelerationRequired);

            // The adjustment causes the car to circle around the dodge point in an effort to line up with the shot
            // vector
            // The adjustment slowly decreases to 0 as the bot nears the time to jump
            var adjustment = carToDodgePoint.Angle(shotVector) * distanceRemaining / 2f; // Size of adjustment
            adjustment *= (Utils.Cap(jumpThreshold - (accelerationRequired.Z), 0, jumpThreshold) / jumpThreshold);
            // Factoring in how close to jump we are

            // We don't adjust the final target if we are already jumping
            var finalTarget =
                dodgePoint +
                (!jumping ? Vector3.Normalize(carToDodgePerp) * adjustment : Vector3.Zero) +
                new Vector3(0, 0, 50);
            // Ensuring our target isn't too close to the sides of the field, where our car would get messed up by the
            // radius of the curves

            // Some adjustment to the final target to ensure it's inside the field and we don't try to drive through any
            // goalposts to reach it
            if (Math.Abs(agent.Me.Location.Y) > 5150)
                finalTarget.X = Utils.Cap(finalTarget.X, -750, 750);

            var localFinalTarget = agent.Me.Local(finalTarget - agent.Me.Location);

            // Drawing debug lines to show the dodge point and final target (which differs due to the adjustment)
            agent.Line(agent.Me.Location, dodgePoint);
            agent.Line(dodgePoint - new Vector3(0, 0, 100), dodgePoint + new Vector3(0, 0, 100), Colors.Red);
            agent.Line(finalTarget - new Vector3(0, 0, 100), finalTarget + new Vector3(0, 0, 100), Colors.Lime);
            agent.Line(agent.Ball.Physics.Location, agent.Ball.Physics.Location + (shotVector * 300));

            // Calling our drive utils to get us going towards the final target
            var (_, angleY, _) = Utils.DefaultPd(agent, localFinalTarget, direction);
            Utils.DefaultThrottle(agent, speedRequired, direction);

            agent.Line(agent.Me.Location, agent.Me.Location + (shotVector * 200), Colors.White);

            agent.Controller.Boost = Math.Abs(angleY) > 0.3 || agent.Me.Airborne ? false : agent.Controller.Boost;
            agent.Controller.Handbrake = Math.Abs(angleY) > 2.3 && direction == 1 || agent.Controller.Handbrake;


            if (!jumping)
            {
                if (rawTimeRemaining <= 0
                    || (speedRequired - 2300) * timeRemaining > 60
                    || !Utils.ShotValid(agent, this))
                {
                    // If we're out of time or not fast enough to be within 45 units of target at the intercept time, we
                    // pop
                    agent.Pop();
                    if (agent.Me.Airborne)
                        agent.Push(new Recovery());
                }

                else if (localAccelerationRequired.Z > jumpThreshold &&
                         localAccelerationRequired.Z > localAccelerationRequired.Flatten().Length())
                    // Switch into the jump when the upward acceleration required reaches our threshold, and our lateral
                    // acceleration is negligible
                    jumping = true;
            }
            else
            {
                if (rawTimeRemaining > 0.2 && !Utils.ShotValid(agent, this, 150)
                    || rawTimeRemaining <= -0.9
                    || !agent.Me.Airborne && counter > 0)
                {
                    agent.Pop();
                    agent.Push(new Recovery());
                }
                else if (counter == 0 && localAccelerationRequired.Z > 0 && rawTimeRemaining > 0.083)
                    // Initial jump to get airborne + we hold the jump button for extra power as required
                    agent.Controller.Jump = true;
                else if (counter < 3)
                {
                    // Make sure we aren't jumping for at least 3 frames
                    agent.Controller.Jump = false;
                    counter++;
                }
                else if (rawTimeRemaining <= 0.1 && rawTimeRemaining > -0.9)
                {
                    // Dodge in the direction of the shotVector
                    agent.Controller.Jump = true;
                    if (!dodging)
                    {
                        var vector = agent.Me.Local(shotVector);
                        p = Math.Abs(vector.X) * -Utils.Sign(vector.X);
                        y = Math.Abs(vector.Y) * Utils.Sign(vector.Y) * direction;
                        dodging = true;
                    }

                    // Simulating a deadzone so that the dodge is more natural
                    agent.Controller.Pitch = Math.Abs(p) > 0.2 ? p : 0;
                    agent.Controller.Yaw = Math.Abs(y) > 0.3 ? y : 0;
                }
            }
        }

        public float InterceptTime { get; set; }
        public Vector3 BallLocation { get; set; }
    }
}