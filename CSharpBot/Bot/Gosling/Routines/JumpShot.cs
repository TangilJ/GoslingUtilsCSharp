using System.Numerics;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Hits a target point at a target time towards a target direction.<br/>
    /// Target must be no higher than 300uu unless you're feeling lucky. 
    /// </summary>
    public class JumpShot : IShotRoutine
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
        /// The point we dodge at.<br/>
        /// 173 is the 93uu ball radius + a bit more to account for the car's hitbox
        /// </summary>
        private Vector3 dodgePoint;

        /// <summary>
        /// Ratio is how aligned the car is. Low ratios (&lt;0.5) aren't likely to be hit properly.
        /// </summary>
        private float ratio;

        /// <summary>
        /// Whether the car should attempt this backwards
        /// </summary>
        private float direction;

        /// <summary>
        /// Intercept speed not implemented
        /// </summary>
        private float speedDesired;

        /// <summary>
        /// Controls how soon car will jump based on acceleration required. max 584<br/>
        /// Bigger = later, which allows more time to align with shot vector<br/>
        /// Smaller = sooner
        /// </summary>
        private float jumpThreshold = 400;

        private bool jumping = false;
        private bool dodging = false;
        private int counter = 0;

        public JumpShot(
            Vector3 ballLocation, float interceptTime, Vector3 shotVector, float ratio,
            float direction = 1, float speed = 2300
        )
        {
            BallLocation = ballLocation;
            InterceptTime = interceptTime;
            this.shotVector = shotVector;
            this.ratio = ratio;
            this.direction = direction;
            speedDesired = speed;
        }

        public void Run(GoslingAgent agent)
        {
            throw new System.NotImplementedException();
        }

        public float InterceptTime { get; set; }
        public Vector3 BallLocation { get; set; }
    }
}