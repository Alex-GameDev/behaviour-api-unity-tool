namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Unity.Runtime;
    using Core;
    using UnityEditor;
    using Vector2 = UnityEngine.Vector2;

    /// <summary>
    /// Visual element that represents a node in a behaviour graph
    /// </summary>
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public NodeAsset Node;
        public static string NODE_LAYOUT => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

        public NodeView(NodeAsset node) : base(NODE_LAYOUT)
        {
            Node = node;
            SetPosition(new UnityEngine.Rect(node.Position, Vector2.zero));
        }
    }
}