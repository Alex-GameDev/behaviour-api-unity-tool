using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public class CurveFunction : UtilitySystems.FunctionFactor
    {
        public AnimationCurve curve;

        protected override float Evaluate(float childUtility)
        {
            return Mathf.Clamp01(curve.Evaluate(childUtility));
        }  
        
        public CurveFunction SetCurve(AnimationCurve animationCurve)
        {
            curve = animationCurve;
            return this;
        }
    }
}
