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
        public static BehaviourGraphAsset GraphAsset;
        NodeInspectorView nodeInspector;

        public static void OpenGraph(BehaviourGraphAsset graphAsset)
        {
            GraphAsset = graphAsset;
            BehaviourGraphEditorWindow window = GetWindow<BehaviourGraphEditorWindow>();
            window.minSize = new Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor ({graphAsset.Graph.GetType().Name})");
        }

        private void CreateGUI()
        {
            var graphView = AddGraphView();
            var nodeInspectorView = AddNodeInspectorView();
            var graphInspectorView = AddGraphInspectorView();
            graphView.NodeSelected += nodeInspectorView.UpdateInspector;
            graphInspectorView.UpdateInspector(GraphAsset);
        }

        private BehaviourGraphView AddGraphView()
        {
            var graphView = new BehaviourGraphView(GraphAsset, this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            return graphView;
        }
        private NodeInspectorView AddNodeInspectorView()
        {
            var nodeInspector = new NodeInspectorView();
            rootVisualElement.Add(nodeInspector);
            return nodeInspector;
        }

        private BehaviourGraphInspectorView AddGraphInspectorView()
        {
            var graphInspector = new BehaviourGraphInspectorView();
            rootVisualElement.Add(graphInspector);
            return graphInspector;
        }
    }
}