using System.Numerics;

namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Porting note: Interface to indicate that a routine has a desired ball intercept time and location where it
    /// should intercept the ball. The original code doesn't have this interface as it's not required for Python, but
    /// we need this for C# to preserve the benefits of static typing. 
    /// </summary>
    public interface IShotRoutine : IRoutine
    {
        float InterceptTime { get; set; }
        Vector3 BallLocation { get; set; }
    }
}