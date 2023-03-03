using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using UtilitySystems;
    public class CurveFunction : CurveFactor
    {
        public AnimationCurve curve;

        protected override float Evaluate(float childUtility)
        {
            return curve.Evaluate(childUtility);
        }  
        
        public CurveFunction SetCurve(AnimationCurve animationCurve)
        {
            curve = animationCurve;
            return this;
        }
    }
}
