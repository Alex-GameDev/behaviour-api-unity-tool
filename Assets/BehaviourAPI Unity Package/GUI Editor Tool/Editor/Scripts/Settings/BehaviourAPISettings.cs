using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using Core;
    using Core.Perceptions;
    using Core.Actions;
    using Framework.Adaptations;
    using UnityExtensions;

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

        private static readonly string[] k_DefaultAssemblies = new[]
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

        private static readonly string k_RootPath = "Assets/BehaviourAPI Unity Tool";
        #endregion

        #region ----------------------- Editor settings -----------------------

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

        private string RootPath = k_RootPath;
        private string CustomAssemblies = string.Empty;

        #endregion

        /// <summary>
        /// Root path of editor layout elements
        /// </summary>
        public string EditorLayoutsPath => $"{RootPath}/Editor/uxml/";

        /// <summary>
        /// Root path of editor style sheets
        /// </summary>
        public string EditorStylesPath => $"{RootPath}/Editor/uss/";

        List<Assembly> assemblies = new List<Assembly>();

        EditorHierarchyNode _actionHierarchy;
        EditorHierarchyNode _perceptionHierarchy;
        Dictionary<System.Type, EditorHierarchyNode> _nodeHierarchyMap;
        Dictionary<System.Type, System.Type> _graphAdapterMap;


        public EditorHierarchyNode ActionHierarchy => _actionHierarchy;
        public EditorHierarchyNode PerceptionHierarchy => _perceptionHierarchy;
        public EditorHierarchyNode NodeHierarchy(Type type) => _nodeHierarchyMap[type];
        public Type GetAdapter(Type type) => _graphAdapterMap[type];


        public void Save() => Save(true);

        public List<Assembly> GetAssemblies() => assemblies;

        public List<System.Type> GetTypes()
        {
            return GetAssemblies().SelectMany(a => a.GetTypes()).ToList();
        }

        /// <summary>
        /// Create the type hierarchies used in the editor when Unity reloads.
        /// </summary>
        public void ReloadAssemblies()
        {
            var customAssemblies = CustomAssemblies.Split(';').ToList();
            var allAssemblyNames = customAssemblies.Union(k_DefaultAssemblies).ToHashSet();
            assemblies = System.AppDomain.CurrentDomain.GetAssemblies().ToList().FindAll(assembly =>
                allAssemblyNames.Contains(assembly.GetName().Name));

            BuildHierarchies();
        }

        private void BuildHierarchies()
        {
            var types = GetTypes();

            var unityActionNode = EditorHierarchyNode.CreateGroupedHierarchyNode(types, typeof(UnityAction), "Unity Actions", true);

            _actionHierarchy = new EditorHierarchyNode("Actions", typeof(Action));
            _actionHierarchy.Childs.Add(new EditorHierarchyNode(typeof(CustomAction)));
            _actionHierarchy.Childs.Add(unityActionNode);
            _actionHierarchy.Childs.Add(new EditorHierarchyNode(typeof(SubgraphAction)));

            var unityPerceptionNode = EditorHierarchyNode.CreateGroupedHierarchyNode(types, typeof(UnityPerception), "Unity Perceptions", true);
            var compoundPerceptionNode = EditorHierarchyNode.CreateGroupedHierarchyNode(types, typeof(CompoundPerception), "Compound perceptions");

            _perceptionHierarchy = new EditorHierarchyNode("Perceptions", typeof(Perception));
            _perceptionHierarchy.Childs.Add(new EditorHierarchyNode(typeof(CustomPerception)));
            _perceptionHierarchy.Childs.Add(unityPerceptionNode);
            _perceptionHierarchy.Childs.Add(compoundPerceptionNode);

            var graphAdapters = types.FindAll(t => t.IsSubclassOf(typeof(GraphAdapter)) &&
                !t.IsAbstract &&
                 t.GetConstructors().Any(c => c.GetParameters().Length == 0) &&
                 (t.GetCustomAttribute<CustomAdapterAttribute>()?.type.IsSubclassOf(typeof(BehaviourGraph)) ?? false));

            _graphAdapterMap = graphAdapters.ToDictionary(g => g.GetCustomAttribute<CustomAdapterAttribute>().type, g => g);

            _nodeHierarchyMap = graphAdapters.ToDictionary(adapterType => adapterType, adapterType =>
            {
                var graphType = adapterType.GetCustomAttribute<CustomAdapterAttribute>().type;
                var adapter = (GraphAdapter)Activator.CreateInstance(adapterType);

                return EditorHierarchyNode.CreateCustomHierarchyNode(types, graphType,
                    $"{graphType.Name} nodes", adapter.MainTypes, adapter.ExcludedTypes);
            });
        }
    }
}