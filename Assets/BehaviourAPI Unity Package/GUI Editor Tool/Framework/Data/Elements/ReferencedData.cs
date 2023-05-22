using System;
using System.Reflection;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    using Core;
    using System.Linq;

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
            Component component = string.IsNullOrEmpty(method.componentName) ? runner : runner.gameObject.GetComponent(method.componentName);

            Type classType = string.IsNullOrEmpty(method.componentName) ? runner.GetType() : Type.GetType(method.componentName);
            FieldInfo field = node.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            if (!field.FieldType.IsSubclassOf(typeof(Delegate))) return;

            MethodInfo delegateMethod = field.FieldType.GetMethod("Invoke");
            ParameterInfo[] parameters = delegateMethod.GetParameters();
            Type[] parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

            var del = method.GetDelegate(runner, new Type[0], field.FieldType);

            if (del != null)
                field.SetValue(node, del);                   

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

        public void Build(Node node, SystemData data)
        {
            if (action == null) return;

            var type = node.GetType();
            var field = type.GetField(fieldName);
            if (field != null && field.FieldType.IsAssignableFrom(action.GetType()))
            {
                field.SetValue(node, action);
            }
            else
            {
                Debug.LogWarning($"The field \"{fieldName}\" does not exist or does not correspond to a property of type Action, the value was not set properly.\n" +
                    $"nodeType: {node.GetType().Name}\nfieldType: {field?.FieldType.Name}");
            }

            if(action is IBuildable buildable)
            {
                buildable.Build(data);
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
            if (perception == null) return;
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