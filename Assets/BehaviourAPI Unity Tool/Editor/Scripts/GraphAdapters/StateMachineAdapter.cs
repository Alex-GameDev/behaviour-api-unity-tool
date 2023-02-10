using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
using BehaviourAPI.StateMachines.StackFSMs;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using ExitTransition = BehaviourAPI.StateMachines.ExitTransition;
using ProbabilisticState = BehaviourAPI.StateMachines.ProbabilisticState;
using State = BehaviourAPI.StateMachines.State;
using StateTransition = BehaviourAPI.StateMachines.StateTransition;
using Transition = BehaviourAPI.StateMachines.Transition;

namespace BehaviourAPI.Unity.Editor
{
    [CustomAdapter(typeof(FSM))]
    public class StateMachineAdapter : GraphAdapter
    {
        #region ------------------- Rendering -------------------

        NodeView _entryStateView;
        protected override List<Type> MainTypes => new List<Type>
        {
            typeof(State),
            typeof(StateTransition),
            typeof(ExitTransition)
        };

        protected override List<Type> ExcludedTypes => new List<Type> { 
            typeof(State), 
            typeof(ExitTransition), 
            typeof(StateTransition), 
            typeof(ProbabilisticState)
        };

        protected override void DrawGraphDetails(GraphAsset graphAsset, BehaviourGraphView graphView, List<NodeView> nodeViews)
        {
            var firstState = graphAsset.Nodes.FirstOrDefault(n => n.Node is State);
            if (firstState != null) ChangeEntryState(nodeViews.Find(n => n.Node == firstState));
        }

        protected override NodeView GetLayout(NodeAsset asset, BehaviourGraphView graphView) => new CyclicNodeView(asset, graphView);

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Set entry state", _ => ChangeEntryState(node), 
                _ => (node.Node != null && node.Node.Node is State) ? (node != _entryStateView).ToMenuStatus() : DropdownMenuAction.Status.Hidden);
        }

        protected override void SetUpDetails(NodeView nodeView)
        {
            var contents = nodeView.Q("contents");
            if (nodeView.Node.Node is Transition)
            {
                nodeView.Q("node-status").ChangeBorderColor(new Color(0,0,0,0));
                contents.style.width = 125;
                contents.ChangeBorderColor(new Color(.25f, .25f, .25f, .25f));
                contents.ChangeBackgroundColor(new Color(.15f, .15f, .15f, .4f));
            }
            else
            {
                contents.style.width = 200;
            }

            if (nodeView.Node.Node is ExitTransition)
            {
                var label = nodeView.RootElement.Q<Label>("node-root-label");
                label.text = "Exit";
                nodeView.RootElement.Q("node-root-tag").ChangeBackgroundColor(new Color(.8f, .3f, .3f));

                var statusLabel = new Label();
                statusLabel.Bind(new SerializedObject(nodeView.Node));
                statusLabel.bindingPath = "node.ExitStatus";

                nodeView.IconElement.Add(statusLabel);
                nodeView.IconElement.Enable();

                nodeView.RootElement.Enable();
            }
        }

        protected override GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change)
        {
            var rootNode = graphView.GraphAsset.Nodes.FirstOrDefault(n => n.Node is State);

            if (rootNode != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(rootNode);
                var view = graphView.nodes.Select(n => n as NodeView).ToList().Find(n => n.Node == rootNode);
                ChangeEntryState(view);
            }
            return change;
        }

        void ChangeEntryState(NodeView newStartNode)
        {
            if (newStartNode == null || newStartNode.Node.Node is not State) return;

            var graphView = newStartNode.GraphView;

            if (_entryStateView != null)
            {
                _entryStateView.RootElement.Disable();
            }

            _entryStateView = newStartNode;
            if (_entryStateView != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(_entryStateView.Node);
                _entryStateView.RootElement.Enable();
            }
        }

        protected override void SetUpGraphContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt)
        {
        }

        #endregion
    }
}
