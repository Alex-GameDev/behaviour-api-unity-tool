using BehaviourAPI.Core;
using System;
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

        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(methodName))
            {
                if (component.GetType().GetMethod(methodName) == null)
                {
                    methodName = "";
                    _function = null;
                }
                else if (_function == null)
                {
                    var method = component.GetType().GetMethod(methodName);

                    var methodCall = Expression.Call(Expression.Constant(component), method);
                    _function = Expression.Lambda<T>(methodCall).Compile();
                }
            }
        }

        public void OnBeforeSerialize()
        {
            return;
        }
    }

    [Serializable]
    public class SerializedAction : SerializedMethod<Action> { }

    [Serializable]
    public class SerializedStatusFunction : SerializedMethod<Func<Status>> { }

    [Serializable]
    public class SerializedBoolFunction : SerializedMethod<Func<bool>> { }

}
