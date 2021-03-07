using System;
using System.Numerics;
using System.Windows.Media;
using Bot.Gosling.Objects;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Very similar to goto() but designed for grabbing boost.<br/>
    /// If a target is provided the bot will try to be facing the target as it passes over the boost.
    /// </summary>
    public class GoToBoost : IRoutine
    {
        private BoostObject boost;
        private Vector3? target;

        public GoToBoost(BoostObject boost, Vector3? target = null)
        {
            this.boost = boost;
            this.target = target;
        }

        public void Run(GoslingAgent agent)
        {
            var carToBoost = boost.Location - agent.Me.Location;

            var distanceRemaining = carToBoost.Flatten().Length();

            agent.Line(
                boost.Location - new Vector3(0, 0, 500),
                boost.Location + new Vector3(0, 0, 500),
                Colors.Lime
            );

            float adjustment;
            Vector3 finalTarget;
            float carToTarget;
            if (target.HasValue)
            {
                var vector = Vector3.Normalize(target.Value - boost.Location);
                var sideOfVector = Utils.Sign(Vector3.Dot(Vector3.Cross(vector, Vector3.UnitZ), carToBoost));
                var carToBoostPerp = Vector3.Normalize(Vector3.Cross(carToBoost, new Vector3(0, 0, sideOfVector)));
                adjustment = carToBoost.Angle(vector) * distanceRemaining / 3.14f;
                finalTarget = boost.Location + (carToBoostPerp * adjustment);
                carToTarget = (target.Value - agent.Me.Location).Length();
            }
            else
            {
                adjustment = 9999;
                carToTarget = 0;
                finalTarget = boost.Location;
            }

            // Some adjustment to the final target to ensure it's inside the field and we don't try to drive through any
            // goalposts to reach it
            if (Math.Abs(agent.Me.Location.Y) > 5150)
                finalTarget.X = Utils.Cap(finalTarget.X, -750, 750);

            var localTarget = agent.Me.Local(finalTarget - agent.Me.Location);

            var (_, angleY, _) = Utils.DefaultPd(agent, localTarget);
            Utils.DefaultThrottle(agent, 2300);

            agent.Controller.Boost = Math.Abs(angleY) < 0.3 && boost.Large;
            agent.Controller.Handbrake = Math.Abs(angleY) > 2.3 || agent.Controller.Handbrake;

            var velocity = 1 + agent.Me.Velocity.Length();
            if (!boost.Active || agent.Me.Boost >= 99 || distanceRemaining < 350)
                agent.Pop();
            else if (agent.Me.Airborne)
                agent.Push(new Recovery(target));
            else if (Math.Abs(angleY) < 0.05 && velocity > 600 && velocity < 2150 && distanceRemaining / velocity > 2
                     || adjustment < 90 && carToTarget / velocity > 2.0)
                agent.Push(new Flip(localTarget));
        }
    }
}