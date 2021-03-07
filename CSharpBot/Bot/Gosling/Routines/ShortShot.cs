using System;
using System.Numerics;
using System.Windows.Media;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// This routine drives towards the ball and attempts to hit it towards a given target.<br/>
    /// It does not require ball prediction and kinda guesses at where the ball will be on its own.
    /// </summary>
    public class ShortShot : IRoutine
    {
        private Vector3 target;

        public ShortShot(Vector3 target)
        {
            this.target = target;
        }

        public void Run(GoslingAgent agent)
        {
            var carToBall = agent.Ball.Physics.Location - agent.Me.Location;
            var distance = carToBall.Length();
            carToBall = Vector3.Normalize(carToBall);
            var ballToTarget = Vector3.Normalize(target - agent.Ball.Physics.Location);

            var relativeVelocity = Vector3.Dot(carToBall, agent.Me.Velocity - agent.Ball.Physics.Velocity);
            float eta;
            if (relativeVelocity != 0)
                eta = Utils.Cap(distance / Utils.Cap(relativeVelocity, 400, 2300), 0, 1.5f);
            else
                eta = 1.5f;

            // If we are approaching the ball from the wrong side the car will try to only hit the very edge of the ball
            var leftVector = Vector3.Cross(carToBall, Vector3.UnitZ);
            var rightVector = Vector3.Cross(carToBall, -Vector3.UnitZ);
            var targetVector = Vector3.Clamp(-ballToTarget, leftVector, rightVector);
            var finalTarget = agent.Ball.Physics.Location + (targetVector * (distance / 2));

            // Some adjustment to the final target to ensure we don't try to dirve through any goalposts to reach it
            if (Math.Abs(agent.Me.Location.Y) > 5150)
                finalTarget.X = Utils.Cap(finalTarget.X, -750, 750);

            agent.Line(finalTarget - new Vector3(0, 0, 100), finalTarget + new Vector3(0, 0, 100), Colors.White);

            var (_, angleY, _) = Utils.DefaultPd(agent, agent.Me.Local(finalTarget - agent.Me.Location));
            Utils.DefaultThrottle(agent,
                distance > 1600
                    ? 2300
                    : 2300 - Utils.Cap(1600 * Math.Abs(angleY), 0, 2050)
            );
            agent.Controller.Boost = agent.Me.Airborne || Math.Abs(angleY) > 0.3 ? false : agent.Controller.Boost;
            agent.Controller.Handbrake = Math.Abs(angleY) > 2.3 || agent.Controller.Handbrake;

            if (Math.Abs(angleY) < 0.05 && (eta < 0.45 || distance < 150))
            {
                agent.Pop();
                agent.Push(new Flip(agent.Me.Local(carToBall)));
            }
        }
    }
}