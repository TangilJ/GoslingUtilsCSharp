using System.Numerics;
using Bot.Utilities.Processed.Packet;

namespace Bot.Gosling.Objects
{
    /// <summary>
    /// This is a simple object that creates/holds goalpost locations for a given team (for soccer on standard maps only).
    /// </summary>
    public class GoalObject : IObject
    {
        public readonly Vector3 Location;

        public readonly Vector3 LeftPost;
        public readonly Vector3 RightPost;

        public GoalObject(int team)
        {
            team = team == 1 ? 1 : -1;
            Location = new Vector3(0, team * 5100, 320);

            // Posts are closer to x=750, but this allows the bot to be a little more accurate
            LeftPost = new Vector3(team * 850, team * 5100, 320);
            RightPost = new Vector3(-team * 850, team * 5100, 320);
        }

        public void Update(Packet packet) { }
    }
}