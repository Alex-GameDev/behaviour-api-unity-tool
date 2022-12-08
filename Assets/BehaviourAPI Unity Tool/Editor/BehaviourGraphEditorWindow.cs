using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using BehaviourAPI.Unity.Runtime;
using UnityEditor.Experimental.GraphView;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourGraphEditorWindow : EditorWindow
    {
        public static BehaviourSystemAsset SystemAsset;

        VisualElement _container, _rootgraphContainer;
        BehaviourGraphView _graphView;
        NodeInspectorView _nodeInspector;
        BehaviourGraphInspectorView _graphInspector;
        

        public static void OpenGraph(BehaviourSystemAsset systemAsset)
        {
            SystemAsset = systemAsset;
            BehaviourGraphEditorWindow window = GetWindow<BehaviourGraphEditorWindow>();
            window.minSize = new Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor");
        }

        private void CreateGUI()
        {
            AddLayout();
        }

        void AddLayout()
        {
            var windowLayout = VisualSettings.GetOrCreateSettings().BehaviourGraphEditorWindowLayout.Instantiate();
            rootVisualElement.Add(windowLayout);

            _container = rootVisualElement.Q("bw-content");
            _graphView = AddGraphView();
            _nodeInspector = AddNodeInspectorView();
            _graphInspector = AddGraphInspectorView();
            _graphView.NodeSelected += _nodeInspector.UpdateInspector;
            _rootgraphContainer = rootVisualElement.Q("bw-rootgraph");
        }

        private BehaviourGraphView AddGraphView()
        {
            var graphView = new BehaviourGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Insert(0, graphView);
            return graphView;
        }

        private NodeInspectorView AddNodeInspectorView()
        {
            var nodeInspector = new NodeInspectorView();
            _container.Add(nodeInspector);
            return nodeInspector;
        }

        private BehaviourGraphInspectorView AddGraphInspectorView()
        {
            var graphInspector = new BehaviourGraphInspectorView();
            _container.Add(graphInspector);
            return graphInspector;
        }
    }
}