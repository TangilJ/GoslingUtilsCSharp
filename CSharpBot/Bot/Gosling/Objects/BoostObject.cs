using System.Numerics;
using Bot.Utilities.Processed.Packet;

namespace Bot.Gosling.Objects
{
    /// <summary>
    /// Porting note: Holds boost pad locations and their current state
    /// </summary>
    public class BoostObject : IObject
    {
        public readonly int Index;
        public readonly Vector3 Location;
        public bool Active;
        public readonly bool Large;

        public BoostObject(int index, Vector3 location, bool large)
        {
            Index = index;
            Location = location;
            Large = large;
        }

        public void Update(Packet packet)
        {
            Active = packet.BoostPadStates[Index].IsActive;
        }
    }
}