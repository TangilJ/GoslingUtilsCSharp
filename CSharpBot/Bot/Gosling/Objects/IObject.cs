using Bot.Utilities.Processed.Packet;

namespace Bot.Gosling.Objects
{
    public interface IObject
    {
        public void Update(Packet packet);
    }
}