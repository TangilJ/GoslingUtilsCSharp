namespace Bot.Gosling.Routines
{
    /// <summary>
    /// An example routine that just drives towards the ball at max speed.
    /// </summary>
    public class Atba : IRoutine
    {
        public void Run(GoslingAgent agent)
        {
            var relativeTarget = agent.Ball.Physics.Location - agent.Me.Location;
            var localTarget = agent.Me.Local(relativeTarget);
            Utils.DefaultPd(agent, localTarget);
            Utils.DefaultThrottle(agent, 2300);
        }
    }
}