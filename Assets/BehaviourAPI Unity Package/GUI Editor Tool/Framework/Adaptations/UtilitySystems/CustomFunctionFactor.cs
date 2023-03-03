using BehaviourAPI.UtilitySystems;
using System;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class CustomFunction : CurveFactor
    {
        public SerializedFloatFloatFunction function;

        protected override float Evaluate(float childUtility) => function.GetFunction()?.Invoke(childUtility) ?? 0f;
    }
}
