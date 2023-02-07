using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using BehaviourAPI.Unity.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using BehaviourAPI.Unity.Framework;
using System.Reflection;
using BehaviourAPI.Core;
using Vector2 = UnityEngine.Vector2;
using System.Linq;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourSystemEditorWindow : EditorWindow
    {
        private static string path => BehaviourAPISettings.instance.EditorLayoutsPath + "windows/behavioursystemwindow.uxml";
        private static string emptyPanelPath => BehaviourAPISettings.instance.EditorLayoutsPath + "emptygraphpanel.uxml";

        public static BehaviourSystemAsset SystemAsset;
        public static bool IsAsset;
        public static bool IsRuntime;

        VisualElement _container, _emptyGraphPanel;
        BehaviourGraphView _graphView;
        NodeInspectorView _nodeInspector;
        IHidable _currentInspector;

        BehaviourGraphInspectorView _graphInspector;
        PullPerceptionInspectorView _pullPerceptionInspector;
        PushPerceptionInspectorView _pushPerceptionInspector;

        GraphAsset _currentGraphAsset;

        bool autoSave = false;

        VisualElement _emptyPanel;

        Label _assetLabel;

        [MenuItem("BehaviourAPI/Open window")]
        public static void Open()
        {
            BehaviourSystemEditorWindow window = GetWindow<BehaviourSystemEditorWindow>();
            window.minSize = new Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor");

            window.Refresh();
        }

        public static void OpenSystem(BehaviourSystemAsset systemAsset, bool runtime = false)
        {
            BehaviourSystemEditorWindow window = GetWindow<BehaviourSystemEditorWindow>();
            window.minSize = new Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor");

            if (SystemAsset != systemAsset || IsRuntime != runtime)
            {
                SystemAsset = systemAsset;
                IsAsset = AssetDatabase.Contains(systemAsset);
                IsRuntime = runtime;
                window.Refresh();
            }
        }

        #region ----------------------------- Create GUI -----------------------------

        void CreateGUI()
        {
            AddLayout();           
        }

        void OnDisable()
        {
            SystemAsset = null;
            IsAsset = false;
            IsRuntime = false;
        }

        void AddLayout()
        {
            var windownFromUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).Instantiate();
            rootVisualElement.Add(windownFromUXML);

            _emptyPanel = AddEmptyPanel();
            _assetLabel = rootVisualElement.Q<Label>("bw-asset-label");

            _container = rootVisualElement.Q("bw-content");
            _graphView = AddGraphView();
            _nodeInspector = AddNodeInspectorView();
            _graphInspector = AddGraphInspectorView();
            _pullPerceptionInspector = AddPullPerceptionInspectorView();
            _pushPerceptionInspector = AddPushPerceptionInspectorView();

            _graphView.NodeSelected += _nodeInspector.UpdateInspector;
            _graphView.NodeAdded += OnAddNode;
            _graphView.NodeRemoved += OnRemoveNode;

            _emptyGraphPanel = AddEmptyGraphPanel();
            _emptyGraphPanel.style.display = DisplayStyle.None;

            SetUpInspectorMenu();

            SetUpMainToolbar();
            SetUpEditToolbar();

            ChangeInspector(_graphInspector);
        }

        VisualElement AddEmptyPanel()
        {
            var emptyPanel = new VisualElement();
            emptyPanel.ChangeBackgroundColor(new Color(.085f, .085f, .085f, .9f));
            emptyPanel.style.alignItems = Align.Center;
            emptyPanel.style.justifyContent = Justify.Center;

            var label = new Label("Select a Behaviour System to start editing");
            label.style.fontSize = 16;
            emptyPanel.Add(label);
            emptyPanel.StretchToParentSize();
            rootVisualElement.Add(emptyPanel);
            return emptyPanel;
        }

        VisualElement AddEmptyGraphPanel()
        {
            var emptyPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(emptyPanelPath).Instantiate();
            _container.Add(emptyPanel);
            return emptyPanel;
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

        PullPerceptionInspectorView AddPullPerceptionInspectorView()
        {
            var pullPerceptionWindow = new PullPerceptionInspectorView(SystemAsset, _graphView);
            _container.Add(pullPerceptionWindow);
            pullPerceptionWindow.Disable();
            pullPerceptionWindow.PerceptionCreated += OnAddPerception;
            pullPerceptionWindow.PerceptionRemoved += OnRemovePerception;
            return pullPerceptionWindow;
        }

        PushPerceptionInspectorView AddPushPerceptionInspectorView()
        {
            var pushPerceptionInspector = new PushPerceptionInspectorView(SystemAsset, _graphView.NodeSearchWindow);
            _container.Add(pushPerceptionInspector);
            pushPerceptionInspector.Disable();
            pushPerceptionInspector.PushPerceptionCreated += OnAddAsset;
            pushPerceptionInspector.PushPerceptionRemoved += OnRemoveAsset;
            return pushPerceptionInspector;
        }

        private void SetUpInspectorMenu()
        {
            rootVisualElement.Q<Button>("im-pullperceptions-btn").clicked += () => ChangeInspector(_pullPerceptionInspector);
            rootVisualElement.Q<Button>("im-graph-btn").clicked += () => ChangeInspector(_graphInspector);
            rootVisualElement.Q<Button>("im-pushperceptions-btn").clicked += () => ChangeInspector(_pushPerceptionInspector);
        }

        void ChangeInspector(IHidable inspector)
        {
            if (_currentInspector == inspector) return;

            if (_currentInspector != null) _currentInspector.Hide();

            _currentInspector = inspector;

            if (_currentInspector != null) _currentInspector.Show();
        }

        #endregion

        #region ------------------------------ Toolbar -------------------------------

        Toolbar _mainToolbar;
        Toolbar _editToolbar;

        ToolbarMenu _selectGraphMenu;

        void SetUpMainToolbar()
        {
            _mainToolbar = rootVisualElement.Q<Toolbar>("bw-toolbar-main");

            _selectGraphMenu = _mainToolbar.Q<ToolbarMenu>("bw-toolbar-graph-menu");
        }

        void SetUpEditToolbar()
        {
            _editToolbar = rootVisualElement.Q<Toolbar>("bw-toolbar-edit");

            _editToolbar.Q<ToolbarButton>("bw-toolbar-setroot-btn").clicked += ChangeMainGraph;
            _editToolbar.Q<ToolbarButton>("bw-toolbar-clear-btn").clicked += OpenClearGraphWindow;
            _editToolbar.Q<ToolbarButton>("bw-toolbar-delete-btn").clicked += DisplayDeleteGraphAlertWindow;
            _editToolbar.Q<ToolbarButton>("bw-toolbar-save-btn").clicked += SaveSystemData;
            _editToolbar.Q<ToolbarButton>("bw-toolbar-generatescript-btn").clicked += OpenCreateScriptWindow;

            var addGraphMenu = _editToolbar.Q<ToolbarMenu>("bw-toolbar-add-menu");

            var adapters = typeof(GraphAdapter).GetSubClasses().FindAll(ad => ad.GetCustomAttribute<CustomAdapterAttribute>() != null);
            foreach (var adapter in adapters)
            {
                var graphType = adapter.GetCustomAttribute<CustomAdapterAttribute>().type;
                if (graphType.IsSubclassOf(typeof(BehaviourGraph)))
                {
                    addGraphMenu.menu.AppendAction(graphType.Name, _ => CreateGraph($"new {graphType.Name}", graphType));
                }
            }
        }

        #endregion

        #region ----------------------- Layout event callbacks -----------------------

        void Refresh()
        {
            if (SystemAsset != null)
            {
                _emptyPanel.Disable();

                if (IsAsset) _assetLabel.text = AssetDatabase.GetAssetPath(SystemAsset);
                else _assetLabel.text = "Scene";

                _graphView.SetSystem(SystemAsset);
                _pushPerceptionInspector.nodeSearchWindow = _graphView.NodeSearchWindow;

                if (SystemAsset.Graphs.Count > 0)
                {
                    DisplayGraph(SystemAsset.MainGraph, forceRefresh: true);
                }
                else
                {
                    DisplayGraph(null, forceRefresh: true);
                }
            }
            else
            {
                _graphView.ClearView();
                _emptyPanel.Enable();
            }

            _pullPerceptionInspector.SetSystem(SystemAsset);
            _pushPerceptionInspector.SetSystem(SystemAsset);

            if (IsRuntime) _editToolbar.Hide();
            else _editToolbar.Show();


            UpdateGraphSelectionToolbar();
        }

        void UpdateGraphSelectionToolbar()
        {
            _selectGraphMenu.menu.MenuItems().Clear();

            if (SystemAsset == null) return;

            foreach (var graph in SystemAsset.Graphs)
            {
                _selectGraphMenu.menu.AppendAction(
                    actionName: $"{graph.Name} ({graph.Graph.GetType().Name}) {(SystemAsset.MainGraph == graph ? "- root" : "")}",
                    action: _ => DisplayGraph(graph),
                    status: _currentGraphAsset == graph ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal
                );
            }
        }

        void OpenCreateScriptWindow()
        {
            ScriptCreationWindow.Create();
        }

        void OpenClearGraphWindow()
        {
            if (_currentGraphAsset == null) return;
            AlertWindow.CreateAlertWindow("¿Clear current graph?", ClearCurrentGraph);

        }

        void DisplayGraph(GraphAsset graphAsset, bool forceRefresh = false)
        {
            if (graphAsset != null && _currentGraphAsset == graphAsset && !forceRefresh) return;

            _currentGraphAsset = graphAsset;

            if (graphAsset != null)
            {
                _emptyGraphPanel.Disable();
                _graphView.SetGraph(graphAsset);
            }
            else
            {
                _graphView.ClearView();
                _emptyGraphPanel.Enable();
            }

            _graphInspector.UpdateInspector(graphAsset);     

            UpdateGraphSelectionToolbar();
        }

        void DisplayDeleteGraphAlertWindow()
        {
            if (SystemAsset == null || SystemAsset.Graphs.Count == 0) return;
            AlertWindow.CreateAlertWindow("Are you sure to delete the current graph?", DeleteCurrentGraph);
        }

        void ChangeMainGraph()
        {
            if (_currentGraphAsset == SystemAsset.MainGraph) return;
            SystemAsset.MainGraph = _currentGraphAsset;
            UpdateGraphSelectionToolbar();
            Toast("Main graph changed");
        }

        void SaveSystemData()
        {
            if (IsAsset)
                AssetDatabase.SaveAssets();
            else
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        void OnAddNode(NodeAsset node)
        {
            OnAddAsset(node);
        }

        void OnRemoveNode(NodeAsset node)
        {
            bool isChanged = false;
            foreach(var pp in SystemAsset.PushPerceptions)
            {
                if(pp.Targets.Remove(node)) isChanged = true;
            }

            if (isChanged) _pushPerceptionInspector.ForceRefresh();

            OnRemoveAsset(node);
        }

        void OnAddPerception(PerceptionAsset perceptionAsset)
        {
            OnAddAsset(perceptionAsset);
        }

        void OnRemovePerception(PerceptionAsset perception)
        {
            bool isChanged = false;
            foreach(var p in SystemAsset.Perceptions)
            {
                if (p is CompoundPerceptionAsset cpa && cpa.subperceptions.Remove(perception)) isChanged = true;
            }

            if (isChanged) _pullPerceptionInspector.ForceRefresh();

            OnRemoveAsset(perception);
        }

        void OnAddAsset(ScriptableObject asset)
        {
            if (IsAsset)
            {
                asset.name = asset.GetType().Name;
                AssetDatabase.AddObjectToAsset(asset, SystemAsset);
            }
            else
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            if (autoSave) SaveSystemData();
        }

        void OnRemoveAsset(ScriptableObject asset)
        {
            if (IsAsset)
                AssetDatabase.RemoveObjectFromAsset(asset);
            else
            {
                DestroyImmediate(asset);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }


            if (autoSave) SaveSystemData();
        }

        #endregion

        #region ----------------------------- Modify asset -----------------------------

        void CreateGraph(string name, Type type)
        {
            if (SystemAsset == null) return;

            var graphAsset = SystemAsset.CreateGraph(name, type);
            OnAddAsset(graphAsset);

            UpdateGraphSelectionToolbar();
            DisplayGraph(graphAsset);

            Toast("Graph created");
        }

        void ClearCurrentGraph()
        {
            if (SystemAsset == null || _currentGraphAsset == null) return;

            _graphView.ClearView();

            if (IsAsset) _currentGraphAsset.Nodes.ForEach(AssetDatabase.RemoveObjectFromAsset);

            _currentGraphAsset.Nodes.Clear();

            Toast("Graph clean");
        }

        void DeleteCurrentGraph()
        {
            if (SystemAsset == null || _currentGraphAsset == null) return;

            if (IsAsset) _currentGraphAsset.Nodes.ForEach(OnRemoveAsset);

            SystemAsset.RemoveGraph(_currentGraphAsset);

            OnRemoveAsset(_currentGraphAsset);

            if (SystemAsset.Graphs.Count > 0)
            {
                DisplayGraph(SystemAsset.MainGraph);
            }
            else
            {
                _graphView.ClearView();
                _emptyGraphPanel.style.display = DisplayStyle.Flex;
            }


            Toast("Graph deleted");
        }

        #endregion

        void Toast(string message, float timeout = .5f)
        {
            ShowNotification(new GUIContent(message), timeout);
        }
    }
}