using System;

namespace Seyam.Automata.Core 
{
    public abstract class State<TContext> where TContext : class, new()
    {
        private long _enterTime;

        protected Machine<TContext> Machine { get; }
        protected TContext Context { get; }
        public string Name { get; }
        public long TimeInState => Environment.TickCount - _enterTime;

        public State(Machine<TContext> machine, string name)
        {
            Machine = machine;
            Context = machine.Context;
            Name = name;
        }

        // The state reads and writes to the shared Context
        public virtual void Enter(TContext context) {
            _enterTime = Environment.TickCount;
        }
        public virtual void Update(TContext context) { }
        public virtual void Exit(TContext context) { }
    }
}