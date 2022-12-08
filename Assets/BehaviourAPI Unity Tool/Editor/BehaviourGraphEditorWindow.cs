using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using BehaviourAPI.Unity.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine.SceneManagement;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourGraphEditorWindow : EditorWindow
    {
        public static BehaviourSystemAsset SystemAsset;
        public static bool IsAsset;

        VisualElement _container, _rootgraphContainer;
        BehaviourGraphView _graphView;
        NodeInspectorView _nodeInspector;
        BehaviourGraphInspectorView _graphInspector;

        ToolbarMenu _selectGraphToolbarMenu;
        ToolbarToggle _autosaveToolbarToggle;
        ToolbarButton _saveToolbarButton;

        ScrollView _createGraphList;

        GraphAsset _currentGraphAsset;
        bool autoSave = false;


        public static void OpenGraph(BehaviourSystemAsset systemAsset, bool isAsset = true)
        {
            SystemAsset = systemAsset;
            IsAsset = isAsset;
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

            SetUpToolbar();
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

        void SetUpToolbar()
        {
            _selectGraphToolbarMenu = rootVisualElement.Q<ToolbarMenu>("bw-toolbar-graph-menu");
            _autosaveToolbarToggle = rootVisualElement.Q<ToolbarToggle>("bw-toolbar-autosave-toggle");
            _saveToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-save-btn");

            _saveToolbarButton.clicked += SaveSystemData;
            _autosaveToolbarToggle.RegisterValueChangedCallback((evt) => autoSave = evt.newValue);

            if (SystemAsset == null || SystemAsset.Graphs.Count == 0) return;
            SystemAsset.Graphs.ForEach(g => _selectGraphToolbarMenu.menu.AppendAction(g.Graph.GetType().Name, (d) => DisplayGraph(g)));
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

        void SaveSystemData()
        {
            if (IsAsset) 
                AssetDatabase.SaveAssets();
            else 
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        void OnAddGraph(GraphAsset graph)
        {
            if (IsAsset)
            {
                graph.name = graph.Graph.GetType().Name;
                AssetDatabase.AddObjectToAsset(graph, SystemAsset);
            }               
            else
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (autoSave) SaveSystemData();

            bool isRoot = SystemAsset.RootGraph == graph;
            _selectGraphToolbarMenu.menu.AppendAction(graph.Graph.GetType().Name + (isRoot ? " (Root)" : ""), 
                (d) => DisplayGraph(graph));
        }

        void OnRemoveGraph(GraphAsset graph)
        {
            // TODO: Fix to execute after remove
            if (IsAsset)
                AssetDatabase.RemoveObjectFromAsset(graph);
            else
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            _selectGraphToolbarMenu.menu.RemoveItemAt(SystemAsset.Graphs.IndexOf(graph));
        }

        #endregion

        #region ----------------------------- Modify asset -----------------------------

        void CreateRootGraph(Type type)
        {
            if (SystemAsset == null) return;

            var rootGraph = SystemAsset.CreateGraph(type);
            SystemAsset.RootGraph = rootGraph;       
            HideEmptySystemPanel();
            OnAddGraph(rootGraph);
            DisplayGraph(rootGraph);
        }

        #endregion
    }
}