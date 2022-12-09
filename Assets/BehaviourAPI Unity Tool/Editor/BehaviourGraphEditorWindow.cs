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
using UnityEngine.Assertions.Must;
using UnityEditor.Graphs;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourGraphEditorWindow : EditorWindow
    {
        public static BehaviourSystemAsset SystemAsset;
        public static bool IsAsset;

        VisualElement _container, _emptyGraphPanel;
        BehaviourGraphView _graphView;
        NodeInspectorView _nodeInspector;
        BehaviourGraphInspectorView _graphInspector;

        ToolbarMenu _selectGraphToolbarMenu;
        ToolbarToggle _autosaveToolbarToggle;
        ToolbarButton _saveToolbarButton, _deleteGraphToolbarButton, _addGraphToolbarButton;

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

            if (SystemAsset.Graphs.Count > 0)
            {
                DisplayGraph(SystemAsset.RootGraph);
            }
            else
            {
                _emptyGraphPanel.style.display = DisplayStyle.Flex;
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
            _graphView.NodeAdded += OnAddNode;
            _graphView.NodeRemoved += OnRemoveNode;

            _emptyGraphPanel = AddEmptyGraphPanel();
            _emptyGraphPanel.style.display = DisplayStyle.None;
            SetUpToolbar();
        }

        VisualElement AddEmptyGraphPanel()
        {
            var visualElement = VisualSettings.GetOrCreateSettings().EmptyGraphPanel.Instantiate();
            rootVisualElement.Add(visualElement);
            visualElement.Q<Button>("egp-add-btn").clicked += ShowGraphCreationPanel;
            return visualElement;
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
            _deleteGraphToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-delete-btn");

            _deleteGraphToolbarButton.clicked += DisplayDeleteGraphAlertWindow;
            _saveToolbarButton.clicked += SaveSystemData;
            _autosaveToolbarToggle.RegisterValueChangedCallback((evt) => autoSave = evt.newValue);

            if (SystemAsset == null || SystemAsset.Graphs.Count == 0) return;
            SystemAsset.Graphs.ForEach(g => _selectGraphToolbarMenu.menu.AppendAction(g.Graph.GetType().Name, (d) => DisplayGraph(g)));
        }

        #endregion

        #region ----------------------- Layout event callbacks -----------------------

        void ShowGraphCreationPanel()
        {
            GraphCreationWindow.CreateGraphCreationWindow(CreateGraph);
        }

        void DisplayGraph(GraphAsset graphAsset)
        {
            _currentGraphAsset = graphAsset;
            _graphInspector.UpdateInspector(graphAsset);
            _graphView.SetGraph(graphAsset);

            _emptyGraphPanel.style.display = DisplayStyle.None;
        }

        void DisplayDeleteGraphAlertWindow()
        {
            if(SystemAsset == null || SystemAsset.Graphs.Count == 0) return;
            AlertWindow.CreateAlertWindow("Are you sure to delete the current graph?", DeleteCurrentGraph);
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

        void OnRemoveGraph(GraphAsset graph, int index)
        {
            if (IsAsset)
            {
                graph.Nodes.ForEach(AssetDatabase.RemoveObjectFromAsset);
                AssetDatabase.RemoveObjectFromAsset(graph);
            }               
            else
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (autoSave) SaveSystemData();

            _selectGraphToolbarMenu.menu.RemoveItemAt(index);            
        }

        void OnAddNode(NodeAsset node)
        {
            if (IsAsset)
            {
                node.name = node.Node.GetType().Name;
                AssetDatabase.AddObjectToAsset(node, SystemAsset);
            }
            else
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (autoSave) SaveSystemData();
        }

        void OnRemoveNode(NodeAsset node)
        {
            if (IsAsset)
                AssetDatabase.RemoveObjectFromAsset(node);
            else
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (autoSave) SaveSystemData();
        }

        #endregion

        #region ----------------------------- Modify asset -----------------------------

        void CreateGraph(string name, Type type)
        {
            if (SystemAsset == null) return;

            var graphAsset = SystemAsset.CreateGraph(name, type);
            OnAddGraph(graphAsset);
            DisplayGraph(graphAsset);
        }

        void DeleteCurrentGraph()
        {
            if(SystemAsset == null || _currentGraphAsset == null) return;

            var id = SystemAsset.Graphs.IndexOf(_currentGraphAsset);
            SystemAsset.RemoveGraph(_currentGraphAsset);
            OnRemoveGraph(_currentGraphAsset, id);

            if(SystemAsset.Graphs.Count > 0)
            {
                DisplayGraph(SystemAsset.RootGraph);
            }
            else
            {
                _graphView.ClearView();
                _emptyGraphPanel.style.display = DisplayStyle.Flex;
            }
        }

        #endregion
    }
}