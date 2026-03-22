namespace Seyam.Automata.Core
{
    public interface IInterruptInput : IInput
        {
            // Marker only to differentiate between Interrupt inputs (which get handled from ANY state) and other regular inputs
        }
}