using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using BehaviourAPI.Unity.Runtime;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourGraphEditorWindow : EditorWindow
    {
        //public static BehaviourGraphAsset asset
        public BehaviourGraphAsset graphAsset;

        [MenuItem("behaviour-api-unity-tool/BehaviourGraphWindow")]
        private static void ShowWindow()
        {
            var window = GetWindow<BehaviourGraphEditorWindow>();
            window.titleContent = new GUIContent("BehaviourGraphWindow");
            window.Show();
        }

        private void CreateGUI()
        {
            var graphView = AddGraphView();
        }

        private BehaviourGraphView AddGraphView()
        {
            var graphView = new BehaviourGraphView(graphAsset);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            return graphView;
        }
    }
}