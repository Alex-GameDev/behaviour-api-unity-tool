using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System.Reflection;
using UnityEditorInternal;

namespace BehaviourAPI.Unity.Editor
{
    class BehaviourAPISettingsProvider : SettingsProvider
    {
        class Styles
        {
            public static GUIContent BehaviourGraphEditorWindowLayout = new GUIContent("BehaviourGraphEditorWindowLayout");

            public static GUIContent AlertWindowLayout = new GUIContent("AlertWindowLayout");
            public static GUIContent GraphCreationWindowLayout = new GUIContent("GraphCreationWindowLayout");
            public static GUIContent ScriptCreationWindowLayout = new GUIContent("ScriptCreationWindowLayout");

            public static GUIContent NodeLayout = new GUIContent("NodeLayout");
            public static GUIContent ContainerLayout = new GUIContent("ContainerLayout");
            public static GUIContent InspectorLayout = new GUIContent("InspectorLayout");
            public static GUIContent EmptyGraphPanel = new GUIContent("EmptyGraphPanel");

            public static GUIContent CustomActionLayout = new GUIContent("Custom Task Layout File");
            public static GUIContent UnityActionLayout = new GUIContent("Unity Task Layout File");
            public static GUIContent SubgraphActionLayout = new GUIContent("Subgraph Action Layout File");
            public static GUIContent ExitActionLayout = new GUIContent("Exit Action Layout File");
            public static GUIContent StatusPerceptionLayout = new GUIContent("Status Perception Layout File");
            public static GUIContent CompoundPerceptionLayout = new GUIContent("Compound Perception Layout File");

            public static GUIContent Assemblies = new GUIContent("Assemblies");
        }

        SerializedObject m_SerializedObject;

        SerializedProperty m_assemblies;

        public BehaviourAPISettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            BehaviourAPISettings.instance.Save();
            m_SerializedObject = new SerializedObject(BehaviourAPISettings.instance);
            m_assemblies = m_SerializedObject.FindProperty("m_assemblies");
        }
        public override void OnGUI(string searchContext)
        {
            using (CreateSettingsWindowGUIScope())
            {
                EditorGUILayout.LabelField("Assemblies", EditorStyles.boldLabel);
                m_SerializedObject.FindProperty("Assemblies").stringValue = EditorGUILayout.TextField(Styles.Assemblies, m_SerializedObject.FindProperty("Assemblies").stringValue);

                if (EditorGUI.EndChangeCheck())
                {
                    m_SerializedObject.ApplyModifiedProperties();
                    BehaviourAPISettings.instance.Save();
                }
            }
        }

        void LayoutProperty(SerializedProperty property, GUIContent label, Type type)
        {
            property.objectReferenceValue = EditorGUILayout.ObjectField(label, property.objectReferenceValue, type, false);
        }

        [SettingsProvider]
        public static SettingsProvider CreateMySingletonProvider()
        {
            var provider = new BehaviourAPISettingsProvider("Project/BehaviourAPI settings", SettingsScope.Project, GetSearchKeywordsFromGUIContentProperties<Styles>());
            return provider;
        }

        private IDisposable CreateSettingsWindowGUIScope()
        {
            var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
            return Activator.CreateInstance(type) as IDisposable;
        }
    }
}