using BehaviourAPI.Unity.Framework;
using BehaviourAPI.UtilitySystems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using UtilityAction = BehaviourAPI.Unity.Framework.Adaptations.UtilityAction;
using CustomFunction = BehaviourAPI.Unity.Framework.Adaptations.CustomFunction;
using VariableFactor = BehaviourAPI.Unity.Framework.Adaptations.VariableFactor;
using UnityEditor.UIElements;
using BehaviourAPI.Unity.Framework.Adaptations;

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
        };
        public override List<Type> ExcludedTypes => new List<Type>
        {
            typeof(UtilitySystems.UtilityAction),
            typeof(UtilitySystems.PointedCurveFactor),
            typeof(UtilitySystems.CustomCurveFactor),
            typeof(UtilitySystems.VariableFactor)
        };

        protected override void DrawGraphDetails(GraphData graphAsset, BehaviourGraphView graphView)
        {            
        }

        protected override NodeView GetLayout(NodeData asset, BehaviourGraphView graphView) => new LayeredNodeView(asset, graphView);

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Order childs by position (y)",
                _ => node.GraphView.graphData.OrderChildNodes(node.Node, (n) => n.position.y),
                (node.Node.childIds.Count > 1).ToMenuStatus()
            );
        }

        protected override void SetUpDetails(NodeView nodeView)
        {
            var node = nodeView.Node.node;
            if (node is UtilitySystems.VariableFactor)
            {
                //nodeView.ChangeTypeColor(BehaviourAPISettings.instance.LeafFactorColor);
                //if (node is VariableFactor vf)
                //{
                //    var obj = new SerializedObject(nodeView.Node);

                //    Label componentLabel = new Label();
                //    componentLabel.Bind(obj);                    
                //    componentLabel.AddToClassList("node-text");
                //    componentLabel.bindingPath = "node.variableFunction.component";
                //    nodeView.extensionContainer.Add(componentLabel);

                //    Label methodLabel = new Label();
                //    methodLabel.Bind(obj);
                //    methodLabel.AddToClassList("node-text");
                //    methodLabel.bindingPath = "node.variableFunction.methodName";
                //    nodeView.extensionContainer.Add(methodLabel);
                //}
            }
            else
            {
                if(node is CurveFactor) nodeView.ChangeTypeColor(BehaviourAPISettings.instance.CurveFactorColor);
                else if(node is FusionFactor) nodeView.ChangeTypeColor(BehaviourAPISettings.instance.FusionFactorColor);
                else if(node is UtilityExecutableNode) nodeView.ChangeTypeColor(BehaviourAPISettings.instance.SelectableNodeColor);
                else if(node is UtilityBucket) nodeView.ChangeTypeColor(BehaviourAPISettings.instance.BucketColor);

                nodeView.IconElement.Enable();
                if(node is Factor) nodeView.IconElement.Add(new Label(node.TypeName().CamelCaseToSpaced().Split().First().ToUpper()));
                else nodeView.IconElement.Add(new Label(node.TypeName().CamelCaseToSpaced().ToUpper()));
            }

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

        protected override void SetUpGraphContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Order childs by position (y)", _ =>
            {
                graph.graphData.OrderAllChildNodes((n) => n.position.y);
                BehaviourEditorWindow.Instance.RegisterChanges();
            });
        }

        #endregion
    }
}
