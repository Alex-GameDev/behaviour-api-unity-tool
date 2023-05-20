using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    [System.Serializable]
    public class TaskInfo<T>
    {
        [SerializeField] string fieldName;
        [SerializeReference] protected T task;

        public TaskInfo(string fieldName)
        {
            this.fieldName = fieldName;
        }

        public string FieldName => fieldName;
        public T Task => task;

        public void Build(Node node)
        {
            var type = node.GetType();
            var field = type.GetField(fieldName);
            if (field.FieldType.IsAssignableFrom(task.GetType()))
            {
                field.SetValue(node, task);
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