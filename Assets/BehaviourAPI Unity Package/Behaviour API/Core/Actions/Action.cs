using System;

namespace BehaviourAPI.Core.Actions
{
    /// <summary>
    /// Represent a task that a behaviour agent can perform.
    /// </summary>
    public abstract class Action : ICloneable
    {
        /// <summary>
        /// Initialize the action. 
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Executes the action. 
        /// </summary>
        public abstract Status Update();

        /// <summary>
        /// Reset the action. 
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Specifies the action execution context
        /// </summary>
        /// <param name="context">The execution context.</param>
        public abstract void SetExecutionContext(ExecutionContext context);

        /// <summary>
        /// Create a shallow copy of the action.
        /// </summary>
        /// <returns>The action copy.</returns>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}