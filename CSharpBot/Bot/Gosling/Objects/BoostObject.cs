using System.Numerics;
using Bot.Utilities.Processed.Packet;

namespace Bot.Gosling.Objects
{
    /// <summary>
    /// Porting note: Holds boost pad locations and their current state
    /// </summary>
    public class BoostObject : IObject
    {
        public int index;
        public Vector3 location;
        public bool active;
        public bool large;

        public BoostObject(int index, Vector3 location, bool large)
        {
            this.index = index;
            this.location = location;
            this.large = large;
        }

        public void Update(Packet packet)
        {
            active = packet.BoostPadStates[index].IsActive;
        }
    }
}