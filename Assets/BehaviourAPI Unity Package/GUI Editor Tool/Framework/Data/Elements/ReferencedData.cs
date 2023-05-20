using System;
using System.Reflection;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    using Core;

    [System.Serializable]
    public class FunctionData
    {
        [SerializeField] string fieldName;
        public SerializedContextMethod method;

        public string Name => fieldName;

        public FunctionData(string fieldName)
        {
            this.fieldName = fieldName;
        }

        public void Build(Node node, Component runner)
        {
            Type classType = string.IsNullOrEmpty(method.componentName) ? runner.GetType() : Type.GetType(method.componentName);
            FieldInfo field = node.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            if (field != null && field.FieldType.IsSubclassOf(typeof(Delegate)))
            {
                MethodInfo methodInfo = classType.GetMethod(method.methodName);

                if (method != null && methodInfo.CreateDelegate(field.FieldType, node) is Delegate del)
                {
                    field.SetValue(node, del);
                }
                else
                {
                    Debug.LogWarning($"The serialized method data does not correspond to the signature of the referenced delegate field \"{fieldName}\", the value was not set properly.");
                }
            }
            else
            {
                Debug.LogWarning($"The field \"{fieldName}\" does not exist or is not a delegate reference, the value was not set properly.");
            }
        }
    }


    [System.Serializable]
    public class ActionData
    {
        [SerializeField] string fieldName;
        [SerializeReference] public Core.Actions.Action action;

        public string Name => fieldName;

        public ActionData(string fieldName)
        {
            this.fieldName = fieldName;
        }

        public void Build(Node node)
        {
            var type = node.GetType();
            var field = type.GetField(fieldName);
            if (field != null && field.FieldType.IsAssignableFrom(action.GetType()))
            {
                field.SetValue(node, action);
            }
            else
            {
                Debug.LogWarning($"The field \"{fieldName}\" does not exist or does not correspond to a property of type Action, the value was not set properly.");
            }
        }
    }


    [System.Serializable]
    public class PerceptionData
    {
        [SerializeField] string fieldName;
        [SerializeReference] public Core.Perceptions.Perception perception;

        public string Name => fieldName;

        public PerceptionData(string fieldName)
        {
            this.fieldName = fieldName;
        }

        public void Build(Node node)
        {
            var type = node.GetType();
            var field = type.GetField(fieldName);
            if (field != null && field.FieldType.IsAssignableFrom(perception.GetType()))
            {
                field.SetValue(node, perception);
            }
            else
            {
                Debug.LogWarning($"The field \"{fieldName}\" does not exist or does not correspond to a property of type Perception, the value was not set properly.");
            }
        }
    }
}