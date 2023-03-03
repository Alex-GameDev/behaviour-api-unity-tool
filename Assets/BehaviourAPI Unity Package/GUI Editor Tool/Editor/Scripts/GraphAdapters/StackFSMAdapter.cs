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
        #region ---------------- Code generation ----------------

        //public override string CreateGraphLine(GraphAsset graphAsset, ScriptTemplate scriptTemplate, string graphName)
        //{
        //    string gName = base.CreateGraphLine(graphAsset, scriptTemplate, graphName);

        //    if (gName != null) scriptTemplate.AddUsingDirective(typeof(StackFSM).Namespace);
        //    return gName;
        //}

        //protected override void AddTransition(NodeAsset node, ScriptTemplate template, string graphName)
        //{
        //    if(node.Node is StackTransition transition)
        //    {
        //        var nodeName = !string.IsNullOrEmpty(node.Name) ? node.Name : transition.TypeName().ToLower();
        //        string typeName = transition.TypeName();

        //        var args = new List<string>();

        //        var sourceState = template.FindVariableName(node.Parents.FirstOrDefault()) ?? "null/*ERROR*/";
        //        args.Add(sourceState);

        //        var methodName = string.Empty;
        //        if (transition is PushTransition pushTransition)
        //        {
        //            var targetState = template.FindVariableName(node.Childs.FirstOrDefault()) ?? "null/*ERROR*/";
        //            args.Add(targetState);
        //            methodName = "CreatePushTransition";
        //        }
        //        else if(transition is PopTransition popTransition)
        //        {
        //            methodName = "CreatePopTransition";
        //        }

        //        if (transition.Perception != null)
        //        {
        //            var perceptionCode = GeneratePerceptionCode(transition.Perception, template);
        //            if (!string.IsNullOrEmpty(perceptionCode)) args.Add(perceptionCode);
        //        }

        //        if (transition.Action != null)
        //        {
        //            var actionCode = GenerateActionCode(transition.Action, template);
        //            if (!string.IsNullOrEmpty(actionCode)) args.Add(actionCode);
        //        }

        //        if (!transition.isPulled) args.Add("isPulled: false");

        //        template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.{methodName}({args.Join()})");
        //    }
        //    else
        //    {
        //        base.AddTransition(node, template, graphName);
        //    }
        //}

        #endregion

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

            if(nodeView.Node.Node is PushTransition)
            {
                nodeView.IconElement.Add(new Label("PUSH"));
                nodeView.IconElement.Enable();
            }
            else if (nodeView.Node.Node is PopTransition)
            {
                nodeView.IconElement.Add(new Label("POP"));
                nodeView.IconElement.Enable();
            }
        }

        #endregion
    }
}
