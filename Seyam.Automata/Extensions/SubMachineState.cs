using Seyam.Automata.Core;

namespace Seyam.Automata.Extensions
{
    public abstract class SubMachineState<TContext> : State<TContext> where TContext : class, new()
    {
        protected Machine<TContext> SubMachine { get; private set; }

        private bool _isSetup;
        private bool _hasSubMachineFiredCompletion = false;

        public SubMachineState(Machine<TContext> parentMachine, string name) : base(parentMachine, name)
        {
            SubMachine = new Machine<TContext>(parentMachine.Context);
            _isSetup = true;
        }

        public override void Enter(TContext context)
        {
            base.Enter(context);

            if (_isSetup) {
                OnSetupSubMachine();
                _isSetup = false;
            }

            _hasSubMachineFiredCompletion = false;

            SubMachine.Start();
        }


        public override void Update(TContext context)
        {
            if (SubMachine.IsRunning)
            {
                SubMachine.Update();

                if (!_hasSubMachineFiredCompletion && SubMachine.CurrentState is ITerminalState)
                {
                    _hasSubMachineFiredCompletion = true;
                    OnSubMachineComplete();
                }
            }
        }

        public override void Exit(TContext context)
        {
            if (SubMachine.IsRunning) {
                SubMachine.Stop();
            }
        }

        // Where you define the states, transitions, and starting state of this specific module
        // Normaly would happen after instantiating the SubMachine by calling
        protected abstract void OnSetupSubMachine();

        // Where you define the "Finish Line" condition that tells the Parent Machine to move on
        // What actions to take when the SubMachine reaches a terminal state
        protected abstract void OnSubMachineComplete();
    }
}