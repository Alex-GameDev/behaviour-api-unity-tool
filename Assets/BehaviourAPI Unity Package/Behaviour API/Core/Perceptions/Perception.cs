using System;

namespace BehaviourAPI.Core.Perceptions
{
    /// <summary>
    /// Represent a task that a behaviour agent can perform to check some condition.
    /// </summary>
    public abstract class Perception : ICloneable
    {
        /// <summary>
        /// Initialize the perception. 
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Executes the perception. 
        /// </summary>
        public abstract bool Check();

        /// <summary>
        /// Reset the perception. 
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// Specifies the perception execution context
        /// </summary>
        /// <param name="context">The execution context.</param>
        public abstract void SetExecutionContext(ExecutionContext context);

        /// <summary>
        /// Create a shallow copy of the action.
        /// </summary>
        /// <returns>The perception copy.</returns>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}