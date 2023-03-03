using System;
using System.Collections.Generic;

namespace BehaviourAPI.StateMachines
{
    using Core;
    using Core.Perceptions;
    using Action = Core.Actions.Action;

    public abstract class Transition : FSMNode, IPushActivable
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override Type ChildType => typeof(State);

        public override int MaxInputConnections => 1;

        public System.Action TransitionTriggered { get; set; }

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        public Perception Perception;

        public Action Action;

        protected FSM _fsm;

        protected State _sourceState;

        public StatusFlags StatusFlags;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public void SetFSM(FSM fsm) => _fsm = fsm;
        public void SetSourceState(State source) => _sourceState = source;

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);

            _fsm = BehaviourGraph as FSM;

            if (parents.Count > 0 && parents[0] is State from)
                _sourceState = from;
            else
                throw new ArgumentException();
        }


        public override object Clone()
        {
            var node = (Transition)base.Clone();
            node.Action = (Action)Action?.Clone();
            node.Perception = (Perception)Perception?.Clone();
            return node;
        }


        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public void Start() => Perception?.Initialize();
        public void Stop() => Perception?.Reset();

        public virtual bool Check()
        {
            return Perception?.Check() ?? true;
        }

        public virtual void Perform()
        {
            if (!_fsm.IsCurrentState(_sourceState)) return;

            if (Action != null)
            {
                Action.Start();
                Action.Update();
                Action.Stop();
            }

            TransitionTriggered?.Invoke();
        }

        public void Fire() => Perform();

        public override void SetExecutionContext(ExecutionContext context)
        {
            Action?.SetExecutionContext(context);
            Perception?.SetExecutionContext(context);
        }

        #endregion
    }
}
