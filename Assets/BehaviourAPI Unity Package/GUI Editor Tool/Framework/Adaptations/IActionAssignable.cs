using BehaviourAPI.Core.Actions;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    /// <summary>
    /// Allows API tools to know what nodes has an <see cref="Action"/> field.
    /// </summary>
    public interface IActionAssignable
    {
        /// <summary>
        /// The property that points to the <see cref="Action"/> of the node.
        /// </summary>
        public Action ActionReference { get; set; }
    }
}
