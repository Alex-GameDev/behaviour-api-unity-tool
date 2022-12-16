using BehaviourAPI.Unity.Runtime;

namespace BehaviourAPI.Unity.Editor
{
    public class DefaultGraphDrawer : CustomGraphDrawer
    {
        public override NodeView DrawNode(NodeAsset node)
        {
            NodeView nodeView = new NodeView(node);
            return nodeView;
        }
    }
}
