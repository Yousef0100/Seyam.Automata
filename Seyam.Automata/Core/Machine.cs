using System;
using System.Collections.Generic;

namespace Seyam.Automata.Core
{
    public class Machine<TContext> where TContext : class, new()
    {
        private State<TContext> _currentState;

        public TContext Context { get; private set; }
        //private Dictionary<State<TContext>, Dictionary<Type, Transition<TContext, IInput>>> Transitions { get; }
        //private Dictionary<Type, Transition<TContext, IInterruptInput>> GlobalInterruptTransitions { get; }
        private Dictionary<State<TContext>, Dictionary<Type, (State<TContext> NextState, Delegate MappingAction)>> Transitions { get; }
        private Dictionary<Type, (State<TContext> NextState, Delegate MappingAction)> GlobalInterruptTransitions { get; }

        public State<TContext> StartingState { get; private set; }
        public State<TContext> CurrentState => _currentState;

        public bool IsRunning { get; private set; }
        public string Name { get; private set; }


        public Machine(TContext context)
        {
            Context = context;
            Transitions = new Dictionary<State<TContext>, Dictionary<Type, (State<TContext> NextState, Delegate MappingAction)>>();
            GlobalInterruptTransitions = new Dictionary<Type, (State<TContext> NextState, Delegate MappingAction)>();
        }

        public void AddState(State<TContext> newState, bool isStartingState)
        {
            if (newState == null)
                throw new ArgumentNullException(nameof(newState), "{0} can't be null");

            if (!Transitions.ContainsKey(newState))
                Transitions.Add(newState, new Dictionary<Type, (State<TContext> NextState, Delegate MappingAction)>());

            StartingState = (isStartingState) ? newState : StartingState;
        }

        public void AddState(State<TContext> state)
        {
            AddState(state, false);
        }

        public void AddStates(int startingStateIdx = -1, params State<TContext>[] states)
        {
            int idx = 0;
            foreach (var state in states)
            {
                if (state != null)
                    AddState(state, startingStateIdx == idx);
                idx++;
            }
        }

        public void AddTransition<TInput>(
            State<TContext> from,
            State<TContext> to,
            Action<TContext, TInput> onTransition = null) where TInput : IInput
        {
            if (from == null || to == null)
                return;

            AddStates(-1, from, to);

            if (!Transitions.ContainsKey(from))
                Transitions[from] = new Dictionary<Type, (State<TContext> NextState, Delegate MappingAction)>();

            Transitions[from][typeof(TInput)] = (to, onTransition);
        }

        public void AddGlobalInterruptTransition<TInterruptInput>(
            State<TContext> to,
            Action<TContext, TInterruptInput> onTransition = null) where TInterruptInput : IInterruptInput
        {
            if (to == null)
                return;

            AddState(to);

            GlobalInterruptTransitions[typeof(TInterruptInput)] = (to, onTransition);
        }

        //public void SetCurrentState (State<TContext> newCurrentState)
        //{
        //    if (newCurrentState == null)
        //        throw new ArgumentNullException(nameof(newCurrentState), "{0} can't be null");

        //    if (!Transitions.ContainsKey(newCurrentState))
        //        throw new InvalidOperationException("new current state is not one of the defined states of this machine.");

        //    CurrentState = newCurrentState;
        //}

        public void Reset()
        {
            Stop();

            _currentState = StartingState;
        }

        public void Start()
        {
            if (StartingState == null)
                throw new ArgumentNullException(nameof(StartingState), "{0} is null, make sure the Starting State is not null.");

            IsRunning = ChangeStateTo(StartingState);
        }

        public void Stop()
        {
            if (CurrentState == null)
                throw new InvalidOperationException("CurrentState is null. Can't stop the machine.");

            CurrentState?.Exit(Context);
            _currentState = null;

            IsRunning = false;
        }

        public void Update()
        {
            if (CurrentState == null)
                throw new InvalidOperationException("CurrentState is null. Can't update the machine.");

            CurrentState?.Update(Context);
        }

        // How a state tells the machine to move to the next phase
        public bool Fire<TInput>(TInput input) where TInput : IInput
        {
            if (CurrentState == null)
                throw new InvalidOperationException("CurrentState is null.");

            if (input == null)
                throw new ArgumentNullException(nameof(input), "{0} is null.");

            Type inputType = input.GetType();

            if (GlobalInterruptTransitions.TryGetValue(inputType, out var globalTransitionData))
            {
                if (globalTransitionData.MappingAction is Action<TContext, TInput> transitionAction)
                {
                    transitionAction.Invoke(Context, input);
                }

                ChangeStateTo(globalTransitionData.NextState);
                return true;
            }

            if (Transitions.TryGetValue(CurrentState, out var stateTransitions) &&
                stateTransitions.TryGetValue(inputType, out var transitionData))
            {
                if (transitionData.MappingAction is Action<TContext, TInput> action)
                {
                    action.Invoke(Context, input);
                }

                ChangeStateTo(transitionData.NextState);
                return true;
            }

            return false;
        }

        // changes the current state to a destination state and handles the exiting and entering of states properly
        // you shouldn't change 'Current State' ever manually
        private bool ChangeStateTo(State<TContext> destinationState)
        {
            if (destinationState == null)
                return false;

            if (!Transitions.ContainsKey(destinationState))
                throw new InvalidOperationException("Distination State is not one of the defined states of this machine.");

            _currentState?.Exit(Context);
            _currentState = destinationState;
            _currentState?.Enter(Context);

            return true;
        }
    }
}

/*
    Mission State Machine                                                   (FSM)
        Active Gameplay State                                                   (State)
            Objectives State Machine                                                    (FSM)
                Objective 1 (Capture the first target)                                      (State)
                    Objective 1 State Machine                                                       (FSM)
                        Go to the location                                                              (State)
                        Find the target                                                                 (State)
                        Kill the enemies                                                                (State)
                        Capture the target                                                              (State)
                        Delivering the target                                                           (State)
                        Target delivered                                                                (State)
                Objcetive 2                                                                 (State)
                    Objective 2 State Machine                                                       (FSM)
                Objective 3                                                                 (State)
                    Objective 3 State Machine                                                       (FSM)
        Mission Failed State                                                    (State)
        Mission Complete State                                                  (State)
 */
