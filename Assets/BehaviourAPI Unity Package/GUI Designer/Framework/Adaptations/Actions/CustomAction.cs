using UnityEngine;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Framework
{
    using Core;
    using Core.Actions;
    using UnityToolkit;

    /// <summary>
    /// Adaptation class for use custom <see cref="FunctionalAction"/> in editor tools.
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    public class CustomAction : FunctionalAction
    {
        /// <summary>
        /// Method reference for start event.
        /// </summary>
        public ContextualSerializedAction start;

        /// <summary>
        /// Method reference for update event.
        /// </summary>
        public ContextualSerializedStatusFunction update;

        /// <summary>
        /// Method reference for stop event.
        /// </summary>
        public ContextualSerializedAction stop;

        /// <summary>
        /// Method reference for update event.
        /// </summary>
        public ContextualSerializedAction pause;

        /// <summary>
        /// Method reference for stop event.
        /// </summary>
        public ContextualSerializedAction unpause;

        /// <summary>
        /// <inheritdoc/>
        /// Build the delegates using <paramref name="context"/> and the method references.
        /// </summary>
        /// <param name="context"><inheritdoc/></param>
        public override void SetExecutionContext(ExecutionContext context)
        {
            var unityContext = (UnityExecutionContext)context;
            if (unityContext != null && unityContext.RunnerComponent != null)
            {
                onStarted = start.CreateDelegate(unityContext.RunnerComponent);
                onUpdated = update.CreateDelegate(unityContext.RunnerComponent);
                onStopped = stop.CreateDelegate(unityContext.RunnerComponent);
                onPaused = pause.CreateDelegate(unityContext.RunnerComponent);
                onUnpaused = unpause.CreateDelegate(unityContext.RunnerComponent);
            }
            else
            {
                Debug.LogError("Context action need an UnityExecutionContext with a runner component to work");
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// Copy the method references too.
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override object Clone()
        {
            var copy = (CustomAction)base.Clone();
            copy.start = (ContextualSerializedAction)start?.Clone();
            copy.update = (ContextualSerializedStatusFunction)update?.Clone();
            copy.stop = (ContextualSerializedAction)stop?.Clone();
            copy.pause = (ContextualSerializedAction)pause?.Clone();
            copy.unpause = (ContextualSerializedAction)pause?.Clone();
            return copy;
        }
    }
}
