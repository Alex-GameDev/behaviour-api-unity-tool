using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using Core;
    using Core.Actions;
    using Core.Perceptions;
    using UnityExtensions;
    using Framework.Adaptations;

    /// <summary>
    /// Class that manages all the Type metadata used in the package tools.
    /// </summary>
    public class APITypeMetadata
    {
        /// <summary>
        /// Dictionary that relates each non-abstract type of node to the Drawer that will be used to represent it in the editor.
        /// </summary>
        public Dictionary<Type, Type> NodeDrawerTypeMap { get; private set; } = new Dictionary<Type, Type>();

        /// <summary>
        /// Dictionary that relates each non-abstract type of behaviourGraph with
        /// </summary>
        public Dictionary<Type, Type> GraphAdapterMap { get; private set; } = new Dictionary<Type, Type>();
      
        /// <summary>
        /// The hierarchy node used to select a new <see cref="Action"/> in the creation window.
        /// </summary>
        public EditorHierarchyNode ActionHierarchy { get; private set; }

        /// <summary>
        /// The hierarchy node used to select a new <see cref="Perception"/> in the creation window.
        /// </summary>
        public EditorHierarchyNode PerceptionHierarchy { get; private set; }

        /// <summary>
        /// Dictionary that stores all the component types in the App domain.
        /// </summary>
        public Dictionary<string, Type> componentMap { get; private set; } = new Dictionary<string, Type>();

        public List<Type> NodeTypes = new List<Type>();

        /// <summary>
        /// Create a new API metadata.
        /// </summary>
        public APITypeMetadata()
        {
            var time = DateTime.Now;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> actionTypes = new List<Type>();
            List<Type> perceptionTypes = new List<Type>();
            HashSet<Type> nodeTypes = new HashSet<Type>();
            List<Type> graphTypes = new List<Type>();

            Dictionary<Type, Type> nodeDrawerMainTypeMap = new Dictionary<Type, Type>();
            Dictionary<Type, Type> graphAdapterMainTypeMap = new Dictionary<Type, Type>();

            HashSet<Type> nodeAdaptedTypes = new HashSet<Type>();
            HashSet<Type> nodeAdapterTypes = new HashSet<Type>();


            int c = 0;
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] types = assemblies[i].GetTypes();

                for(int j = 0; j < types.Length; j++)
                {
                    if (types[j].IsAbstract) continue;

                    if (typeof(Node).IsAssignableFrom(types[j]))
                    {
                        var adapterAttribute = types[j].GetCustomAttribute<NodeAdapterAttribute>();
                        if (adapterAttribute != null && typeof(Node).IsAssignableFrom(adapterAttribute.NodeType))
                        {
                            nodeAdapterTypes.Add(types[j]);
                            nodeAdaptedTypes.Add(adapterAttribute.NodeType);
                        }
                        else
                        {
                            nodeTypes.Add(types[j]);
                        }
                    }
                    else if (typeof(BehaviourGraph).IsAssignableFrom(types[j]))
                    {
                        graphTypes.Add(types[j]);
                    }
                    else if (typeof(UnityAction).IsAssignableFrom(types[j]))
                    {
                        actionTypes.Add(types[j]);
                    }
                    else if (typeof(UnityPerception).IsAssignableFrom(types[j]))
                    {
                        perceptionTypes.Add(types[j]);
                    }
                    else if (typeof(NodeDrawer).IsAssignableFrom(types[j]))
                    {
                        var drawerAttribute = types[j].GetCustomAttribute<CustomNodeDrawerAttribute>();
                        if (drawerAttribute != null && typeof(Node).IsAssignableFrom(drawerAttribute.NodeType))
                        {
                            nodeDrawerMainTypeMap[drawerAttribute.NodeType] = types[j];
                        }
                    }
                    else if(typeof(GraphAdapter).IsAssignableFrom(types[j]))
                    {
                        var adapterAttribute = types[j].GetCustomAttribute<CustomGraphAdapterAttribute>();
                        if (adapterAttribute != null && typeof(BehaviourGraph).IsAssignableFrom(adapterAttribute.GraphType))
                        {
                            graphAdapterMainTypeMap[adapterAttribute.GraphType] = types[j];
                        }
                    }
                    else if (typeof(Component).IsAssignableFrom(types[j]))
                    {
                        componentMap.TryAdd(types[j].Name, types[j]);
                        c++;
                    }

                }
            }

            nodeTypes.RemoveWhere((t) => nodeAdaptedTypes.Any(tb => tb.IsAssignableFrom(t)));
            nodeTypes.UnionWith(nodeAdapterTypes);

            NodeTypes = nodeTypes.ToList();
            BuildGraphAdapterMap(graphTypes, graphAdapterMainTypeMap);
            BuildNodeDrawerMap(NodeTypes, nodeDrawerMainTypeMap);         
            BuildActionHierarchy(actionTypes);
            BuildPerceptionHierarchy(perceptionTypes);

            Debug.Log((DateTime.Now - time).TotalMilliseconds);

            //Debug.Log("Actions: " + actionTypes.Count);
            //Debug.Log("Perceptions: " + perceptionTypes.Count);
            //Debug.Log("Nodes: " + nodeTypes.Count);


            Debug.Log((DateTime.Now - time).TotalMilliseconds);
        }

        private void BuildNodeDrawerMap(List<Type> nodeTypes, Dictionary<Type, Type> nodeDrawerMainTypeMap)
        {
            for (int i = 0; i < nodeTypes.Count; i++)
            {
                var type = nodeTypes[i];

                if (type.IsAbstract) continue;

                bool mainTypeFound = false;
                while (type != typeof(Node) && !mainTypeFound)
                {
                    if (nodeDrawerMainTypeMap.TryGetValue(type, out Type drawerType))
                    {
                        NodeDrawerTypeMap[nodeTypes[i]] = drawerType;
                        mainTypeFound = true;
                    }
                    else
                    {
                        type = type.BaseType;
                    }
                }
            }
        }

        private void BuildGraphAdapterMap(List<Type> graphTypes, Dictionary<Type, Type> graphAdapterMainTypeMap)
        {
            for (int i = 0; i < graphTypes.Count; i++)
            {
                var type = graphTypes[i];
                bool mainTypeFound = false;
                while (type != typeof(BehaviourGraph) && !mainTypeFound)
                {
                    if (graphAdapterMainTypeMap.TryGetValue(type, out Type adapterType))
                    {
                        GraphAdapterMap[graphTypes[i]] = adapterType;
                        mainTypeFound = true;
                    }
                    else
                    {
                        type = type.BaseType;
                    }
                }
            }
        }

        private void BuildActionHierarchy(List<Type> actionTypes)
        {
            ActionHierarchy = new EditorHierarchyNode("Actions", typeof(Action));
            ActionHierarchy.Childs.Add(new EditorHierarchyNode(typeof(CustomAction)));
            ActionHierarchy.Childs.Add(new EditorHierarchyNode(typeof(SubgraphAction)));

            Dictionary<string, EditorHierarchyNode> groups = new Dictionary<string, EditorHierarchyNode>();
            List<EditorHierarchyNode> ungroupedActionNodes = new List<EditorHierarchyNode>();

            for (int i = 0; i < actionTypes.Count; i++)
            {
                var groupAttributes = actionTypes[i].GetCustomAttributes<SelectionGroupAttribute>();
                EditorHierarchyNode actionTypeNode = new EditorHierarchyNode(actionTypes[i]);

                foreach (var groupAttribute in groupAttributes)
                {
                    if (!groups.TryGetValue(groupAttribute.name, out EditorHierarchyNode groupNode))
                    {
                        groupNode = new EditorHierarchyNode(groupAttribute.name, null);
                        groups[groupAttribute.name] = groupNode;
                    }
                    groupNode.Childs.Add(actionTypeNode);
                }

                if(groupAttributes.Count() == 0)
                {
                    ungroupedActionNodes.Add(actionTypeNode);
                }
            }
            EditorHierarchyNode unityActionHierarchyNode = new EditorHierarchyNode("Unity Actions", typeof(UnityAction));
            unityActionHierarchyNode.Childs.AddRange(groups.Values);
            unityActionHierarchyNode.Childs.AddRange(ungroupedActionNodes);
            ActionHierarchy.Childs.Add(unityActionHierarchyNode);
        }

        private void BuildPerceptionHierarchy(List<Type> perceptionTypes)
        {
            PerceptionHierarchy = new EditorHierarchyNode("Actions", typeof(Perception));
            PerceptionHierarchy.Childs.Add(new EditorHierarchyNode(typeof(CustomPerception)));

            EditorHierarchyNode compoundPerceptionHierarchyNode = new EditorHierarchyNode(typeof(CompoundPerception));
            compoundPerceptionHierarchyNode.Childs = perceptionTypes.FindAll(typeof(CompoundPerception).IsAssignableFrom)
                .Select(compoundPerceptionType => new EditorHierarchyNode(compoundPerceptionType)).ToList();

            PerceptionHierarchy.Childs.Add(compoundPerceptionHierarchyNode);

            Dictionary<string, EditorHierarchyNode> groups = new Dictionary<string, EditorHierarchyNode>();
            List<EditorHierarchyNode> ungroupedPerceptionsNodes = new List<EditorHierarchyNode>();

            for (int i = 0; i < perceptionTypes.Count; i++)
            {
                var groupAttributes = perceptionTypes[i].GetCustomAttributes<SelectionGroupAttribute>();
                EditorHierarchyNode actionTypeNode = new EditorHierarchyNode(perceptionTypes[i]);

                foreach (var groupAttribute in groupAttributes)
                {
                    if (!groups.TryGetValue(groupAttribute.name, out EditorHierarchyNode groupNode))
                    {
                        groupNode = new EditorHierarchyNode(groupAttribute.name, null);
                        groups[groupAttribute.name] = groupNode;
                    }
                    groupNode.Childs.Add(actionTypeNode);
                }

                if (groups.Count == 0)
                {
                    ungroupedPerceptionsNodes.Add(actionTypeNode);
                }
            }
            EditorHierarchyNode unityPerceptionHierarchyNode = new EditorHierarchyNode("Unity Perceptions", typeof(UnityPerception));
            unityPerceptionHierarchyNode.Childs.AddRange(groups.Values);
            unityPerceptionHierarchyNode.Childs.AddRange(ungroupedPerceptionsNodes);
            PerceptionHierarchy.Childs.Add(unityPerceptionHierarchyNode);
        }
    }
}