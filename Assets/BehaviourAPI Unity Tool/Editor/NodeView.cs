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
            int numberOfInputPorts = Node.Node.MaxInputConnections != -1 ?
                Node.Node.MaxInputConnections : Node.Parents.Count * 2 + 1;

            int numberOfOutputPorts = Node.Node.MaxOutputConnections != -1 ?
                Node.Node.MaxOutputConnections : Node.Childs.Count * 2 + 1;

            for (int i = 0; i < numberOfInputPorts; i++)
                InsertPort(Direction.Input, i);

            for (int i = 0; i < numberOfOutputPorts; i++)
                InsertPort(Direction.Output, i);
        }

        void InsertPort(Direction direction, int index)
        {
            var portType = direction == Direction.Input ? Node.Node.GetType() : Node.Node.ChildType;
            var port = InstantiatePort(Orientation.Vertical, direction, Port.Capacity.Single, portType);
            port.portName = "";
            var container = direction == Direction.Input ? inputContainer : outputContainer;
            container.Insert(index, port);
        }

        void DeletePort(Direction direction, int index)
        {
            var container = direction == Direction.Input ? inputContainer : outputContainer;
            container.RemoveAt(index);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            Selected?.Invoke(Node);
        }

        public void OnConnected(Direction direction, int portIndex, NodeView other)
        {
            if(direction == Direction.Input)
                Node.Parents.Insert(portIndex / 2, other.Node);
            else
                Node.Childs.Insert(portIndex / 2, other.Node);

            var capacity = direction == Direction.Input ? Node.Node.MaxInputConnections : Node.Node.MaxOutputConnections;
            if (capacity == -1)
            {
                InsertPort(direction, portIndex + 1);
                InsertPort(direction, portIndex);
            }
        }

        public void OnDisconnected(Direction direction, int portIndex)
        {
            if (direction == Direction.Input)
                Node.Parents.RemoveAt(portIndex / 2);
            else
                Node.Childs.RemoveAt(portIndex / 2);

            var capacity = direction == Direction.Input ? Node.Node.MaxInputConnections : Node.Node.MaxOutputConnections;
            var currentConnections = direction == Direction.Input ? Node.Parents.Count : Node.Childs.Count;
            if(capacity == -1 && currentConnections > 0)
            {
                DeletePort(direction, portIndex - 1);
                DeletePort(direction, portIndex);
            }           
        }
    }
}