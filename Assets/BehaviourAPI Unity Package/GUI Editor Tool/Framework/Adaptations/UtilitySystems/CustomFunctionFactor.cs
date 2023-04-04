namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using UnityEngine;
    using UtilitySystems;
    using UnityExtensions;

    /// <summary>
    /// Adaptation wrapper class for use <see cref="CustomCurveFactor"/> in editor tools. 
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    [NodeAdapter(typeof(CustomCurveFactor))]
    public class CustomFunction : CurveFactor
    {
        /// <summary>
        /// Method reference for <see cref="CustomCurveFactor.Function"/>.
        /// </summary>
        public ContextualSerializedFloatFloatFunction function;

        /// <summary>
        /// <inheritdoc/>
        /// Build the <see cref="function"/> delegate with the context.
        /// </summary>
        /// <param name="context"><inheritdoc/></param>
        public override void SetExecutionContext(ExecutionContext context)
        {
            var unityContext = (UnityExecutionContext)context;
            if (unityContext == null)
            {
                function.SetContext(unityContext);
            }
            else
            {
                Debug.LogError("Context Function factor need an UnityExecutionContext to work");
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// Use <see cref="function"/> internal delegate to modify the utility.
        /// </summary>
        /// <param name="childUtility"><inheritdoc/></param>
        /// <returns>The result of invoke <see cref="function"/> deledate. </returns>
        protected override float Evaluate(float childUtility) => function.GetFunction()?.Invoke(childUtility) ?? 0f;
    }
}
