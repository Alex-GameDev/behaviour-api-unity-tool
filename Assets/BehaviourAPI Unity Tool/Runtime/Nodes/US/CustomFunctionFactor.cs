using BehaviourAPI.UtilitySystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class CustomFunction : FunctionFactor
    {
        public SerializedFloatFloatFunction function;

        protected override float Evaluate(float childUtility) => function.GetFunction()?.Invoke(childUtility) ?? 0f;
    }

    [Serializable]
    public class SerializedFloatFloatFunction : SerializedMethod<Func<float,float>> { }
}
