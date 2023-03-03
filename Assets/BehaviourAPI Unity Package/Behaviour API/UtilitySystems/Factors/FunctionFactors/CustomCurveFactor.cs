using System;

namespace BehaviourAPI.UtilitySystems
{
    public class CustomCurveFactor : CurveFactor
    {
        public Func<float, float> Function;

        public CustomCurveFactor SetFunction(Func<float, float> function)
        {
            Function = function;
            return this;
        }

        protected override float Evaluate(float x) => Function?.Invoke(x) ?? x;
    }
}
