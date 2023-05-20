using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    using Core;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class that serialize node data.
    /// </summary>
    [Serializable]
    public class NodeData
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        public string name;

        /// <summary>
        /// The unique id of this element.
        /// </summary>
        [HideInInspector] public string id;

        /// <summary>
        /// The position of the node in the editor.
        /// </summary>
        [HideInInspector] public UnityEngine.Vector2 position;

        /// <summary>
        /// The serializable reference of the node.
        /// </summary>
        [SerializeReference] public Node node;

        /// <summary>
        /// List that allows unity to serialize the action(s) of the node if necessary.
        /// </summary>
        [SerializeField] public List<ActionData> actions = new List<ActionData>();

        /// <summary>
        /// List that allows unity to serialize the perceptions(s) of the node if necessary.
        /// </summary>
        [SerializeField] public List<PerceptionData> perceptions = new List<PerceptionData>();

        /// <summary>
        /// List that allows unity to serialize delegates of the node if necessary.
        /// </summary>
        [SerializeField] public List<FunctionData> functions = new List<FunctionData>();

        /// <summary>
        /// List of parent nodes referenced by id.
        /// </summary>
        [HideInInspector] public List<string> parentIds = new List<string>();

        /// <summary>
        /// List of children nodes referenced by id.
        /// </summary>
        [HideInInspector] public List<string> childIds = new List<string>();

        public NodeData(Type type, Vector2 position)
        {
            this.position = position;
            node = (Node)Activator.CreateInstance(type);
            name = "";
            id = Guid.NewGuid().ToString();

            ValidateReferences(type);
        }

        public void Validate() => ValidateReferences(node.GetType());

        void ValidateReferences(Type type)
        {
            var actionDict = this.actions.ToDictionary(k => k.Name, k => k.action);
            var perceptionDict = this.perceptions.ToDictionary(k => k.Name, k => k.perception);
            var functionDict = this.functions.ToDictionary(k => k.Name, k => k.method);

            List<ActionData> actions = new List<ActionData>();
            List<PerceptionData> perceptions = new List<PerceptionData>();
            List<FunctionData> functions = new List<FunctionData>();

            foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
            {
                if (fieldInfo.FieldType == typeof(Core.Actions.Action))
                {
                    actions.Add(new ActionData(fieldInfo.Name));
                }
                else if (fieldInfo.FieldType == typeof(Core.Perceptions.Perception))
                {
                    perceptions.Add(new PerceptionData(fieldInfo.Name));
                }
                else if (fieldInfo.FieldType.IsSubclassOf(typeof(Delegate)))
                {
                    Type delegateType = fieldInfo.FieldType;
                    //TODO: Guardar parámetros?

                    MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
                    ParameterInfo[] parameters = invokeMethod.GetParameters();
                    Type returnType = invokeMethod.ReturnType;
                    functions.Add(new FunctionData(fieldInfo.Name));
                }
            }

            foreach (var actionData in actions)
            {
                if(actionDict.TryGetValue(actionData.Name, out Core.Actions.Action a))
                {
                    actionData.action = a;
                }
            }

            foreach (var perceptionData in perceptions)
            {
                if (perceptionDict.TryGetValue(perceptionData.Name, out Core.Perceptions.Perception p))
                {
                    perceptionData.perception = p;
                }
            }

            foreach (var functionData in functions)
            {
                if (functionDict.TryGetValue(functionData.Name, out SerializedContextMethod m))
                {
                    functionData.method = m;
                }
            }

            this.actions = actions;
            this.perceptions = perceptions;
            this.functions = functions;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public NodeData()
        {
        }

        /// <summary>
        /// Create a new node data by a node and id.
        /// </summary>
        /// <param name="node">The <see cref="Node"/> reference</param>
        /// <param name="id">The id of the element.</param>
        public NodeData(Node node, string id)
        {
            this.node = node;
            this.id = id;
        }

        /// <summary>
        /// Creates a copy in the same graph, with a new id and name.
        /// </summary>
        /// <returns>The duplicated node.</returns>
        public NodeData Duplicate()
        {
            NodeData duplicate = new NodeData();
            duplicate.name = name + " (copy)";
            duplicate.id = Guid.NewGuid().ToString();
            duplicate.position = position + UnityEngine.Vector2.one * 50;
            duplicate.node = (Node)node.Clone();
            return duplicate;
        }

        public void BuildReferences(Component runner)
        {
            actions.ForEach(a => a.Build(node));
            perceptions.ForEach(p => p.Build(node));
            functions.ForEach(f => f.Build(node, runner));
        }
    }
}