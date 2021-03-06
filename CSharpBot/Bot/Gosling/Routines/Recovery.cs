using System.Numerics;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Point towards our velocity vector and land upright, unless we aren't moving very fast.
    /// A vector can be provided to control where the car points when it lands.
    /// </summary>
    public class Recovery : IRoutine
    {
        private Vector3? target;

        public Recovery(Vector3? target = null)
        {
            this.target = target;
        }

        public void Run(GoslingAgent agent)
        {
            Vector3 localTarget;
            if (target.HasValue)
                localTarget = agent.Me.Local((target - agent.Me.Location).Value.Flatten());
            else
                localTarget = agent.Me.Local(agent.Me.Velocity.Flatten());

            Utils.DefaultPd(agent, localTarget);
            agent.Controller.Throttle = 1;
            if (!agent.Me.Airborne)
                agent.Pop();
        }
    }
}