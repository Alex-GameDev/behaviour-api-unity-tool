using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class VariableFactor : UtilitySystems.VariableFactor
    {
        public SerializedFloatFunction variableFunction;

        protected override float ComputeUtility()
        {
            Utility = variableFunction.GetFunction()?.Invoke() ?? min;
            Utility = (Utility - min) / (max - min);
            return Mathf.Clamp01(Utility);
        }
    }

    [Serializable]
    public class SerializedFloatFunction : SerializedMethod<Func<float>> { }
}
