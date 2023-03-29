using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using UnityExtensions;
    /// <summary>
    /// Adaptation wrapper class for use <see cref="UtilitySystems.VariableFactor"/> in editor tools. 
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    public class VariableFactor : UtilitySystems.VariableFactor
    {

        /// <summary>
        /// Method reference for <see cref="UtilitySystems.VariableFactor.Variable"/>.
        /// </summary>
        public ContextualSerializedFloatFunction variableFunction;

        /// <summary>
        /// <inheritdoc/>
        /// Build the <see cref="variableFunction"/> delegate with the context.
        /// </summary>
        /// <param name="context"><inheritdoc/></param>
        public override void SetExecutionContext(ExecutionContext context)
        {
            var unityContext = (UnityExecutionContext)context;
            if (unityContext != null)
            {
                variableFunction.SetContext(unityContext);
                Variable = variableFunction.GetFunction();
            }
            else
            {
                Debug.LogError("Context Variable factor need an UnityExecutionContext to work");
            }
        }
    }
}
