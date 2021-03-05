using System.Numerics;
using Bot.Utilities.Processed.Packet;

namespace Bot.Gosling.Objects
{
    /// <summary>
    /// This is a simple object that creates/holds goalpost locations for a given team (for soccer on standard maps only).
    /// </summary>
    public class GoalObject : IObject
    {
        public Vector3 location;

        public Vector3 leftPost;
        public Vector3 rightPost;

        public GoalObject(int team)
        {
            team = team == 1 ? 1 : -1;
            location = new Vector3(0, team * 5100, 320);

            // Posts are closer to x=750, but this allows the bot to be a little more accurate
            leftPost = new Vector3(team * 850, team * 5100, 320);
            rightPost = new Vector3(-team * 850, team * 5100, 320);
        }

        public void Update(Packet packet) { }
    }
}