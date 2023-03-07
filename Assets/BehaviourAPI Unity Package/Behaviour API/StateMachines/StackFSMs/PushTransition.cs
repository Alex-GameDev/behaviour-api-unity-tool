using System;
using System.Collections.Generic;

namespace BehaviourAPI.StateMachines.StackFSMs
{
    using Core;
    using Core.Exceptions;

    public class PushTransition : StackTransition
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxOutputConnections => 1;

        protected State _targetState;


        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public void SetTargetState(State target) => _targetState = target;

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);

            if (children.Count > 0 && children[0] is State to)
                _targetState = to;
            else
                throw new ArgumentException();
        }


        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Perform()
        {
            base.Perform();
            if (_targetState == null) throw new MissingChildException(this, "The target state can't be null.");

            _stackFSM.Push(_targetState, this);
        }

        #endregion

    }
}
