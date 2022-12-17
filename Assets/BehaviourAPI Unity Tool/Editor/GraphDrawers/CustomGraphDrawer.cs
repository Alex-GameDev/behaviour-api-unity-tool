using BehaviourAPI.Unity.Runtime;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class CustomGraphDrawer
    {
        public abstract NodeView DrawNode(NodeAsset nodeAsset);
    }
}
