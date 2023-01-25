using BehaviourAPI.Unity.Framework.Adaptations;

namespace BehaviourAPI.Unity.Editor
{
    public class CompoundPerceptionView : PerceptionView<CompoundPerception>
    {
        public CompoundPerceptionView(CompoundPerception perception) : base(perception, BehaviourAPISettings.instance.CompoundPerceptionLayout)
        {
        }

        protected override void AddLayout()
        {

        }
    }
}
