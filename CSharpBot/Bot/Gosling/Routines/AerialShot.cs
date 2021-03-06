using System.Numerics;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Very similar to JumpShot(), but instead designed to hit targets above 300uu.<br/>
    /// ***This routine is a WIP*** It does not currently hit the ball very hard, nor does it like to be accurate above
    /// 600uu or so.
    /// </summary>
    public class AerialShot : IShotRoutine
    {
        /// <summary>
        /// The direction we intend to hit the ball in
        /// </summary>
        private Vector3 shotVector;

        /// <summary>
        /// The point we hit the ball at
        /// </summary>
        private Vector3 intercept;

        /// <summary>
        /// Dictates when (how late) we jump, much later than in JumpShot because we can take advantage of a double jump
        /// </summary>
        private float jumpThreshold = 600;

        /// <summary>
        /// What time we began our jump at
        /// </summary>
        private float jumpTime = 0;

        /// <summary>
        /// If we need a second jump we have to let go of the jump button for 3 frames, this counts how many frames we have let go for
        /// </summary>
        private int counter = 0;
        
        // Porting note: the ratio parameter is unused but the original code has it so we keep it in the port
        public AerialShot(Vector3 ballLocation, float interceptTime, Vector3 shotVector, float ratio)
        {
            BallLocation = ballLocation;
            InterceptTime = interceptTime;

            this.shotVector = shotVector;

            intercept = ballLocation - (shotVector * 120);
        }

        public void Run(GoslingAgent agent)
        {
            throw new System.NotImplementedException();
        }

        public float InterceptTime { get; set; }
        public Vector3 BallLocation { get; set; }
    }
}