using System.Numerics;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// A simple 1v1 kickoff that just drives up behind the ball and dodges<br/>
    /// misses the boost on the slight-offcenter kickoffs haha
    /// </summary>
    public class Kickoff : IRoutine
    {
        public void Run(GoslingAgent agent)
        {
            var target = agent.Ball.Physics.Location + new Vector3(0, 200 * Utils.Side(agent.team), 0);
            var localTarget = agent.Me.Local(target - agent.Me.Location);
            Utils.DefaultPd(agent, localTarget);
            Utils.DefaultThrottle(agent, 2300);
            if (localTarget.Length() < 650)
            {
                agent.Stack.Pop();
                // Flip towards opponent goal
                agent.Stack.Push(new Flip(agent.Me.Local(agent.FoeGoal.Location - agent.Me.Location)));
            }   
        }
    }
}