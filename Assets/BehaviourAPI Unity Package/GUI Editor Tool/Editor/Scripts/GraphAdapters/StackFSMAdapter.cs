using BehaviourAPI.StateMachines;
using BehaviourAPI.StateMachines.StackFSMs;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomAdapter(typeof(StackFSM))]
    public class StackFSMAdapter : StateMachineAdapter
    {      

        #region ------------------- Rendering -------------------

        public override List<Type> MainTypes => new List<Type>
        {
            typeof(State),
            typeof(StateTransition),
            typeof(ExitTransition),
            typeof(StackTransition)
        };

        public override List<Type> ExcludedTypes => new List<Type> {
            typeof(State),
            typeof(ExitTransition),
            typeof(StateTransition),
            typeof(ProbabilisticState),
            typeof(PushTransition),
            typeof(PopTransition)
        };

        protected override void SetUpDetails(NodeView nodeView)
        {
            base.SetUpDetails(nodeView);

            if(nodeView.Node.node is PushTransition)
            {
                nodeView.IconElement.Add(new Label("PUSH"));
                nodeView.IconElement.Enable();  
            }
            else if (nodeView.Node.node is PopTransition)
            {
                nodeView.IconElement.Add(new Label("POP"));
                nodeView.IconElement.Enable();
            }
        }

        #endregion
    }
}
