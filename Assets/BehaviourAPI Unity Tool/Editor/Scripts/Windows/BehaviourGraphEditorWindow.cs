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
    public class BehaviourGraphEditorWindow : EditorWindow
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
        PushPerceptionInspectorView _pushPerceptionInspector;

        ToolbarMenu _selectGraphToolbarMenu, _addGraphMenu;
        ToolbarToggle _autosaveToolbarToggle;
        ToolbarButton _saveToolbarButton, _deleteGraphToolbarButton, _addGraphToolbarButton, _setRootGraphToolbarButton, _generateScriptToolbarButton, _clearGraphToolbarButton;

        GraphAsset _currentGraphAsset;


        bool autoSave = false;

        public static void OpenGraph(BehaviourSystemAsset systemAsset, bool runtime = false)
        {
            SystemAsset = systemAsset;
            IsAsset = AssetDatabase.Contains(systemAsset);
            IsRuntime = runtime;

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
            var windownFromUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).Instantiate();
            rootVisualElement.Add(windownFromUXML);

            _container = rootVisualElement.Q("bw-content");
            _graphView = AddGraphView();
            _nodeInspector = AddNodeInspectorView();
            _graphInspector = AddGraphInspectorView();
            _pushPerceptionInspector = AddPushPerceptionInspectorView();

            _graphView.NodeSelected += _nodeInspector.UpdateInspector;
            _graphView.NodeAdded += OnAddNode;
            _graphView.NodeRemoved += OnRemoveNode;

            _emptyGraphPanel = AddEmptyGraphPanel();
            _emptyGraphPanel.style.display = DisplayStyle.None;

            SetUpInspectorMenu();
            SetUpToolbar();

            ChangeInspector(_graphInspector);
        }

        VisualElement AddEmptyGraphPanel()
        {
            var emptyPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(emptyPanelPath).Instantiate();
            rootVisualElement.Add(emptyPanel);
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
            rootVisualElement.Q<Button>("im-graph-btn").clicked += () => ChangeInspector(_graphInspector);
            rootVisualElement.Q<Button>("im-pushperceptions-btn").clicked += () => ChangeInspector(_pushPerceptionInspector);
        }

        void SetUpToolbar()
        {
            if (IsRuntime)
            {
                var toolbar = rootVisualElement.Q<Toolbar>("bw-toolbar");
                var runtimeToolbar = rootVisualElement.Q<Toolbar>("bw-runtime-toolbar");
                toolbar.Disable();
                runtimeToolbar.Enable();

                _selectGraphToolbarMenu = rootVisualElement.Q<ToolbarMenu>("bw-runtime-toolbar-graph-menu");
            }
            else
            {
                _selectGraphToolbarMenu = rootVisualElement.Q<ToolbarMenu>("bw-toolbar-graph-menu");
                _addGraphToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-add-btn");
                _autosaveToolbarToggle = rootVisualElement.Q<ToolbarToggle>("bw-toolbar-autosave-toggle");
                _saveToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-save-btn");
                _deleteGraphToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-delete-btn");
                _setRootGraphToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-setroot-btn");
                _generateScriptToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-generatescript-btn");
                _clearGraphToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-clear-btn");

                _deleteGraphToolbarButton.clicked += DisplayDeleteGraphAlertWindow;
                _saveToolbarButton.clicked += SaveSystemData;
                _autosaveToolbarToggle.RegisterValueChangedCallback((evt) => autoSave = evt.newValue);
                _setRootGraphToolbarButton.clicked += ChangeRootGraph;
                _generateScriptToolbarButton.clicked += OpenCreateScriptWindow;
                _clearGraphToolbarButton.clicked += OpenClearGraphWindow;

                SetUpAddGraphMenu();

            }
            UpdateGraphSelectionToolbar();
        }

        void SetUpAddGraphMenu()
        {
            var addGraphMenu = rootVisualElement.Q<ToolbarMenu>("bw-toolbar-add-menu");
            typeof(GraphAdapter).GetSubClasses().ForEach(adapterType =>
            {
                var adapterAttribute = adapterType.GetCustomAttribute<CustomAdapterAttribute>();
                if (adapterAttribute != null)
                {
                    var graphType = adapterAttribute.type;
                    if (graphType.IsSubclassOf(typeof(BehaviourGraph)))
                    {
                        addGraphMenu.menu.AppendAction(graphType.Name, _ => CreateGraph($"new {graphType.Name}", graphType));
                    }
                }
            });
        }

        void UpdateGraphSelectionToolbar()
        {
            _selectGraphToolbarMenu.menu.MenuItems().Clear();

            if (SystemAsset == null) return;

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

        private void OpenCreateScriptWindow()
        {
            ScriptCreationWindow.Create();
        }

        private void OpenClearGraphWindow()
        {
            if (_currentGraphAsset == null) return;
            AlertWindow.CreateAlertWindow("¿Clear current graph?", ClearCurrentGraph);
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
            if (SystemAsset == null || SystemAsset.Graphs.Count == 0) return;
            AlertWindow.CreateAlertWindow("Are you sure to delete the current graph?", DeleteCurrentGraph);
        }

        void ChangeRootGraph()
        {
            if (_currentGraphAsset == SystemAsset.RootGraph) return;
            SystemAsset.RootGraph = _currentGraphAsset;
            UpdateGraphSelectionToolbar();
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
                DisplayGraph(SystemAsset.RootGraph);
            }
            else
            {
                _graphView.ClearView();
                _emptyGraphPanel.style.display = DisplayStyle.Flex;
            }


            Toast("Graph deleted");
        }

        #endregion

        private void Toast(string message, float timeout = .5f)
        {
            ShowNotification(new GUIContent(message), timeout);
        }
    }
}