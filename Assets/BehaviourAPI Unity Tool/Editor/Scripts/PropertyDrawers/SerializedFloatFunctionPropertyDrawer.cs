using BehaviourAPI.Unity.Runtime;
using System.Reflection;
using UnityEditor;

namespace BehaviourAPI.Unity.Editor
{
    [CustomPropertyDrawer(typeof(SerializedFloatFunction))]
    public class SerializedFloatFunctionPropertyDrawer : CustomActionPropertyDrawer
    {
        protected override bool ValidateMethod(MethodInfo methodInfo)
        {
            return methodInfo.ReturnParameter.ParameterType == typeof(float) &&
               methodInfo.GetParameters().Length == 0;
        }
    }

    [CustomPropertyDrawer(typeof(SerializedFloatFloatFunction))]
    public class SerializedFloatFloatFunctionPropertyDrawer : CustomActionPropertyDrawer
    {
        protected override bool ValidateMethod(MethodInfo methodInfo)
        {
            return methodInfo.ReturnParameter.ParameterType == typeof(float) &&
               methodInfo.GetParameters().Length == 1 && 
               methodInfo.GetParameters()[0].ParameterType == typeof(float);
        }
    }
}
