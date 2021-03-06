using System;
using System.Numerics;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Flip takes a vector in local coordinates and flips/dodges in that direction
    /// cancel causes the flip to cancel halfway through, which can be used to half-flip.
    /// </summary>
    public class Flip : IRoutine
    {
        private Vector3 vector;
        private float pitch;
        private float yaw;
        private bool cancel;

        /// <summary>
        /// The time the jump began
        /// </summary>
        private float time = -1;

        /// <summary>
        /// Keeps track of the frames the jump button has been released
        /// </summary>
        private int counter = 0;

        public Flip(Vector3 vector, bool cancel = false)
        {
            this.vector = Vector3.Normalize(vector);
            pitch = Math.Abs(this.vector.X) * -Utils.Sign(this.vector.X);
            yaw = Math.Abs(this.vector.Y) * Utils.Sign(this.vector.Y);
            this.cancel = cancel;
        }

        public void Run(GoslingAgent agent)
        {
            float elapsed;
            if (time == -1)
            {
                elapsed = 0;
                time = agent.Time;
            }
            else
                elapsed = agent.Time - time;

            if (elapsed < 0.15)
                agent.Controller.Jump = true;
            else if (elapsed >= 0.15 && counter < 3)
            {
                agent.Controller.Jump = false;
                counter++;
            }
            else if (elapsed < 0.3 || !cancel && elapsed < 0.9)
            {
                agent.Controller.Jump = true;
                agent.Controller.Pitch = pitch;
                agent.Controller.Yaw = yaw;
            }
            else
            {
                agent.Stack.Pop();
                agent.Stack.Push(new Recovery());
            }
        }
    }
}