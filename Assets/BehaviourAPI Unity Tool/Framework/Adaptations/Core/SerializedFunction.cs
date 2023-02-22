using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;


namespace BehaviourAPI.Unity.Framework.Adaptations
{
    [Serializable]
    public class SerializedMethod
    {
        public Component component;
        public string methodName;
    }

    [Serializable]
    public class SerializedMethod<T> : SerializedMethod, ISerializationCallbackReceiver where T : Delegate
    {
        T _function;

        public T GetFunction() => _function;

        protected virtual Type[] FunctionArgs => new Type[0];
        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(methodName) && component != null)
            {
                Type[] arguments = FunctionArgs;
                var method = component.GetType().GetMethod(methodName, arguments);

                if (method == null)
                {
                    methodName = "";
                    _function = null;
                }
                else if (_function == null)
                {
                    ParameterExpression[] parameters = arguments.Select(type => Expression.Parameter(type)).ToArray();
                    if(method != null)
                    {
                        ConstantExpression componentExpression = Expression.Constant(component);
                        MethodCallExpression methodCall = Expression.Call(componentExpression, method, parameters);
                        Expression<T> exp = Expression.Lambda<T>(methodCall, parameters);
                        _function = exp.Compile();
                    }
                    else
                    {
                        Debug.LogWarning("Deserialization error: The selected method parameters don't match the function");
                    }
                }
            }
        }

        public void OnBeforeSerialize()
        {
            return;
        }
    }

    /// <summary>
    /// Serialized method for start and stop events
    /// </summary>
    [Serializable]
    public class SerializedAction : SerializedMethod<Action> 
    { 
    }

    /// <summary>
    /// Serialized method for action update event
    /// </summary>
    [Serializable]
    public class SerializedStatusFunction : SerializedMethod<Func<Status>> { }

    /// <summary>
    /// Serialized method for PerceptionReference check event
    /// </summary>
    [Serializable]
    public class SerializedBoolFunction : SerializedMethod<Func<bool>> { }

    /// <summary>
    /// Serialized method for variable factor utility computing
    /// </summary>
    [Serializable]
    public class SerializedFloatFunction : SerializedMethod<Func<float>> { }

    /// <summary>
    /// Serialized method for function factor utility computing
    /// </summary>
    [Serializable]
    public class SerializedFloatFloatFunction : SerializedMethod<Func<float,float>> 
    {
        protected override Type[] FunctionArgs => new Type[] { typeof(float) };
    }

    /// <summary>
    /// Serialized method for start and stop events with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedAction : SerializedMethod<Action<UnityExecutionContext>>
    {
        protected override Type[] FunctionArgs => new Type[] { typeof(UnityExecutionContext) };
    }

    /// <summary>
    /// Serialized method for action update event with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedStatusFunction : SerializedMethod<Func<UnityExecutionContext, Status>>
    {
        protected override Type[] FunctionArgs => new Type[] { typeof(UnityExecutionContext) };
    }

    /// <summary>
    /// Serialized method for PerceptionReference check event with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedBoolFunction : SerializedMethod<Func<UnityExecutionContext, bool>> { }

    /// <summary>
    /// Serialized method for variable factor utility computing with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedFloatFunction : SerializedMethod<Func<UnityExecutionContext, float>>
    {
        protected override Type[] FunctionArgs => new Type[] { typeof(UnityExecutionContext) };
    }

    /// <summary>
    /// Serialized method for function factor utility computing with context
    /// </summary>
    [Serializable]
    public class ContextualSerializedFloatFloatFunction : SerializedMethod<Func<UnityExecutionContext, float, float>>
    {
        protected override Type[] FunctionArgs => new Type[] { typeof(UnityExecutionContext), typeof(float) };
    }

}
