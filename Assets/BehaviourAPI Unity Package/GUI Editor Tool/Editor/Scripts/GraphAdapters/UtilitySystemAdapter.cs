using BehaviourAPI.Unity.Framework;
using BehaviourAPI.UtilitySystems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using UtilityAction = BehaviourAPI.Unity.Framework.Adaptations.UtilityAction;
using CustomFunction = BehaviourAPI.Unity.Framework.Adaptations.CustomFunction;
using VariableFactor = BehaviourAPI.Unity.Framework.Adaptations.VariableFactor;

namespace BehaviourAPI.Unity.Editor
{
    [CustomAdapter(typeof(UtilitySystem))]
    public class UtilitySystemAdapter : GraphAdapter
    {

        #region ------------------- Rendering -------------------

        public override List<Type> MainTypes => new List<Type>
        {
            typeof(UtilityAction),
            typeof(UtilityBucket),
            typeof(UtilityExitNode),
            typeof(FusionFactor),
            typeof(CurveFactor),
            typeof(VariableFactor),
            typeof(ConstantFactor)
        };
        public override List<Type> ExcludedTypes => new List<Type>
        {
            typeof(UtilitySystems.UtilityAction),
            typeof(UtilitySystems.PointedCurveFactor),
            typeof(UtilitySystems.CustomCurveFactor),
            typeof(UtilitySystems.VariableFactor)
        };

        protected override NodeView GetLayout(NodeData asset, BehaviourGraphView graphView) => new LayeredNodeView(asset, graphView);

        protected override void DrawGraphDetails(GraphData graphAsset, BehaviourGraphView graphView)
        {            
        }

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Order childs by position (y)", _ =>
            {
                node.GraphView.graphData.OrderChildNodes(node.Node, (n) => n.position.y);
                BehaviourEditorWindow.Instance.RegisterChanges();
                node.UpdateEdgeViews();
            },
                (node.Node.childIds.Count > 1).ToMenuStatus()
            );
        }

        protected override void SetUpGraphContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Order childs by position (y)", _ =>
            {
                graph.graphData.OrderAllChildNodes((n) => n.position.y);
                BehaviourEditorWindow.Instance.RegisterChanges();
                graph.nodeViews.ForEach(n => n.UpdateEdgeViews());
            });
        }

        protected override void DrawNodeDetails(NodeView nodeView)
        {
            var node = nodeView.Node.node;
            if (node is UtilitySystems.VariableFactor) nodeView.ChangeTypeColor(BehaviourAPISettings.instance.LeafFactorColor);
            else if(node is CurveFactor) nodeView.ChangeTypeColor(BehaviourAPISettings.instance.CurveFactorColor);
            else if(node is FusionFactor) nodeView.ChangeTypeColor(BehaviourAPISettings.instance.FusionFactorColor);
            else if(node is UtilityExecutableNode) nodeView.ChangeTypeColor(BehaviourAPISettings.instance.SelectableNodeColor);
            else if(node is UtilityBucket) nodeView.ChangeTypeColor(BehaviourAPISettings.instance.BucketColor);

            nodeView.IconElement.Enable();
            if(node is Factor) nodeView.IconElement.Add(new Label(node.TypeName().CamelCaseToSpaced().Split().First().ToUpper()));
            else nodeView.IconElement.Add(new Label(node.TypeName().CamelCaseToSpaced().ToUpper()));

            if(nodeView.GraphView.Runtime)
            {
                if(node is UtilityNode utilityHandler)
                {
                    var utilityBar = new ProgressBar()
                    {
                        title = " ",
                        lowValue = 0,
                        highValue = 1,
                    };

                    utilityHandler.UtilityChanged += (value) =>
                    {
                        utilityBar.value = value;
                        utilityBar.title = value.ToString("0.000");
                    };

                    utilityBar.value = utilityHandler.Utility;
                    utilityBar.title = utilityHandler.Utility.ToString("0.000");
                    nodeView.extensionContainer.Add(utilityBar);
                }
            }
        }

        protected override GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change)
        {
            return change;
        }


        #endregion
    }

    public class LayeredNodeView : NodeView
    {
        public LayeredNodeView(NodeData node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorLayoutsPath + "/Nodes/DAG Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/DAG Node.uxml";

        public override void SetUpPorts()
        {
            if (Node.node == null || Node.node.MaxInputConnections != 0)
            {
                var port = InstantiatePort(Direction.Input, PortOrientation.Right);
            }
            else
            {
                inputContainer.style.display = DisplayStyle.None;
            }

            if (Node.node == null || Node.node.MaxOutputConnections != 0)
            {
                var port = InstantiatePort(Direction.Output, PortOrientation.Left);
            }
            else
                outputContainer.style.display = DisplayStyle.None;
        }
    }
}
