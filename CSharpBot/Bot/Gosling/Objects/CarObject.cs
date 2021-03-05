using System.Numerics;
using Bot.Utilities.Processed.Packet;

namespace Bot.Gosling.Objects
{
    /// <summary>
    /// The CarObject, and kin, convert the GameTickPacket in something a little friendlier to use,
    /// and are updated by GoslingAgent as the game runs.
    /// </summary>
    public class CarObject : IObject
    {
        public Vector3 Location = Vector3.Zero;

        // Porting note: we use Orientation instead of a 3x3 matrix because System.Numerics does not have Matrix3x3 and
        // also because Orientation is essentially the same thing with a slightly different interface.
        public Orientation Orientation;

        public Vector3 Velocity = Vector3.Zero;
        public Vector3 AngularVelocity = Vector3.Zero;
        public bool Demolished = false;
        public bool Airborne = false;
        public bool Supersonic = false;
        public bool Jumped = false;
        public bool DoubleJumped = false;
        public float Boost = 0;
        public int Index;

        public CarObject(int index, Packet packet = null)
        {
            Orientation = new Orientation(null); // Passing in null is the same as passing pitch=0, roll=0, yaw=0
            Index = index;

            if (packet is not null)
                Update(packet);
        }

        /// <summary>
        /// Shorthand for Orientation.RelativeLocation(Vector3.Zero, value, Orientation)
        /// </summary>
        public Vector3 Local(Vector3 value)
        {
            return Orientation.RelativeLocation(Vector3.Zero, value, Orientation);
        }

        public void Update(Packet packet)
        {
            var car = packet.Players[Index];
            Location = car.Physics.Location;
            Velocity = car.Physics.Velocity;
            Orientation = car.Physics.Rotation;
            AngularVelocity = Orientation.RelativeLocation(
                Vector3.Zero,
                car.Physics.AngularVelocity,
                Orientation
            );
            Demolished = car.IsDemolished;
            Airborne = !car.HasWheelContact;
            Supersonic = car.IsSupersonic;
            Jumped = car.Jumped;
            DoubleJumped = car.DoubleJumped;
            Boost = car.Boost;
        }

        /// <summary>
        /// A vector pointing forwards relative to the car's orientation. Its magnitude is 1.
        /// </summary>
        public Vector3 Forward => Orientation.Forward;

        /// <summary>
        /// A vector pointing left relative to the car's orientation. Its magnitude is 1.
        /// </summary>
        /// <remarks>
        /// Porting note: this is actually Orientation.Right but the original code calls it Left.
        /// </remarks>
        public Vector3 Left => Orientation.Right;

        /// <summary>
        /// A vector pointing up relative to the car's orientation. Its magnitude is 1.
        /// </summary>
        public Vector3 Up => Orientation.Up;
    }
}