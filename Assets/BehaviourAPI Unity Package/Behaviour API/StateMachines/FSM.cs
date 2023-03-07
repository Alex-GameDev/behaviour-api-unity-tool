using System;

namespace BehaviourAPI.StateMachines
{
    using Core;
    using Core.Perceptions;
    using Core.Actions;
    using System.Xml.Linq;

    public class FSM : BehaviourGraph
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override Type NodeType => typeof(FSMNode);

        public override bool CanRepeatConnection => false;

        public override bool CanCreateLoops => true;

        public Transition LastPerformedTransition { get; private set; }

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        protected State _currentState;


        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        protected T CreateInternalTransition<T>(string name, State from, Perception perception, Action action, StatusFlags flags) where T : Transition, new()
        {
            T transition = CreateNode<T>(name);
            transition.SetFSM(this);
            transition.Perception = perception;
            transition.Action = action;
            transition.StatusFlags = flags;
            Connect(from, transition);
            transition.SetSourceState(from);
            from.AddTransition(transition);
            return transition;
        }

        protected T CreateInternalTransition<T>(State from, Perception perception, Action action, StatusFlags flags) where T : Transition, new()
        {
            T transition = CreateNode<T>();
            transition.SetFSM(this);
            transition.Perception = perception;
            transition.Action = action;
            transition.StatusFlags = flags;
            Connect(from, transition);
            transition.SetSourceState(from);
            from.AddTransition(transition);
            return transition;
        }

        /// <summary>
        /// Create a new State in this <see cref="FSM"/> that executes the <see cref="Action"/> specified in <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The action this state executes.</param>
        /// <returns>The <see cref="State"/> created.</returns>
        public State CreateState(Action action = null)
        {
            State state = CreateNode<State>();
            state.Action = action;
            return state;
        }

        /// <summary>
        /// Create a new State named <paramref name="name"/> in this <see cref="FSM"/> that executes the <see cref="Action"/> specified in <paramref name="action"/>.
        /// </summary>
        /// <param name="name">The name of this node.</param>
        /// <param name="action">The action this state executes.</param>
        /// <returns>The <see cref="State"/> created.</returns>
        public State CreateState(string name, Action action = null)
        {
            State state = CreateNode<State>(name);
            state.Action = action;
            return state;
        }



        /// <summary>
        /// Create a new State of type <typeparamref name="T"/> in this <see cref="FSM"/> that executes the given action.
        /// </summary>
        /// <param name="action">The action this state wil executes</param>
        /// <returns>The State created</returns>
        public T CreateState<T>(Action action = null) where T : State, new()
        {
            T state = CreateNode<T>();
            state.Action = action;
            return state;
        }

        /// <summary>
        /// Create a new State of type <typeparamref name="T"/> named <paramref name="name"/> in this <see cref="FSM"/> that executes the <see cref="Action"/> specified in <paramref name="action"/>.
        /// </summary>
        /// <param name="name">The name of this node.</param>
        /// <param name="action">The action this state executes.</param>
        /// <returns>The <typeparamref name="T"/> created.</returns>
        public T CreateState<T>(string name, Action action = null) where T : State, new()
        {
            T state = CreateNode<T>(name);
            state.Action = action;
            return state;
        }


        /// <summary>
        /// Create a new <see cref="StateTransition"/> of type <typeparamref name="T"/> named <paramref name="name"/> in this <see cref="FSM"/> that goes from the state <paramref name="from"/> to the state <paramref name="to"/>.
        /// The transition checks <paramref name="perception"/> and executes <paramref name="action"/> when is performed. If <paramref name="perception"/> is not specified or is null, the transition works as a lambda transition.
        /// To disable the transition from being checked from the source state, set <paramref name="isPulled"/> to false. 
        /// </summary>
        /// <param name="name">The name of the transition.</param>
        /// <param name="from">The source state of the transition and it's parent node.</param>
        /// <param name="to">The target state of the transition and it's child node.</param>
        /// <param name="perception">The perception checked by the transition.</param>
        /// <param name="action">The action executed by the transition.</param>
        /// <param name="statusFlags">The status that the source state can have to check the perception. If none, the transition will never be checked.</param>
        /// <returns>The <see cref="ExitTransition"/> created.</returns>

        public StateTransition CreateTransition(string name, State from, State to, Perception perception = null, Action action = null, StatusFlags statusFlags = StatusFlags.Actived)
        {
            StateTransition transition = CreateInternalTransition<StateTransition>(name, from, perception, action, statusFlags);
            Connect(transition, to);
            transition.SetTargetState(to);
            return transition;       
        }

