namespace Seyam.Automata.Core
{
    /// <summary>
    /// A marker interface used to flag a State as a "Dead End" or "Finish Line".
    /// When a Child Machine reaches a state implementing this interface, 
    /// the Parent Machine knows the Child has completed its entire sequence.
    /// Thus enabling the Child Machine (local machine) to jump up a layer and hand the execution over to its Parent Machine
    /// </summary>
    public interface ITerminalState
    {
        // Deliberately left empty!
    }
}
