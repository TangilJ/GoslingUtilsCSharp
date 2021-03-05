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
        /// Determines if a point is inside the standard soccer field.
        /// </summary>
        public static bool InField(Vector3 point, float radius)
        {
            point = new Vector3(Math.Abs(point.X), Math.Abs(point.Y), Math.Abs(point.Z));
            if (point.X > 4080 - radius)
                return false;
            if (point.Y > 5900 - radius)
                return false;
            if (point.X > 880 - radius && point.Y > 5105 - radius)
                return false;
            if (point.X > 2650 && point.Y > -point.X + 8025 - radius)
                return false;
            return true;
        }

        /// <summary>
        /// Finds the slope of your car's position relative to the shot vector (shot vector is y axis).
        /// 10 = you are on the axis and the ball is between you and the direction to shoot in
        /// -10 = you are on the wrong side
        /// 1.0 = you're about 45 degrees offcenter
        /// </summary>
        public static float FindSlope(Vector3 shotVector, Vector3 carToTarget)
        {
            var d = Vector3.Dot(shotVector, carToTarget);
            var e = Math.Abs(Vector3.Dot(Vector3.Cross(shotVector, Vector3.UnitZ), carToTarget));
            return Cap(e != 0 ? d / e : 10 * Sign(d), -3, 3);
        }

        /// <summary>
        /// This function returns target locations that are corrected to account for the ball's radius.
        /// It also checks to make sure the ball can fit between the corrected locations.
        /// </summary>
        public static void PostCorrection(Vector3 ballLocation, Vector3 leftTarget, Vector3 rightTarget)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the two roots of a quadratic
        /// </summary>
        public static (float, float) Quadratic(float a, float b, float c)
        {
            var inside = (float) Math.Sqrt(b * b - 4 * a * c);
            if (a != 0)
                return ((-b + inside) / (2 * a), (-b - inside) / (2 * a));
            return (-1, 1);
        }

        /// <summary>
        /// Returns true if the ball is still where the shot anticipates it to be.
        /// First finds the two closest slices in the ball prediction to shot's intercept_time
        /// threshold controls the tolerance we allow the ball to be off by.
        /// </summary>
        public static bool ShotValid(GoslingAgent agent, float threshold = 45)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns -1 for blue team and 1 for orange team.
        /// </summary>
        public static float Side(float x)
        {
            if (x == 0)
                return -1;
            return 1;
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

        /// <summary>
        /// Linearly interpolate from a to b using t.
        /// For instance, when t == 0, a is returned, and when t == 1, b is returned.
        /// Works for both numbers and Vector3s.
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return (b - a) * t + a;
        }

        /// <summary>
        /// Linearly interpolate from a to b using t.
        /// For instance, when t == 0, a is returned, and when t == 1, b is returned.
        /// Works for both numbers and Vector3s.
        /// </summary>
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return (b - a) * t + a;
        }

        /// <summary>
        /// Inverse linear interpolation from a to b with value v.
        /// For instance, it returns 0 if v == a, and returns 1 if v == b, and returns 0.5 if v is exactly between a and b.
        /// Works for both numbers and Vector3s.
        /// </summary>
        public static float InvLerp(float a, float b, float v)
        {
            return (v - a) / (b - a);
        }

        /// <summary>
        /// Inverse linear interpolation from a to b with value v.
        /// For instance, it returns 0 if v == a, and returns 1 if v == b, and returns 0.5 if v is exactly between a and b.
        /// Works for both numbers and Vector3s.
        /// </summary>
        public static Vector3 InvLerp(Vector3 a, Vector3 b, Vector3 v)
        {
            return (v - a) / (b - a);
        }
    }
}