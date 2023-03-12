using BehaviourAPI.Core.Perceptions;

namespace behaviourAPI.Unity.Framework.Adaptations
{
    public interface IPerceptionAssignable
    {
        public Perception PerceptionReference { get; set; }
    }
}