        /// <summary>
        /// Create a new <see cref="StateTransition"/> of type <typeparamref name="T"/> in this <see cref="FSM"/> that goes from the state <paramref name="from"/> to the state <paramref name="to"/>.
        /// The transition checks <paramref name="perception"/> and executes <paramref name="action"/> when is performed. If <paramref name="perception"/> is not specified or is null, the transition works as a lambda transition.
        /// To disable the transition from being checked from the source state, set <paramref name="isPulled"/> to false. 
        /// </summary>
        /// <param name="from">The source state of the transition and it's parent node.</param>
        /// <param name="to">The target state of the transition and it's child node.</param>
        /// <param name="perception">The perception checked by the transition.</param>
        /// <param name="action">The action executed by the transition.</param>
        /// <param name="statusFlags">The status that the source state can have to check the perception. If none, the transition will never be checked.</param>
        /// <returns>The <see cref="StateTransition"/> created.</returns>
        public StateTransition CreateTransition(State from, State to, Perception perception = null, Action action = null, StatusFlags statusFlags = StatusFlags.Actived)
        {
            StateTransition transition = CreateInternalTransition<StateTransition>(from, perception, action, statusFlags);
            Connect(transition, to);
            transition.SetTargetState(to);
            return transition;
        }



        /// <summary>
        /// Create a new <see cref="ExitTransition"/> named <paramref name="name"/> in this <see cref="FSM"/> that goes from the state <paramref name="from"/>  to exit the graph with value of <paramref name="exitStatus"/>.
        /// The transition checks <paramref name="perception"/> and executes <paramref name="action"/> when is performed. If <paramref name="perception"/> is not specified or is null, the transition works as a lambda transition.
        /// To disable the transition from being checked from the source state, set <paramref name="isPulled"/> to false. 
        /// </summary>
        /// <param name="name">The name of the transition.</param>
        /// <param name="from">The source state of the transition and it's parent node.</param>
        /// <param name="perception">The perception checked by the transition.</param>
        /// <param name="action">The action executed by the transition.</param>
        /// <param name="statusFlags">The status that the source state can have to check the perception. If none, the transition will never be checked.</param>
        /// <returns>The <see cref="ExitTransition"/> created.</returns>
        public ExitTransition CreateExitTransition(string name, State from, Status exitStatus, Perception perception = null, Action action = null, StatusFlags statusFlags = StatusFlags.Actived)
        {
            ExitTransition transition = CreateInternalTransition<ExitTransition>(name, from, perception, action, statusFlags);
            transition.ExitStatus = exitStatus;
            return transition;
        }

        /// <summary>
        /// Create a new <see cref="ExitTransition"/> in this <see cref="FSM"/> that goes from the state <paramref name="from"/> to exit the graph with value of <paramref name="exitStatus"/>.
        /// The transition checks <paramref name="perception"/> and executes <paramref name="action"/> when is performed. If <paramref name="perception"/> is not specified or is null, the transition works as a lambda transition.
        /// To disable the transition from being checked from the source state, set <paramref name="isPulled"/> to false. 
        /// </summary>
        /// <param name="from">The source state of the transition and it's parent node.</param>
        /// <param name="perception">The perception checked by the transition.</param>
        /// <param name="action">The action executed by the transition.</param>
        /// <param name="statusFlags">The status that the source state can have to check the perception. If none, the transition will never be checked.</param>
        /// <returns>The <see cref="ExitTransition"/> created.</returns>
        public ExitTransition CreateExitTransition(State from, Status exitStatus, Perception perception = null, Action action = null, StatusFlags statusFlags = StatusFlags.Actived)
        {
            ExitTransition transition = CreateInternalTransition<ExitTransition>(from, perception, action, statusFlags);
            transition.ExitStatus = exitStatus;
            return transition;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public void SetEntryState(State state)
        {
            StartNode = state;
        }

        public override void Start()
        {
            base.Start();

            _currentState = StartNode as State;
            _currentState?.Start();
        }

        public override void Execute()
        {
            _currentState?.Update();
        }

        public override void Stop()
        {
            base.Stop();
            _currentState?.Stop();
        }

        public virtual void SetCurrentState(State state, Transition transition)
        {
            if(LastPerformedTransition != null)
                LastPerformedTransition.SourceStateLastStatus = Status.None;

            LastPerformedTransition = transition;
            _currentState?.Stop();
            _currentState = state;
            _currentState?.Start();
        }

        public bool IsCurrentState(State state) => _currentState == state;

        #endregion
    }
}
