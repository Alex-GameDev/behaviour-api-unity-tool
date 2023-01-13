using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class CurveFunctionFactor : UtilitySystems.FunctionFactor
    {
        [SerializeField] AnimationCurve curve;

        protected override float Evaluate(float childUtility)
        {
            return Mathf.Clamp01(curve.Evaluate(childUtility));
        }       
    }
}
