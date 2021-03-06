using System;
using System.Numerics;

namespace Bot.Gosling
{
    /// <summary>
    /// Porting note: this class doesn't exist in the original code. We have this in the port so that we can add helper
    /// methods to already existing structures instead of creating completely new ones just to be close to the original.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Sets the Z component of the Vector3 to 0
        /// </summary>
        public static Vector3 Flatten(this Vector3 v) => new Vector3(v.X, v.Y, 0);

        /// <summary>
        /// Returns the angle between this Vector3 and another Vector3.
        /// </summary>
        /// <remarks>
        /// Porting note: for some reason, the original code rounds to 4 decimal places. I don't see any reason to lose
        /// accuracy so this ported method won't do that.
        /// </remarks>
        public static float Angle(this Vector3 v, Vector3 other)
        {
            return (float)
                Math.Acos(
                    Vector3.Dot(
                        Vector3.Normalize(v.Flatten()),
                        Vector3.Normalize(other.Flatten())
                    )
                );
        }
    }
}