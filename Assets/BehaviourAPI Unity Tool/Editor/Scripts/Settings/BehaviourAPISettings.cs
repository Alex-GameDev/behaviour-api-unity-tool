using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        public VisualTreeAsset UnityActionLayout;
        public VisualTreeAsset CustomActionLayout;
        public VisualTreeAsset SubgraphActionLayout;
        public VisualTreeAsset ExitActionLayout;


        [Header("Style")]
        public StyleSheet BehaviourGraphEditorWindowStylesheet;
        public StyleSheet GraphStylesheet;
        public StyleSheet NodeStylesheet;
        public StyleSheet InspectorStylesheet;

        public string Assemblies;

        public void Save() => Save(true);
    }
}