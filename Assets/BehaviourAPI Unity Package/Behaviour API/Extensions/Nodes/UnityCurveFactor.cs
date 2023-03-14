using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using UtilitySystems;

    /// <summary>
    /// Create the function using unity animation curve
    /// </summary>
    public class UnityCurveFactor : CurveFactor
    {
        public AnimationCurve curve;

        protected override float Evaluate(float childUtility)
        {
            return curve.Evaluate(childUtility);
        }

        public UnityCurveFactor SetCurve(AnimationCurve animationCurve)
        {
            curve = animationCurve;
            return this;
        }
    }
}
