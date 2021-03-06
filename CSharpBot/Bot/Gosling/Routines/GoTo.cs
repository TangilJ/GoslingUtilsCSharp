using System;
using System.Numerics;
using System.Windows.Media;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Drives towards a designated (stationary) target.<br/>
    /// Optional vector controls where the car should be pointing upon reaching the target.<br/>
    /// TODO - slow down if target is inside our turn radius.
    /// </summary>
    public class GoTo : IRoutine
    {
        private Vector3 target;
        private Vector3? vector;
        private float direction;

        public GoTo(Vector3 target, Vector3? vector = null, float direction = 1)
        {
            this.target = target;
            this.vector = vector;
            this.direction = direction;
        }

        public void Run(GoslingAgent agent)
        {
            var carToTarget = target - agent.Me.Location;
            var distanceRemaining = carToTarget.Flatten().Length();

            agent.Line(target - new Vector3(0, 0, 500), target + new Vector3(0, 0, 500), Color.FromRgb(255, 0, 255));

            Vector3 finalTarget;
            if (vector.HasValue)
            {
                // See comments for adjustment in JumpShot or Aerial for explanation
                var sideOfVector =
                    Utils.Sign(Vector3.Dot(Vector3.Cross(vector.Value, Vector3.UnitZ), carToTarget));
                var carToTargetPerp = Vector3.Normalize(Vector3.Cross(carToTarget, new Vector3(0, 0, sideOfVector)));
                var adjustment = carToTarget.Angle(vector.Value) * distanceRemaining / 3.14f;
                finalTarget = target + (carToTargetPerp * adjustment);
            }
            else
                finalTarget = target;

            // Some adjustment to the final target to ensure it's inside the field and we don't try to drive through any
            // goalposts to reach it
            if (Math.Abs(agent.Me.Location.Y) > 5150)
                finalTarget.X = Utils.Cap(finalTarget.X, -750, 750);

            var localTarget = agent.Me.Local(finalTarget - agent.Me.Location);

            var (_, angleY, _) = Utils.DefaultPd(agent, localTarget, direction);
            Utils.DefaultThrottle(agent, 2300, direction);

            agent.Controller.Boost = false;
            agent.Controller.Handbrake = Math.Abs(angleY) > 2.3 || agent.Controller.Handbrake;

            var velocity = 1 + agent.Me.Velocity.Length();
            if (distanceRemaining < 350)
                agent.Stack.Pop();
            else if (Math.Abs(angleY) < 0.05 && velocity > 600 && velocity < 2150 &&
                     distanceRemaining / velocity > 2)
                agent.Stack.Push(new Flip(localTarget));
            else if (Math.Abs(angleY) > 2.8 && velocity < 200)
                agent.Stack.Push(new Flip(localTarget, cancel: true));
            else if (agent.Me.Airborne)
                agent.Stack.Push(new Recovery(target));
        }
    }
}