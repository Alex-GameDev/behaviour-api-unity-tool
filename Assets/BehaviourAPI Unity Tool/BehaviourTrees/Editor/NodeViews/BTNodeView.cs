using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Displays a Behaviour tree node in a graph view
    /// </summary>
    public class BTNodeView : NodeView
    {
        public static string NODE_LAYOUT => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().BTNodeLayout);

        VisualElement portContainer;
        VisualElement actionContainer;
        public BTNodeView(NodeAsset node) : base(node, NODE_LAYOUT)
        { 
        }

        protected override void DrawPorts()
        {
            if (Node.Node.MaxInputConnections != 0)
            {
                var capacity = Node.Node.MaxInputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = InstantiatePort(Orientation.Vertical, Direction.Input, capacity, Node.Node.GetType());
                port.portName = "";
                port.style.flexDirection = FlexDirection.Column;
                port.style.position = Position.Absolute;
                port.style.top = new StyleLength(new Length(5, LengthUnit.Pixel));
                portContainer.Add(port);
                InputPorts.Add(port);
            }

            if (Node.Node.MaxOutputConnections != 0)
            {
                var capacity = Node.Node.MaxOutputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = InstantiatePort(Orientation.Vertical, Direction.Output, capacity, Node.Node.ChildType);
                port.portName = "";
                port.style.flexDirection = FlexDirection.ColumnReverse;
                port.style.position = Position.Absolute;
                port.style.bottom = new StyleLength(new Length(5, LengthUnit.Pixel));
                portContainer.Add(port);
                OutputPorts.Add(port);
            }
        }

        protected override void DrawExtensionContainer()
        {

        }

        protected override void AddLayout()
        {
            portContainer = this.Q("node-port-container");
            actionContainer = this.Q("ac-container");
        }
    }
}
