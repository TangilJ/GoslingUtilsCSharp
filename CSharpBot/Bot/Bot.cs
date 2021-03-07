using System.Numerics;
using RLBotDotNet;
using Bot.Gosling;
using Bot.Gosling.Routines;

namespace Bot
{
    class Bot : GoslingAgent
    {
        public Bot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex) { }

        // Porting note: the original code calls the first parameter "agent" instead of "self". In C#, the equivalent of
        // self/agent is "this" and it doesn't appear as the first parameter of a method.
        protected override void Run()
        {
            // An example of using raw utilities:

            var relativeTarget = Ball.Physics.Location - Me.Location;
            // Porting note: In the original code, the first part is agent.ball.location. We use Ball instead of another
            // abstraction, so we need to access .Physics before getting the Location.
            // Also, in C# we can also skip out writing "this." if there are no ambiguities, so we can skip out on the
            // "agent." part as well.

            var localTarget = Me.Local(relativeTarget);

            // Porting note: 
            Utils.DefaultPd(this, localTarget);
            Utils.DefaultThrottle(this, 2300);

            // An example of pushing routines to the stack:
            if (Stack.Count < 1)
            {
                if (KickoffFlag)
                    Push(new Kickoff());
                else
                    Push(new Atba());
            }
        }
    }
}