using System.Numerics;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Very similar to JumpShot(), but instead designed to hit targets above 300uu.
    /// ***This routine is a WIP*** It does not currently hit the ball very hard, nor does it like to be accurate above
    /// 600uu or so.
    /// </summary>
    public class AerialShot : IShotRoutine
    {
        public void Run(GoslingAgent agent)
        {
            throw new System.NotImplementedException();
        }

        public float InterceptTime { get; set; }
        public Vector3 BallLocation { get; set; }
    }
}