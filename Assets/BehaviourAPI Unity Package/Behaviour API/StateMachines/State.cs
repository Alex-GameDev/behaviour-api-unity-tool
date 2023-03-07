using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System;
using System.Collections.Generic;
using Action = BehaviourAPI.Core.Actions.Action;

namespace BehaviourAPI.StateMachines
{
    public class State : FSMNode, IStatusHandler
    {
        #region ------------------------------------------ Properties -----------------------------------------
        public override Type ChildType => typeof(Transition);

        public override int MaxInputConnections => -1;
        public override int MaxOutputConnections => -1;

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

        public Status LastExecutionStatus => _lastExecutionStatus;

        public Action<Status> StatusChanged { get; set; }

        Status _status;
        Status _lastExecutionStatus;

        #endregion

        #region -------------------------------------------- Fields ------------------------------------------

        public Action Action;

        protected List<Transition> _transitions;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public State()
        {
            _transitions = new List<Transition>();
        }

        public void AddTransition(Transition transition) => _transitions.Add(transition);

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

        public State SetAction(Action action)
        {
            Action = action;
            return this;
        }

        public override object Clone()
        {
            var node = (State)base.Clone();
            node.Action = (Action)Action?.Clone();
            node.StatusChanged = delegate { };
            return node;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public virtual void Start()
        {
            if (Status != Status.None)
                throw new Exception("ERROR: This node is already been executed");

            Status = Status.Running;
            _transitions.ForEach(t => t?.Start());
            Action?.Start();
        }

        public virtual void Update()
        {
            if (Status == Status.Running) 
                Status = Action?.Update() ?? Status.Running;

            CheckTransitions();
        }

        public virtual void Stop()
        {
            if (Status == Status.None)
                throw new Exception("ERROR: This node is already been stopped");

            _lastExecutionStatus = Status;
            Status = Status.None;
            _transitions.ForEach(t => t?.Stop());
            Action?.Stop();
        }

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

        public override void SetExecutionContext(ExecutionContext context)
        {
            Action?.SetExecutionContext(context);
        }

        #endregion
    }
}
