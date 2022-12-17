using BehaviourAPI.Unity.Runtime;

namespace BehaviourAPI.Unity.Editor
{
    public class DefaultGraphDrawer : CustomGraphDrawer
    {
        public override NodeView DrawNode(NodeAsset node)
        {
            DefaultNodeView nodeView = new DefaultNodeView(node);
            return nodeView;
        }
    }
}
