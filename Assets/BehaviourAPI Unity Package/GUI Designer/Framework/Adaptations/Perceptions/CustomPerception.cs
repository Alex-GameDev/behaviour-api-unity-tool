using UnityEngine;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Framework
{
    using Core;
    using Core.Perceptions;
    using UnityToolkit;

    /// <summary>
    /// Adaptation class for use custom <see cref="ConditionPerception"/> in editor tools.
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    public class CustomPerception : ConditionPerception
    {
        /// <summary>
        /// Method reference for init event.
        /// </summary>
        public ContextualSerializedAction init;

        /// <summary>
        /// Method reference for check event.
        /// </summary>
        public ContextualSerializedBoolFunction check;

        /// <summary>
        /// Method reference for reset event.
        /// </summary>
        public ContextualSerializedAction reset;

        /// <summary>
        /// Method reference for reset event.
        /// </summary>
        public ContextualSerializedAction pause;

        /// <summary>
        /// Method reference for reset event.
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
                onInit = init.CreateDelegate(unityContext.RunnerComponent);
                onCheck = check.CreateDelegate(unityContext.RunnerComponent);
                onReset = reset.CreateDelegate(unityContext.RunnerComponent);
                onPause = pause.CreateDelegate(unityContext.RunnerComponent);
                onUnpause = unpause.CreateDelegate(unityContext.RunnerComponent);
            }
            else
            {
                Debug.LogError("Context perception need an UnityExecutionContext to work");
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// Copy the method references too.
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override object Clone()
        {
            var copy = (CustomPerception)base.Clone();
            copy.init = (ContextualSerializedAction)init?.Clone();
            copy.check = (ContextualSerializedBoolFunction)check?.Clone();
            copy.reset = (ContextualSerializedAction)reset?.Clone();
            copy.pause = (ContextualSerializedAction)pause?.Clone();
            copy.unpause = (ContextualSerializedAction)unpause?.Clone();
            return copy;
        }
    }
}
