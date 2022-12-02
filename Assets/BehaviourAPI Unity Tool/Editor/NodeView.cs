namespace BehaviourAPI.Unity.Editor
{
    using Core;
    /// <summary>
    /// Visual element that represents a node in a behaviour graph
    /// </summary>
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Node Node;

        public NodeView(Node node) : base("")
        {
            Node = node;
        }
    }
}