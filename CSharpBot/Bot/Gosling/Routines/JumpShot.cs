using System.Numerics;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Hits a target point at a target time towards a target direction.
    /// Target must be no higher than 300uu unless you're feeling lucky. 
    /// </summary>
    public class JumpShot : IShotRoutine
    {
        public void Run(GoslingAgent agent)
        {
            throw new System.NotImplementedException();
        }

        public float InterceptTime { get; set; }
        public Vector3 BallLocation { get; set; }
    }
}