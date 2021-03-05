namespace Bot.Gosling.Routines
{
    /// <summary>
    /// Interface for mechanical tasks, called "routines", that the bot can do.
    /// </summary>
    /// <remarks>
    /// Porting note: the original code for GoslingUtils does not use an interface as Python doesn't have them. We use
    /// an interface here for routines as it's the most idiomatic option for C#.
    /// </remarks>
    public interface IRoutine
    {
        void Run(GoslingAgent agent);
    }
}