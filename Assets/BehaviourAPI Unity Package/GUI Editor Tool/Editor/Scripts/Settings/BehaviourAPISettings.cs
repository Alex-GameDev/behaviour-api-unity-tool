using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Action = BehaviourAPI.Core.Actions.Action;

namespace BehaviourAPI.Unity.Editor
{
    [FilePath("ProjectSettings/BehaviourAPISettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class BehaviourAPISettings : ScriptableSingleton<BehaviourAPISettings>
    {
        #region ------------------------- Default values ------------------------

        private static readonly Color k_LeafNodeColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_DecoratorColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_CompositeColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_StateColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_TransitionColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_LeafFactorColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_CurveFactorColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_FusionFactorColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_SelectableNodeColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_BucketColor = new Color(1f, 0.65f, 0.15f, 1f);

        #endregion

        #region ----------------------- Script generation -----------------------

        public string GenerateScriptDefaultPath = "Assets/Scripts/";
        public string GenerateScriptDefaultName = "NewBehaviourRunner";

        [Header("Colors")]
        public Color LeafNodeColor = k_LeafNodeColor;
        public Color DecoratorColor = k_DecoratorColor;
        public Color CompositeColor = k_CompositeColor;

        public Color StateColor = k_StateColor;
        public Color TransitionColor = k_TransitionColor;

        public Color LeafFactorColor = k_LeafFactorColor;
        public Color CurveFactorColor = k_CurveFactorColor;
        public Color FusionFactorColor = k_FusionFactorColor;
        public Color SelectableNodeColor = k_SelectableNodeColor;
        public Color BucketColor = k_BucketColor;

        #endregion

        public string RootPath = "Assets/BehaviourAPI Unity Tool";
        public string EditorLayoutsPath => $"{RootPath}/Editor/uxml/";
        public string EditorStylesPath => $"{RootPath}/Editor/uss/";

        public string CustomAssemblies;

        public readonly string[] DefaultAssemblies = new[]
        {
            "Assembly-CSharp",
            "BehaviourAPI.Core",
            "BehaviourAPI.StateMachines",
            "BehaviourAPI.BehaviourTrees",
            "BehaviourAPI.UtilitySystems",
            "BehaviourAPI.UnityExtensions",
            "BehaviourAPI.Unity.Runtime",
            "BehaviourAPI.UnityTool.Framework",
            "BehaviourAPI.Unity.Editor"
        };

        List<Assembly> assemblies = new List<Assembly>();

        EditorHierarchyNode _actionHierarchy;
        EditorHierarchyNode _perceptionHierarchy;
        Dictionary<System.Type, EditorHierarchyNode> _nodeHierarchyMap;
        Dictionary<System.Type, System.Type> _graphAdapterMap;


        public EditorHierarchyNode ActionHierarchy => _actionHierarchy;
        public EditorHierarchyNode PerceptionHierarchy => _perceptionHierarchy;
        public EditorHierarchyNode NodeHierarchy(Type type) => _nodeHierarchyMap[type];
        public Type Adapter(Type type) => _graphAdapterMap[type];


        public void Save() => Save(true);

        public List<Assembly> GetAssemblies() => assemblies;

        public List<System.Type> GetTypes()
        {
            return GetAssemblies().SelectMany(a => a.GetTypes()).ToList();
        }

        public void ReloadAssemblies()
        {
            var customAssemblies = CustomAssemblies.Split(';').ToList();
            var allAssemblyNames = customAssemblies.Union(DefaultAssemblies).ToHashSet();
            assemblies = System.AppDomain.CurrentDomain.GetAssemblies().ToList().FindAll(assembly =>
                allAssemblyNames.Contains(assembly.GetName().Name));

            BuildHierarchies();
        }

        private void BuildHierarchies()
        {
            var types = GetTypes();

            var unityActionTypes = GetValidSubTypes(typeof(UnityAction), types)
                .Select(t => new EditorHierarchyNode(t.Name.CamelCaseToSpaced(), t));

            var unityActionNode = GetUnityActionHierarchy(types);

            _actionHierarchy = new EditorHierarchyNode("Actions", typeof(Action), new List<EditorHierarchyNode>()
            {
                new EditorHierarchyNode("Custom Action", typeof(CustomAction)),
                new EditorHierarchyNode("Custom Action (Context)", typeof(ContextCustomAction)),
                unityActionNode,
                new EditorHierarchyNode("Subgraph Action", typeof(SubgraphAction))
            });

            var unityPerceptionTypes =GetValidSubTypes(typeof(UnityPerception), types)
                .Select(t => new EditorHierarchyNode(t.Name.CamelCaseToSpaced(), t)).ToList();

            var compoundPerceptionTypes = types.FindAll(t => t.IsSubclassOf(typeof(CompoundPerception)) &&
                 t.GetConstructors().Any(c => c.GetParameters().Length == 0))
                .Select(t => new EditorHierarchyNode(t.Name.CamelCaseToSpaced(), t)).ToList();

            _perceptionHierarchy = new EditorHierarchyNode("Perceptions", typeof(Perception), new List<EditorHierarchyNode>()
            {
                new EditorHierarchyNode("Custom Perception", typeof(CustomPerception)),
                new EditorHierarchyNode("Custom Perception (Context)", typeof(ContextCustomPerception)),
                new EditorHierarchyNode("Unity Perception(s)",typeof(UnityPerception), unityPerceptionTypes),
                new EditorHierarchyNode("Compound Perception(s)", typeof(CompoundPerception), compoundPerceptionTypes),
                new EditorHierarchyNode("Status Perception", typeof(ExecutionStatusPerception))
            });

            _nodeHierarchyMap = new Dictionary<System.Type, EditorHierarchyNode>();
            _graphAdapterMap = new Dictionary<System.Type, System.Type>();

            var graphAdapters = types.FindAll(t => t.IsSubclassOf(typeof(GraphAdapter)) &&
                !t.IsAbstract &&
                 t.GetConstructors().Any(c => c.GetParameters().Length == 0) &&
                 (t.GetCustomAttribute<CustomAdapterAttribute>()?.type.IsSubclassOf(typeof(BehaviourGraph)) ?? false));

            _graphAdapterMap = graphAdapters.ToDictionary(g => g.GetCustomAttribute<CustomAdapterAttribute>().type, g => g);

            _nodeHierarchyMap = graphAdapters.ToDictionary(g => g, g =>
            {
                var graphType = g.GetCustomAttribute<CustomAdapterAttribute>().type;
                var adapter = (GraphAdapter) Activator.CreateInstance(g);

                var list = new List<EditorHierarchyNode>();

                foreach (var type in adapter.MainTypes)
                {
                    var subtypes = GetValidSubTypes(type, types).ToList();
                    if(!type.IsAbstract) subtypes.Add(type);

                    subtypes = subtypes.Except(adapter.ExcludedTypes).ToList();

                    if(subtypes.Count == 1)
                    {
                        var elemType = subtypes.First();
                        list.Add(new EditorHierarchyNode(elemType.Name.CamelCaseToSpaced(), elemType));
                    }
                    else if(subtypes.Count > 1)
                    {
                        list.Add(new EditorHierarchyNode(type.Name.CamelCaseToSpaced() + "(s)", type,
                            subtypes.Select(subType => new EditorHierarchyNode(subType.Name.CamelCaseToSpaced(), subType)).ToList()));
                    }
                }
                return new EditorHierarchyNode($"{graphType.Name} nodes", graphType, list);
            });           

        }

        EditorHierarchyNode GetUnityActionHierarchy(List<System.Type> allTypes)
        {
            var types = GetValidSubTypes(typeof(UnityAction), allTypes);

            Dictionary<string, EditorHierarchyNode> groups = new Dictionary<string, EditorHierarchyNode>();
            List<EditorHierarchyNode> ungroupedActions = new List<EditorHierarchyNode>();

            foreach(var actionType in types)
            {
                var group = actionType.GetCustomAttributes<SelectionGroupAttribute>();
                var actionNode = new EditorHierarchyNode(actionType.Name.CamelCaseToSpaced(), actionType);

                if(group.Count() == 0)
                {
                    ungroupedActions.Add(actionNode);
                }
                else
                {
                    foreach (var attribute in group)
                    {
                        var groupName = attribute.name;
                        var groupNode = new EditorHierarchyNode(groupName, null);
                        groups.TryAdd(groupName, groupNode);
                        groups[groupName].Childs.Add(actionNode);
                    }
                }
            }

            var unityActionNode = new EditorHierarchyNode("Unity Actions", typeof(UnityAction),
                groups.Values.Union(ungroupedActions).ToList());
            return unityActionNode;
        }

        static IEnumerable<System.Type> GetValidSubTypes(System.Type type, List<System.Type> allTypes)
        {
            return allTypes.FindAll(t => t.IsSubclassOf(type) &&
                !t.IsAbstract &&
                 t.GetConstructors().Any(c => c.GetParameters().Length == 0));
        }
    }
}