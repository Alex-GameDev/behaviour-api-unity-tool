using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using UnityEngine;
    using UtilitySystems;
    using UnityExtensions;

    public class CustomFunction : CurveFactor
    {
        public ContextualSerializedFloatFloatFunction function;

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

        protected override float Evaluate(float childUtility) => function.GetFunction()?.Invoke(childUtility) ?? 0f;
    }
}
