﻿using System;
using System.Collections.Generic;

namespace BehaviourAPI.StateMachines.StackFSMs
{
    using Core;
    public abstract class StackTransition : Transition
    {
        protected StackFSM _stackFSM;
        public void SetStackFSM(StackFSM stackFSM) => _stackFSM = stackFSM;

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            _stackFSM = BehaviourGraph as StackFSM;

            if (_stackFSM == null)
                throw new Exception("Stack transitions can only be used in StackFSMs");

            base.BuildConnections(parents, children);
        }
    }
}
