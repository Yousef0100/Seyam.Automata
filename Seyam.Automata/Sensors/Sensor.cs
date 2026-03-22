using Seyam.Automata.Core;

namespace Seyam.Automata.Sensors
{

    public abstract class Sensor<TContext> where TContext : class, new()
    {
        public bool IsActive { get; private set; } = true;

        public void Update(TContext context, Machine<TContext> machine)
        {
            if (!IsActive)
                return;

            if (context == null || machine == null)
                return;

            // fire the input only once and not many times
            if (EvaluateAndFire(context, machine))
                IsActive = false;
        }

        protected abstract bool EvaluateAndFire(TContext context, Machine<TContext> machine);

        public bool Reset()
        {
            if (IsActive)
                return false;

            IsActive = true;
            return true;
        }

        public bool Disable()
        {
            if (!IsActive)
                return false;

            IsActive = false;
            return true;
        }
    }
}