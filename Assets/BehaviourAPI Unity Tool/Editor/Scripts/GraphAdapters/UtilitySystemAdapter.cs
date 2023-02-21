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
            typeof(FunctionFactor),
            typeof(VariableFactor),
            typeof(ContextVariableFactor)
        };
        public override List<Type> ExcludedTypes => new List<Type>
        {
            typeof(UtilitySystems.UtilityAction),
            typeof(UtilitySystems.PointedFunction),
            typeof(UtilitySystems.CustomFunction),
            typeof(UtilitySystems.VariableFactor)
        };

        protected override void DrawGraphDetails(GraphAsset graphAsset, BehaviourGraphView graphView, List<NodeView> nodeViews)
        {            
        }

        protected override NodeView GetLayout(NodeAsset asset, BehaviourGraphView graphView) => new LayeredNodeView(asset, graphView);

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Order childs by position (y)", _ => node.Node.OrderChilds(n => n.Position.y), (node.Node.Childs.Count > 1).ToMenuStatus());
        }

        protected override void SetUpDetails(NodeView nodeView)
        {
            var node = nodeView.Node.Node;
            if (node is UtilitySystems.VariableFactor)
            {
                if(node is VariableFactor vf)
                {
                    var obj = new SerializedObject(nodeView.Node);

                    Label componentLabel = new Label();
                    componentLabel.Bind(obj);                    
                    componentLabel.AddToClassList("node-text");
                    componentLabel.bindingPath = "node.variableFunction.component";
                    nodeView.extensionContainer.Add(componentLabel);

                    Label methodLabel = new Label();
                    methodLabel.Bind(obj);
                    methodLabel.AddToClassList("node-text");
                    methodLabel.bindingPath = "node.variableFunction.methodName";
                    nodeView.extensionContainer.Add(methodLabel);
                }
            }
            else
            {
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
            menuEvt.menu.AppendAction("Order all node's child by position (y)", _ => graph.GraphAsset.Nodes.ForEach(n => n.OrderChilds(n => n.Position.y)));
        }


        #endregion
    }
}
