using System;
using System.Numerics;
using Bot.Gosling.Objects;

namespace Bot.Gosling
{
    /// <summary>
    /// This class is for small utilities for math and movement.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Finds the acceleration required for a car to reach a target in a specific amount of time.
        /// </summary>
        public static Vector3 Backsolve(Vector3 target, CarObject car, float time, float gravity = 650)
        {
            var velocityRequired = (target - car.Location) / time;
            var accelerationRequired = velocityRequired - car.Velocity;
            accelerationRequired.Z += gravity * time;
            return accelerationRequired;
        }

        /// <summary>
        /// Caps/clamps a number between a low and high value.
        /// </summary>
        public static float Cap(float x, float low, float high)
        {
            if (x < low)
                return low;
            if (x > high)
                return high;
            return x;
        }

        /// <summary>
        /// Points the car towards a given local target.
        /// Direction can be changed to allow the car to steer towards a target while driving backwards.
        /// </summary>
        public static (float, float, float) DefaultPd(GoslingAgent agent, Vector3 localTarget, float direction = 1)
        {
            localTarget *= direction;
            var up = agent.Me.Local(Vector3.UnitZ);
            (float, float, float) targetAngles = (
                (float) Math.Atan2(localTarget.Z, localTarget.X), // angle required to pitch towards target
                (float) Math.Atan2(localTarget.Y, localTarget.X), // angle required to yaw towards target
                (float) Math.Atan2(up.Y, up.Z) // angle required to roll upright
            );

            // Once we have the angles we need to rotate, we feed them into PD loops to determine the controller inputs.
            agent.Controller.Steer = SteerPd(targetAngles.Item2, 0) * direction;
            agent.Controller.Pitch = SteerPd(targetAngles.Item1, agent.Me.AngularVelocity.Y / 4);
            agent.Controller.Yaw = SteerPd(targetAngles.Item2, -agent.Me.AngularVelocity.Z / 4);
            agent.Controller.Roll = SteerPd(targetAngles.Item3, agent.Me.AngularVelocity.X / 2);

            // Returns the angles, which can be useful for other purposes
            return targetAngles;
        }

        public static float DefaultThrottle(GoslingAgent agent, float targetSpeed, float direction = 1)
        {
            var carSpeed = agent.Me.Local(agent.Me.Velocity).X;
            var t = targetSpeed * direction - carSpeed;
            agent.Controller.Throttle = Cap((float) Math.Pow(t, 2) * Sign(t) / 1000, -1, 1);
            agent.Controller.Boost = t > 150 && carSpeed < 2275 && agent.Controller.Throttle == 1;
            return carSpeed;
        }

        /// <summary>
        /// Returns the sign of a number, -1, 0, +1
        /// </summary>
        public static float Sign(float x)
        {
            if (x < 0)
                return -1;
            if (x > 0)
                return 1;
            return 0;
        }

        /// <summary>
        /// A Proportional-Derivative control loop used for DefaultPD
        /// </summary>
        public static float SteerPd(float angle, float rate)
        {
            return Cap((float) Math.Pow(35 * (angle + rate), 3) / 10, -1, 1);
        }
    }
}