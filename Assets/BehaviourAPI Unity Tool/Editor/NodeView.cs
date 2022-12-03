namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Unity.Runtime;
    using Core;
    using System;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using Vector2 = UnityEngine.Vector2;

    /// <summary>
    /// Visual element that represents a node in a behaviour graph
    /// </summary>
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public NodeAsset Node { get; set; }

        public Action<NodeAsset> Selected = delegate { };

        public static string NODE_LAYOUT => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

        public NodeView(NodeAsset node) : base(NODE_LAYOUT)
        {
            Node = node;
            SetPosition(new UnityEngine.Rect(node.Position, Vector2.zero));
            DrawPorts();
        }

        void DrawPorts()
        {
            if (Node == null) return;

            int numberOfInputPorts = Node.Node.MaxInputConnections != -1 ?
                Node.Node.MaxInputConnections : Node.Parents.Count * 2 + 1;

            int numberOfOutputPorts = Node.Node.MaxOutputConnections != -1 ?
                Node.Node.MaxOutputConnections : Node.Parents.Count * 2 + 1;

            for(int i = 0; i < numberOfInputPorts; i++)
                InsertPort(Direction.Input, i, Node.Node.GetType());

            for(int i = 0; i < numberOfOutputPorts; i++)
                InsertPort(Direction.Output, i, Node.Node.ChildType);
        }

        void InsertPort(Direction direction, int index, Type portType)
        {
            var port = InstantiatePort(Orientation.Vertical, direction, Port.Capacity.Single, portType);
            port.portName = "";
            var container = direction == Direction.Input ? inputContainer : outputContainer;
            container.Insert(index, port);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            Selected?.Invoke(Node);
        }


    }
}