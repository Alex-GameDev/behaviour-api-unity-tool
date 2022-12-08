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

        ScrollView _createGraphList;

        GraphAsset _currentGraphAsset;
        

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

            if (SystemAsset == null) return;

            if (SystemAsset.RootGraph != null)
            {
                HideEmptySystemPanel();
                DisplayGraph(SystemAsset.RootGraph);
            }
            else
            {
                ShowEmptySystemPanel();
            }
        }

        #region ----------------------------- Create GUI -----------------------------

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
            _createGraphList = rootVisualElement.Q<ScrollView>("bw-graphs-scrollview");
        }

        BehaviourGraphView AddGraphView()
        {
            var graphView = new BehaviourGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Insert(0, graphView);
            return graphView;
        }

        NodeInspectorView AddNodeInspectorView()
        {
            var nodeInspector = new NodeInspectorView();
            _container.Add(nodeInspector);
            return nodeInspector;
        }

        BehaviourGraphInspectorView AddGraphInspectorView()
        {
            var graphInspector = new BehaviourGraphInspectorView();
            _container.Add(graphInspector);
            return graphInspector;
        }

        #endregion

        #region ----------------------- Layout event callbacks -----------------------

        void ShowEmptySystemPanel()
        {
            _rootgraphContainer.style.visibility = Visibility.Visible;
            _graphInspector.Hide();
            _nodeInspector.Hide();
            _createGraphList.Clear();
            TypeUtilities.GetAllGraphTypes().ForEach(gType => _createGraphList
                .Add(new Button(() => CreateRootGraph(gType)) { text = gType.Name })
            );            
        }

        void HideEmptySystemPanel()
        {
            _graphInspector.Show();
            _nodeInspector.Show(); 
            _rootgraphContainer.style.visibility = Visibility.Hidden;
        }

        void DisplayGraph(GraphAsset graphAsset)
        {
            _currentGraphAsset = graphAsset;
            _graphInspector.UpdateInspector(graphAsset);
            _graphView.SetGraph(graphAsset);
        }

        #endregion

        #region ----------------------------- Modify asset -----------------------------

        void CreateRootGraph(Type type)
        {
            if (SystemAsset == null) return;

            var rootGraph = SystemAsset.CreateGraph(type);
            SystemAsset.RootGraph = rootGraph;       
            HideEmptySystemPanel();
            DisplayGraph(rootGraph);
        }

        #endregion
    }
}