using BehaviourAPI.Core;
using BehaviourAPI.Core.Exceptions;
using System;
using System.Collections.Generic;

namespace BehaviourAPI.StateMachines
{
    public class StateTransition : Transition
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

        public override bool Perform()
        {
            bool canBePerformed = base.Perform();
            if (canBePerformed)
            {
                if (_targetState == null) throw new MissingChildException(this, "The target state can't be null.");
                _fsm.SetCurrentState(_targetState, this);
            }
            return canBePerformed;
        }

        #endregion
    }
}
