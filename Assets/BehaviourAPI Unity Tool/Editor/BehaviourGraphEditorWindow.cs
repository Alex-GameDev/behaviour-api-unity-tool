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
using System.Collections.Generic;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourGraphEditorWindow : EditorWindow
    {
        public static BehaviourSystemAsset SystemAsset;
        public static bool IsAsset;

        VisualElement _container, _emptyGraphPanel;
        BehaviourGraphView _graphView;
        NodeInspectorView _nodeInspector;
        IHidable _currentInspector;
        BehaviourGraphInspectorView _graphInspector;
        ActionInspectorView _actionInspector;

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
            _actionInspector = AddActionInspectorView();

            _graphView.NodeSelected += _nodeInspector.UpdateInspector;
            _graphView.NodeAdded += OnAddNode;
            _graphView.NodeRemoved += OnRemoveNode;

            _emptyGraphPanel = AddEmptyGraphPanel();
            _emptyGraphPanel.style.display = DisplayStyle.None;

            SetUpInspectorMenu();
            SetUpToolbar();

            _actionInspector.Hide();
            ChangeInspector(_graphInspector);
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

        ActionInspectorView AddActionInspectorView()
        {
            var actionInspector = new ActionInspectorView();
            _container.Add(actionInspector);
            return actionInspector;
        }

        BehaviourGraphInspectorView AddGraphInspectorView()
        {
            var graphInspector = new BehaviourGraphInspectorView();
            _container.Add(graphInspector);
            return graphInspector;
        }

        private void SetUpInspectorMenu()
        {
            rootVisualElement.Q<Button>("im-graph-btn").clicked += () => ChangeInspector(_graphInspector);
            rootVisualElement.Q<Button>("im-actions-btn").clicked += () => ChangeInspector(_actionInspector);
        }

        void SetUpToolbar()
        {
            _selectGraphToolbarMenu = rootVisualElement.Q<ToolbarMenu>("bw-toolbar-graph-menu");
            _addGraphToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-add-btn");
            _autosaveToolbarToggle = rootVisualElement.Q<ToolbarToggle>("bw-toolbar-autosave-toggle");
            _saveToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-save-btn");
            _deleteGraphToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-delete-btn");

            _addGraphToolbarButton.clicked += ShowGraphCreationPanel;
            _deleteGraphToolbarButton.clicked += DisplayDeleteGraphAlertWindow;
            _saveToolbarButton.clicked += SaveSystemData;
            _autosaveToolbarToggle.RegisterValueChangedCallback((evt) => autoSave = evt.newValue);

            UpdateGraphSelectionToolbar();

            if (SystemAsset == null || SystemAsset.Graphs.Count == 0) return;
            
        }

        void UpdateGraphSelectionToolbar()
        {
            _selectGraphToolbarMenu.menu.MenuItems().Clear();

            SystemAsset.Graphs.ForEach(g => 
                _selectGraphToolbarMenu.menu.AppendAction(
                    $"{g.Name} ({g.Graph.GetType().Name}) {(SystemAsset.RootGraph == g ? "- root" : "")}", 
                    (d) => DisplayGraph(g), 
                _currentGraphAsset == g ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal)
            );
        }

        void ChangeInspector(IHidable inspector)
        {
            if (_currentInspector == inspector) return;

            if (_currentInspector != null) _currentInspector.Hide();

            _currentInspector = inspector;

            if (_currentInspector != null) _currentInspector.Show();
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
            UpdateGraphSelectionToolbar();
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

            UpdateGraphSelectionToolbar();
        }

        void OnRemoveGraph(GraphAsset graph)
        {
            if (IsAsset)
            {
                graph.Nodes.ForEach(AssetDatabase.RemoveObjectFromAsset);
                AssetDatabase.RemoveObjectFromAsset(graph);
            }               
            else
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (autoSave) SaveSystemData();

            UpdateGraphSelectionToolbar();
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

            SystemAsset.RemoveGraph(_currentGraphAsset);
            OnRemoveGraph(_currentGraphAsset);

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