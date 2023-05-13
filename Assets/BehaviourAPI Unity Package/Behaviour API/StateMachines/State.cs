﻿using System;
using System.Collections.Generic;

namespace BehaviourAPI.StateMachines
{
    using Core;
    using Core.Actions;
    /// <summary>
    /// Represents a state in a FSM graph.
    /// </summary>
    public class State : FSMNode, IStatusHandler
    {
        #region ------------------------------------------ Properties -----------------------------------------
        public override Type ChildType => typeof(Transition);
        public override int MaxInputConnections => -1;
        public override int MaxOutputConnections => -1;

        /// <summary>
        /// The execution status of the state.
        /// </summary>
        public Status Status
        {
            get => _status;
            protected set
            {
                if (_status != value)
                {
                    _status = value;
                    StatusChanged?.Invoke(_status);
                }
            }
        }

        /// <summary>
        /// Event called when current status changed.
        /// </summary>
        public Action<Status> StatusChanged { get; set; }

        #endregion

        #region -------------------------------------------- Fields ------------------------------------------

        /// <summary>
        /// The action that this state executes.
        /// </summary>
        public Action Action;

        #endregion

        #region -------------------------------------- Private variables -------------------------------------

        Status _status;

        /// <summary>
        /// The list of transitions that have this state as source state.
        /// </summary>
        protected List<Transition> _transitions;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public State()
        {
            _transitions = new List<Transition>();
        }

        /// <summary>
        /// Add a new transition to the list.
        /// </summary>
        /// <param name="transition">The added transition.</param>
        protected internal void AddTransition(Transition transition) => _transitions.Add(transition);

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            _transitions = new List<Transition>();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Transition t)
                    _transitions.Add(t);
                else
                    throw new ArgumentException();
            }
        }

        public override object Clone()
        {
            var node = (State)base.Clone();
            node.Action = (Action)Action?.Clone();
            node.StatusChanged = (Action<Status>)StatusChanged?.Clone();
            return node;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// Change status to Running.
        /// Starts the action execution and initialize the transitions.
        /// </summary>
        /// <exception cref="ExecutionStatusException">If the node execution already started. </exception>
        public virtual void Start()
        {
            if (Status != Status.None)
                throw new ExecutionStatusException(this, "ERROR: This node is already been executed");

            Status = Status.Running;
            Action?.Start();
            _transitions.ForEach(t => t?.Start());
        }

        /// <summary>
        /// Update the action and check the transitions.
        /// </summary>
        public virtual void Update()
        {
            if (Status == Status.Running)
                Status = Action?.Update() ?? Status.Running;

            CheckTransitions();
        }

        /// <summary>
        /// Change status to none.
        /// Stop the action and reset the transitions
        /// </summary>
        /// <exception cref="Exception">If was already stopped.</exception>
        public virtual void Stop()
        {
            if (Status == Status.None)
                throw new Exception("ERROR: This node is already been stopped");

            Status = Status.None;
            Action?.Stop();
            _transitions.ForEach(t => t?.Stop());
        }


        public virtual void Pause()
        {
            Action?.Pause();
            _transitions.ForEach(t => t?.Pause());
        }

        public virtual void Unpause()
        {
            Action?.Unpause();
            _transitions.ForEach(t => t?.Unpause());
        }

        /// <summary>
        /// Check the transitions until one of them is triggered.
        /// </summary>
        protected virtual void CheckTransitions()
        {
            for (int i = 0; i < _transitions.Count; i++)
            {
                if (_transitions[i] != null && CheckTransition(_transitions[i]))
                {
                    _transitions[i]?.Perform();
                }
            }
        }

        /// <summary>
        /// Check if <paramref name="t"/> flags matches the current status and then check the transition.
        /// </summary>
        /// <param name="t">The transition checked.</param>
        /// <returns>True if the flags matches and the transition was triggered.</returns>
        protected bool CheckTransition(Transition t)
        {
            if (((uint)Status & (uint)t.StatusFlags) != 0)
            {
                return t.Check();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// Passes the context to the action.
        /// </summary>
        /// <param name="context"><inheritdoc/></param>
        public override void SetExecutionContext(ExecutionContext context)
        {
            Action?.SetExecutionContext(context);
        }

        #endregion
    }
}
