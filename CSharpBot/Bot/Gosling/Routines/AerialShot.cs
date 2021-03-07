using System;
using System.Numerics;
using System.Windows.Media;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Very similar to JumpShot(), but instead designed to hit targets above 300uu.<br/>
    /// ***This routine is a WIP*** It does not currently hit the ball very hard, nor does it like to be accurate above
    /// 600uu or so.
    /// </summary>
    public class AerialShot : IShotRoutine
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
        /// Dictates when (how late) we jump, much later than in JumpShot because we can take advantage of a double jump
        /// </summary>
        private float jumpThreshold = 600;

        /// <summary>
        /// What time we began our jump at
        /// </summary>
        private float jumpTime = 0;

        /// <summary>
        /// If we need a second jump we have to let go of the jump button for 3 frames, this counts how many frames we have let go for
        /// </summary>
        private int counter = 0;

        // Porting note: the ratio parameter is unused but the original code has it so we keep it in the port
        public AerialShot(Vector3 ballLocation, float interceptTime, Vector3 shotVector, float ratio)
        {
            BallLocation = ballLocation;
            InterceptTime = interceptTime;

            this.shotVector = shotVector;

            intercept = ballLocation - (shotVector * 120);
        }

        public void Run(GoslingAgent agent)
        {
            var rawTimeRemaining = InterceptTime - agent.Time;
            // Capping rawTimeRemaining above 0 to prevent division problems
            var timeRemaining = Utils.Cap(rawTimeRemaining, 0.01f, 10);

            var carToBall = BallLocation - agent.Me.Location;
            // Whether we are to the left or right of the shot vector
            var sideOfShot = Utils.Sign(Vector3.Dot(Vector3.Cross(shotVector, Vector3.UnitZ), carToBall));

            var carToIntercept = intercept - agent.Me.Location;
            var carToInterceptPerp = Vector3.Cross(carToIntercept, new Vector3(0, 0, sideOfShot)); // Perpendicular
            var flatDistanceRemaining = carToIntercept.Flatten().Length();

            var speedRequired = flatDistanceRemaining / timeRemaining;
            // When still on the ground we pretend gravity doesn't exist, for better or worse
            var accelerationRequired = Utils.Backsolve(intercept, agent.Me, timeRemaining, 325);
            var localAccelerationRequired = agent.Me.Local(accelerationRequired);

            // The adjustment causes the car to circle around the dodge point in an effort to line up with the shot vector
            // The adjustment slowly decreases to 0 as the bot nears the time to jump
            var adjustment = carToIntercept.Angle(shotVector) * flatDistanceRemaining / 1.57f; // Size of adjustment
            // Factoring in how close to jump we are
            adjustment *= Utils.Cap(jumpThreshold - accelerationRequired.Z, 0, jumpThreshold) / jumpThreshold;
            // We don't adjust the final target if we are already jumping
            var finalTarget = intercept + (
                jumpTime == 0
                    ? Vector3.Normalize(carToInterceptPerp) * adjustment
                    : Vector3.Zero
            );

            // Some adjustment to the final target to ensure it's inside the field and we don't try to drive through any
            // goalposts to reach it
            if (Math.Abs(agent.Me.Location.Y) > 5150)
                finalTarget.X = Utils.Cap(finalTarget.X, -750, 750);

            var localTarget = agent.Me.Local(finalTarget - agent.Me.Location);

            // Drawing debug lines to show the dodge point and final target (which differs due to the adjustment)
            agent.Line(agent.Me.Location, intercept);
            agent.Line(intercept - new Vector3(0, 0, 100), intercept + new Vector3(0, 0, 100), Colors.Red);
            agent.Line(finalTarget - new Vector3(0, 0, 100), finalTarget + new Vector3(0, 0, 100),
                Color.FromRgb(0, 255, 0));

            var (angleX, angleY, _) = Utils.DefaultPd(agent, localTarget);

            float timeSinceJump;
            if (jumpTime == 0)
            {
                Utils.DefaultThrottle(agent, speedRequired);
                agent.Controller.Boost = Math.Abs(angleY) > 0.3 || agent.Me.Airborne ? false : agent.Controller.Boost;
                agent.Controller.Handbrake = Math.Abs(angleY) > 2.3 || agent.Controller.Handbrake;

                var velocityRequired = carToIntercept / timeRemaining;
                var velAbs = velocityRequired.Abs();
                var goodSlope = velocityRequired.Z / Utils.Cap(velAbs.X + velAbs.Y, 1, 10000) > 0.15;
                var dot = Vector3.Dot(
                    Vector3.Normalize(agent.Me.Velocity.Flatten()),
                    Vector3.Normalize(accelerationRequired.Flatten())
                );
                if (goodSlope && localAccelerationRequired.Z > jumpThreshold && dot > 0.8)
                    // Switch into the jump when the upward acceleration required reaches our threshold, hopefully we
                    // have aligned already...
                    jumpTime = agent.Time;
            }
            else
            {
                timeSinceJump = agent.Time - jumpTime;

                // While airborne we boost if we're within 30 degrees of our local acceleration requirement
                if (agent.Me.Airborne && localAccelerationRequired.Length() * timeRemaining > 90)
                {
                    (angleX, angleY, _) = Utils.DefaultPd(agent, localAccelerationRequired);
                    if (Math.Abs(angleX) + Math.Abs(angleY) < 0.45)
                        agent.Controller.Boost = true;
                }
                else
                {
                    finalTarget -= new Vector3(0, 0, 45);
                    var localFinalTarget = agent.Me.Local(finalTarget - agent.Me.Location);
                    (angleX, angleY, _) = Utils.DefaultPd(agent, localFinalTarget);
                }

                if (counter == 0 && (timeSinceJump <= 0.2 && localAccelerationRequired.Z > 0))
                    // Hold the jump button up to 0.2 seconds to get the most acceleration from the first jump
                    agent.Controller.Jump = true;
                else if (timeSinceJump > 0.2 && counter < 3)
                {
                    // Release the jump button for 3 ticks
                    agent.Controller.Jump = false;
                    agent.Controller.Pitch = 0;
                    agent.Controller.Yaw = 0;
                    agent.Controller.Roll = 0;
                    counter++;
                }
                else if (localAccelerationRequired.Z > 300 && counter == 3)
                {
                    // The acceleration from the second jump is instant, so we only do it for 1 frame
                    agent.Controller.Jump = true;
                    agent.Controller.Pitch = 0;
                    agent.Controller.Yaw = 0;
                    agent.Controller.Roll = 0;
                    counter++;
                }
            }

            if (rawTimeRemaining < -0.25)
            {
                agent.Pop();
                agent.Push(new Recovery());
            }

            if (!Utils.ShotValid(agent, this, 90))
                agent.Pop();
        }

        public float InterceptTime { get; set; }
        public Vector3 BallLocation { get; set; }
    }
}