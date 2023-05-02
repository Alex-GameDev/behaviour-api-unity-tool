using System;
using System.Linq.Expressions;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    using Core;
    using UnityExtensions;

    /// <summary>
    /// Data class to serialize a component method call.
    /// </summary>
    [Serializable]

    public class SerializedContextMethod : ICloneable
    {
        /// <summary>
        /// The name of the component. If is empty, the used component will be the behaviour runner.
        /// </summary>
        public string componentName;

        /// <summary>
        /// The name of the method called.
        /// </summary>
        public string methodName;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>
    /// Serialized method data that uses their fields to generate a delegate of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the delegate that stores the method.</typeparam>
    [Serializable]
    public class SerializedContextMethod<T> : SerializedContextMethod where T : Delegate
    {
        T _function;

        /// <summary>
        /// Get the generated function in set context method.
        /// </summary>
        /// <returns>The function.</returns>
        public T GetFunction() => _function;

        protected virtual Type[] FunctionArgs => new Type[0];

        /// <summary>
        /// Set value to <see cref="_function"/> using reflection to create a method call by <see cref="SerializedContextMethod.methodName"/> 
        /// and <see cref="SerializedContextMethod.componentName"/>.
        /// </summary>
        /// <param name="context">The context used to get the component reference.</param>
        public void SetContext(UnityExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(methodName)) return;

            Component component = string.IsNullOrWhiteSpace(componentName) ? context.RunnerComponent : context.GameObject.GetComponent(componentName);

            if (component != null)
            {
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
    /// Serialized method for void events.
    /// </summary>
    [Serializable]
    public class ContextualSerializedAction : SerializedContextMethod<Action>
    {
    }

    /// <summary>
    /// Serialized method for action update event.
    /// </summary>
    [Serializable]
    public class ContextualSerializedStatusFunction : SerializedContextMethod<Func<Status>>
    {
    }

    /// <summary>
    /// Serialized method for PerceptionReference check event.
    /// </summary>
    [Serializable]
    public class ContextualSerializedBoolFunction : SerializedContextMethod<Func<bool>>
    {
    }

    /// <summary>
    /// Serialized method for variable factor utility.
    /// </summary>
    [Serializable]
    public class ContextualSerializedFloatFunction : SerializedContextMethod<Func<float>>
    {
    }

    /// <summary>
    /// Serialized method for function factor utility.
    /// </summary>
    [Serializable]
    public class ContextualSerializedFloatFloatFunction : SerializedContextMethod<Func<float, float>>
    {
        protected override Type[] FunctionArgs => new Type[] { typeof(float) };
    }
}