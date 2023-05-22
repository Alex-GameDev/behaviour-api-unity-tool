using System;
using System.Linq.Expressions;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    using Core;
    using System.Reflection;
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

        public Delegate GetDelegate(Component defaultComponent, Type[] arguments, Type delegateType)
        {
            if (string.IsNullOrWhiteSpace(methodName) || defaultComponent == null) return null;
            if (!delegateType.IsSubclassOf(typeof(Delegate))) return null;

            Component component = string.IsNullOrWhiteSpace(componentName) ? defaultComponent : defaultComponent.gameObject.GetComponent(componentName);

            if (component == null)
            {
                Debug.LogWarning($"BUILD ERROR: The specified component ({componentName}) does not exist or is not attached to runner.", defaultComponent.gameObject);
                return null;
            }

            MethodInfo methodInfo = component.GetType().GetMethod(methodName, arguments);

            if (methodInfo == null) Debug.LogWarning("Custom function error: The specified method or component name is not valid.", defaultComponent.gameObject);

            return methodInfo.CreateDelegate(delegateType, component);
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
            var del = GetDelegate(context.RunnerComponent, FunctionArgs, typeof(T));
            if (del is T typedDelegate) _function = typedDelegate;
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