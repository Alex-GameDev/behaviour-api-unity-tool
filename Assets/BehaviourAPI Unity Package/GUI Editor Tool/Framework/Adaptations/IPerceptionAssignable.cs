using BehaviourAPI.Core.Perceptions;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    /// <summary>
    /// Allows API tools to know what nodes has a <see cref="Perception"/> field.
    /// </summary>
    public interface IPerceptionAssignable
    {
        /// <summary>
        /// The property that points to the <see cref="Perception"/> of the node.
        /// </summary>
        public Perception PerceptionReference { get; set; }
    }
}
