using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityExtensions;

namespace BehaviourAPI.Unity.Framework
{
    [Serializable]
    public class SerializedContextMethod : ICloneable
    {
        public string componentName;
        public string methodName;

        public object Clone()
        {
            var copy = (SerializedContextMethod)MemberwiseClone();
            return copy;
        }
    }

    [Serializable]
    public class SerializedContextMethod<T> : SerializedContextMethod where T : Delegate
    {
        T _function;
        public T GetFunction() => _function;

        protected virtual Type[] FunctionArgs => new Type[0];

        public void SetContext(UnityExecutionContext context)
        {
            if (!string.IsNullOrEmpty(componentName) && !string.IsNullOrEmpty(methodName))
            {
                var component = context.GameObject.GetComponent(componentName);
                if (component != null)
                {
                    Type[] arguments = FunctionArgs;
                    var method = component.GetType().GetMethod(methodName, arguments);
                    ParameterExpression[] parameters = arguments.Select(type => Expression.Parameter(type)).ToArray();
                    if (method != null)
                    {
                        ConstantExpression componentExpression = Expression.Constant(component);
                        MethodCallExpression methodCall = Expression.Call(componentExpression, method, parameters);
                        Expression<T> exp = Expression.Lambda<T>(methodCall, parameters);
                        _function = exp.Compile();
                    }
                    else
                    {
                        Debug.LogWarning("Custom function error: The specified method or component name is not valid.", context.GameObject);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Serialized method for start and stop events with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedAction : SerializedContextMethod<Action>
    {
    }

    /// <summary>
    /// Serialized method for action update event with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedStatusFunction : SerializedContextMethod<Func<Status>>
    {
    }

    /// <summary>
    /// Serialized method for PerceptionReference check event with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedBoolFunction : SerializedContextMethod<Func<bool>>
    {
    }

    /// <summary>
    /// Serialized method for variable factor utility computing with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedFloatFunction : SerializedContextMethod<Func<float>>
    {
    }

    /// <summary>
    /// Serialized method for function factor utility computing with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedFloatFloatFunction : SerializedContextMethod<Func<float, float>>
    {
        protected override Type[] FunctionArgs => new Type[] { typeof(float) };
    }
}