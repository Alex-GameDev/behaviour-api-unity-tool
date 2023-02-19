using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Action = BehaviourAPI.Core.Actions.Action;
using LeafNode = BehaviourAPI.Unity.Framework.Adaptations.LeafNode;

namespace BehaviourAPI.Unity.Editor
{
    [FilePath("Config/StateFile.foo", FilePathAttribute.Location.PreferencesFolder)]
    public class BehaviourAPISettings : ScriptableSingleton<BehaviourAPISettings>
    {
        #region ----------------------- Script generation -----------------------

        public string GenerateScriptDefaultPath = "Assets/Scripts/";
        public string GenerateScriptDefaultName = "NewBehaviourRunner";

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
            "BehaviourAPI.Unity.Runtime",
            "BehaviourAPI.Unity.Framework",
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
            var time = System.DateTime.Now;
            var types = GetTypes();

            var unityActionTypes = GetValidSubTypes(typeof(UnityAction), types)
                .Select(t => new EditorHierarchyNode(t.Name.CamelCaseToSpaced(), t));

            _actionHierarchy = new EditorHierarchyNode("Actions", typeof(Action), new List<EditorHierarchyNode>()
            {
                new EditorHierarchyNode("Custom Action", typeof(CustomAction)),
                new EditorHierarchyNode("Unity Action(s)",typeof(UnityAction), unityActionTypes),
                new EditorHierarchyNode("Subgraph Action", typeof(SubgraphAction))
            });

            var unityPerceptionTypes =GetValidSubTypes(typeof(UnityPerception), types)
                .Select(t => new EditorHierarchyNode(t.Name.CamelCaseToSpaced(), t));

            var compoundPerceptionTypes = types.FindAll(t => t.IsSubclassOf(typeof(CompoundPerception)) &&
                 t.GetConstructors().Any(c => c.GetParameters().Length == 0))
                .Select(t => new EditorHierarchyNode(t.Name.CamelCaseToSpaced(), t));

            _perceptionHierarchy = new EditorHierarchyNode("Perceptions", typeof(Perception), new List<EditorHierarchyNode>()
            {
                new EditorHierarchyNode("Custom Perception", typeof(CustomPerception)),
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

            Debug.Log($"Time to create hierarchies: {(System.DateTime.Now - time).TotalMilliseconds}");
            //Debug.Log($"Number of adapters: {_graphAdapterMap.Count()}");
            //Debug.Log($"Number of main nodes per type: {_nodeHierarchyMap.Select(kvp => kvp.Value.Childs.Count().ToString()).Join()}");
        }

        static IEnumerable<System.Type> GetValidSubTypes(System.Type type, List<System.Type> allTypes)
        {
            return allTypes.FindAll(t => t.IsSubclassOf(type) &&
                !t.IsAbstract &&
                 t.GetConstructors().Any(c => c.GetParameters().Length == 0));
        }
    }
}