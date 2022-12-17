namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.Core.Perceptions;
    using BehaviourAPI.Unity.Runtime;
    using Core;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using Vector2 = UnityEngine.Vector2;

    /// <summary>
    /// Visual element that represents a node in a behaviour graph
    /// </summary>
    public abstract class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public NodeAsset Node { get; set; }

        public Action<NodeAsset> Selected = delegate { };

        public List<Port> InputPorts;
        public List<Port> OutputPorts;

        public NodeView(NodeAsset node, string layout) : base(layout)
        {
            Node = node;
            InputPorts = new List<Port>();
            OutputPorts = new List<Port>();
            SetPosition(new UnityEngine.Rect(node.Position, Vector2.zero));
            AddLayout();
            DrawPorts();
            DrawExtensionContainer();
            styleSheets.Add(VisualSettings.GetOrCreateSettings().NodeStylesheet);
            SetUpDataBinding();
        }

        protected abstract void AddLayout();
        protected abstract void DrawPorts();
        protected abstract void DrawExtensionContainer();

        public override void OnSelected()
        {
            base.OnSelected();
            Selected?.Invoke(Node);
        }

        public void OnConnected(Direction direction, NodeView other)
        {
            if(direction == Direction.Input)
                Node.Parents.Add(other.Node);
            else
                Node.Childs.Add(other.Node);
        }

        public void OnMoved(Vector2 pos)
        {
            Node.Position = pos;
        }

        public void OnDisconnected(Direction direction, NodeView other)
        {
            if (direction == Direction.Input)
                Node.Parents.Remove(other.Node);
            else
                Node.Childs.Remove(other.Node);           
        }

        void SetUpDataBinding()
        {
            var titleInputField = this.Q<TextField>(name: "title-input-field");
            titleInputField.bindingPath = "Name";
            titleInputField.Bind(new SerializedObject(Node));
        }
    }
}