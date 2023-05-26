namespace BehaviourAPI.UnityToolkit
{
    using Core;
    using Core.Perceptions;

    /// <summary>
    /// Perception type specific to the Unity environment.
    /// </summary>
    public abstract class UnityPerception : Perception
    {
        /// <summary>
        /// The execution context of the perception. Use it to get component references to the 
        /// object that executes the perception.
        /// </summary>
        protected UnityExecutionContext context;

        /// <summary>
        /// The info displayed in the editor window.
        /// <para>Use ${varname} to display the value of a field.</para>
        /// </summary>
        public virtual string DisplayInfo => "Unity Perception";

        public sealed override void SetExecutionContext(ExecutionContext context)
        {
            this.context = (UnityExecutionContext)context;
            OnSetContext();
        }

        /// <summary>
        /// Executes this method when the graph is getting the context.
        /// Override this method to use <see cref="context"/> to get component references.
        /// </summary>
        protected virtual void OnSetContext()
        {
            return;
        }
    }
}
