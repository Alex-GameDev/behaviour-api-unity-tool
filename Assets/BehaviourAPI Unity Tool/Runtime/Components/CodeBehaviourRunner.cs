using BehaviourAPI.Core;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class CodeBehaviourRunner : BehaviourRunner
    {
        BehaviourGraph _graph;

        protected abstract BehaviourGraph CreateGraph();

        protected override void OnAwake() => _graph = CreateGraph();
        protected override void OnStart() => _graph.Start();
        protected override void OnUpdate() => _graph.Update();

        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            throw new System.NotImplementedException();
        }
    }
}
