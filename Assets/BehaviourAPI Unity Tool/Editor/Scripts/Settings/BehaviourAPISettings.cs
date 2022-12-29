using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [FilePath("Config/StateFile.foo", FilePathAttribute.Location.PreferencesFolder)]
    public class BehaviourAPISettings : ScriptableSingleton<BehaviourAPISettings>
    {
        [Header("Layout")]
        public VisualTreeAsset BehaviourGraphEditorWindowLayout;

        public VisualTreeAsset AlertWindowLayout;
        public VisualTreeAsset GraphCreationWindowLayout;


        public VisualTreeAsset NodeLayout;
        public VisualTreeAsset ContainerLayout;
        public VisualTreeAsset InspectorLayout;
        public VisualTreeAsset EmptyGraphPanel;

        public VisualTreeAsset ListItemLayout;

        [Header("Containers")]
        public VisualTreeAsset UnityTaskLayout;
        public VisualTreeAsset CustomTaskLayout;

        public VisualTreeAsset SubgraphActionLayout;
        public VisualTreeAsset ExitActionLayout;

        public VisualTreeAsset StatusPerceptionLayout;
        public VisualTreeAsset CompoundPerceptionLayout;


        [Header("Style")]
        public StyleSheet BehaviourGraphEditorWindowStylesheet;
        public StyleSheet GraphStylesheet;
        public StyleSheet NodeStylesheet;
        public StyleSheet InspectorStylesheet;

        public string Assemblies;

        public readonly string[] DefaultAssemblies = new[]
        {
            "Assembly-CSharp",
            "BehaviourAPI.Unity.Runtime",
            "BehaviourAPI.Core",
            "BehaviourAPI.StateMachines",
            "BehaviourAPI.BehaviourTrees",
            "BehaviourAPI.UtilitySystems"
        };

        public void Save() => Save(true);

        public List<Assembly> GetAssemblies()
        {
            return DefaultAssemblies.Select(assemblyName => Assembly.Load(assemblyName)).ToList();
        }
    }
}