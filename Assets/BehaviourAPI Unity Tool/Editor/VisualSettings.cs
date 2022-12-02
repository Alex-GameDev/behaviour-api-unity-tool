using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace BehaviourAPI.Unity.Editor
{
    [CreateAssetMenu(fileName = "Behaviour API Visual Settings", menuName = "BehaviourAPI/Editor/VisualSettings", order = 0)]
    public class VisualSettings : ScriptableObject
    {
        [Header("Layout")]
        public VisualTreeAsset NodeLayout;

        [Header("Style")]
        public StyleSheet VariablesStylesheet;
        public StyleSheet GraphStylesheet;
        public StyleSheet NodeStylesheet;

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
    }

}