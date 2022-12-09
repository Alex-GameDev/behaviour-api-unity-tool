using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.Unity.Editor
{
    [CreateAssetMenu(fileName = "Behaviour API Visual Settings", menuName = "BehaviourAPI/Editor/VisualSettings", order = 0)]
    public class VisualSettings : ScriptableObject
    {
        [Header("Layout")]
        public VisualTreeAsset BehaviourGraphEditorWindowLayout;
        public VisualTreeAsset AlertWindowLayout;
        public VisualTreeAsset GraphCreationWindowLayout;

        public VisualTreeAsset NodeLayout;
        public VisualTreeAsset ContainerLayout;
        public VisualTreeAsset NodeInspectorLayout;
        public VisualTreeAsset graphInspectorLayout;
        public VisualTreeAsset EmptyGraphPanel;


        [Header("Style")]
        public StyleSheet BehaviourGraphEditorWindowStylesheet;
        public StyleSheet GraphStylesheet;
        public StyleSheet NodeStylesheet;
        public StyleSheet InspectorStylesheet;

        [Header("External assemblies")]
        public string[] assemblies = GetDefaultAssemblies();

       
        private static string[] GetDefaultAssemblies()
        {
            return new string[]{
                "Assembly-CSharp",
                "BehaviourAPI.Core",
                "BehaviourAPI.StateMachines",
                "BehaviourAPI.BehaviourTrees",
                "BehaviourAPI.UtilitySystems"
            };
        }

        public static VisualSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<VisualSettings>();
                AssetDatabase.CreateAsset(settings, "Assets");
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        static VisualSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:VisualSettings");
            if (guids.Length == 0)
            {
                return null;
            }
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<VisualSettings>(path);
            }
        }

        [ContextMenu("Reset assemblies")]
        public void ResetAssemblies()
        {
            assemblies = GetDefaultAssemblies();
        }
    }

}