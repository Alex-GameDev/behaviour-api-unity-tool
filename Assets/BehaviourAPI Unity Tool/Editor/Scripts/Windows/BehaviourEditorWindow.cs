using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourEditorWindow : EditorWindow
    {
        static string PATH => BehaviourAPISettings.instance.EditorLayoutsPath + "windows/behavioursystemwindow.uxml";        static string EMPTYPANELPATH => BehaviourAPISettings.instance.EditorLayoutsPath + "emptygraphpanel.uxml";

        public static BehaviourEditorWindow Instance;

        public IBehaviourSystem System { get; private set; }
        public bool IsRuntime { get; private set; }
        public bool IsAsset { get; private set; }

        public GraphAsset CurrentGraphAsset { get; private set; }

        BehaviourGraphView _graphView;
        VisualElement _noGraphPanel, _container;

        NodeInspectorView _nodeInspector;

        BehaviourGraphInspectorView _graphInspector;
        PullPerceptionInspectorView _pullPerceptionInspector;
        PushPerceptionInspectorView _pushPerceptionInspector;

        SubgraphSearchWindow _subgraphSearchWindow;
        PerceptionSearchWindow _perceptionSearchWindow;
        NodeSearchWindow nodeSearchWindow;

        Toolbar _editToolbar;
        ToolbarMenu _selectGraphMenu;

        IHidable _currentInspector;

        public static void Open()
        {
            BehaviourEditorWindow window = GetWindow<BehaviourEditorWindow>();
            window.minSize = new UnityEngine.Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor");

            window.Refresh();
        }
        public static void OpenSystem(IBehaviourSystem system, bool runtime = false)
        {
            BehaviourEditorWindow window = GetWindow<BehaviourEditorWindow>();
            window.minSize = new UnityEngine.Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor");

            if (Instance.System != system || Instance.IsRuntime != runtime)
            {
                Instance.System = system;
                Instance.IsAsset = AssetDatabase.Contains((UnityEngine.Object)system);
                Instance.IsRuntime = runtime;
                window.Refresh();
            }
        }
        private void OnEnable()
        {
            Instance = this;
        }

        void OnDisable()
        {
            Instance = null;
            System = null;
            IsRuntime = false;
        }

        #region ----------------------------- Create GUI -----------------------------

        void CreateGUI()
        {
            var windownFromUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PATH).Instantiate();
            rootVisualElement.Add(windownFromUXML);

            _container = rootVisualElement.Q("bw-content");

            // Graphs:
            _noGraphPanel = AddEmptyPanel();
            _graphView = AddGraphView();

            // Inspectors:
            _nodeInspector = AddInspector<NodeInspectorView>();
            _graphInspector = AddInspector<BehaviourGraphInspectorView>(swapable: true);
            _pullPerceptionInspector = AddInspector<PullPerceptionInspectorView>(swapable: true);
            _pushPerceptionInspector = AddInspector<PushPerceptionInspectorView>(swapable: true);
            
            _pullPerceptionInspector._graphView = _graphView;
            _pushPerceptionInspector.nodeSearchWindow = _graphView.NodeSearchWindow;

            _graphView.NodeSelected += _nodeInspector.UpdateInspector;
            _graphView.NodeAdded += OnAddAsset;
            _graphView.NodeRemoved += OnRemoveAsset;

            rootVisualElement.Q<Button>("im-pullperceptions-btn").clicked += () => ChangeInspector(_pullPerceptionInspector);
            rootVisualElement.Q<Button>("im-graph-btn").clicked += () => ChangeInspector(_graphInspector);
            rootVisualElement.Q<Button>("im-pushperceptions-btn").clicked += () => ChangeInspector(_pushPerceptionInspector);

            // Toolbar:
            SetUpToolbar();

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        void OnUndoRedoPerformed()
        {
            Refresh();
        }

        private T AddInspector<T>(bool swapable = false) where T : VisualElement, new()
        {
            var inspector = new T();
            _container.Add(inspector);

            if(swapable)
            {
                inspector.Disable();
            }

            return inspector;
        }

        BehaviourGraphView AddGraphView()
        {
            var graphView = new BehaviourGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Insert(0, graphView);
            return graphView;
        }

        VisualElement AddEmptyPanel()
        {
            var emptyPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(EMPTYPANELPATH).Instantiate();
            _container.Add(emptyPanel);
            return emptyPanel;
        }

        void SetUpToolbar()
        {
            var mainToolbar = rootVisualElement.Q<Toolbar>("bw-toolbar-main");
            _selectGraphMenu = mainToolbar.Q<ToolbarMenu>("bw-toolbar-graph-menu");

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
                    addGraphMenu.menu.AppendAction(graphType.Name, 
                        _ => CreateGraph($"new {graphType.Name}", graphType)
                    );
                }
            }
        }

        void OpenCreateScriptWindow()
        {
            ScriptCreationWindow.Create();
        }


        void OpenClearGraphWindow()
        {
            if (CurrentGraphAsset == null) return;
            AlertWindow.CreateAlertWindow("¿Clear current graph?", ClearCurrentGraph);
        }

        private void SaveSystemData()
        {
            if (System != null) System.Save();
        }

        void DisplayDeleteGraphAlertWindow()
        {
            if (System == null || System.Graphs.Count == 0) return;
            AlertWindow.CreateAlertWindow("Are you sure to delete the current graph?", DeleteCurrentGraph);
        }

        void Refresh()
        {
            if (System != null)
            {
                DisplayGraph(System.MainGraph, forceRefresh: true);
            }
            else
            {
                _graphView?.ClearView();
            }

            _pushPerceptionInspector?.ResetList();
            _pullPerceptionInspector?.ResetList();
            UpdateGraphSelectionToolbar();

            if (IsRuntime)
            {
                _nodeInspector?.Hide();
                _editToolbar?.Hide();
                _container?.Hide();
            }
            else
            {
                _nodeInspector?.Show();
                _editToolbar?.Show();
                _container.Show();
            }
        }

        void ChangeInspector(IHidable inspector)
        {
            _currentInspector?.Hide();
            if (_currentInspector == inspector)
            {
                _currentInspector = null;
            }
            else
            {
                _currentInspector = inspector;
                _currentInspector?.Show();
            }
        }

        #endregion

        #region ---------------------------- Update layout ----------------------------

        void UpdateGraphSelectionToolbar()
        {
            _selectGraphMenu?.menu.MenuItems().Clear();

            if (System == null) return;

            for (int i = 0; i < System.Graphs.Count; i++)
            {
                var graph = System.Graphs[i];

                if(graph != null)
                {
                    var id = i;
                    _selectGraphMenu.menu.AppendAction(
                        actionName: $"{i + 1} - {graph.Name} ({graph.Graph.GetType().Name})",
                        action: _ => DisplayGraph(graph),
                        status: CurrentGraphAsset == graph ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal
                    );
                }                
            }
        }

        void DisplayGraph(GraphAsset graphAsset, bool forceRefresh = false)
        {
            if (graphAsset != null && CurrentGraphAsset == graphAsset && !forceRefresh) return;

            CurrentGraphAsset = graphAsset;

            if (graphAsset != null)
            {
                _noGraphPanel.Disable();
                _graphView.SetGraph(graphAsset);
            }
            else
            {
                _graphView.ClearView();
                _noGraphPanel.Enable();
            }

            _graphInspector.UpdateInspector(graphAsset);

            UpdateGraphSelectionToolbar();
        }

        #endregion

        #region --------------------------------- Modify system ---------------------------------

        void CreateGraph(string name, Type type)
        {
            if (System == null)
            {
                Debug.LogWarning("Can't create graph if no system is selected");
                return;
            }

            var graphAsset = System.CreateGraph(name, type);
            DisplayGraph(graphAsset);
            Toast("Graph created");
        }

        void DeleteCurrentGraph()
        {
            if (System == null)
            {
                Debug.LogWarning("Can't delete graph if no system is selected");
                return;
            }

            System.RemoveGraph(CurrentGraphAsset);

            DisplayGraph(System.MainGraph);

            Toast("Graph deleted");
        }

        private void ChangeMainGraph()
        {
            if (System == null || System.MainGraph == CurrentGraphAsset) return;
            System.MainGraph = CurrentGraphAsset;
            UpdateGraphSelectionToolbar();
            Toast("Main graph changed");
        }

        private void OnRemoveAsset(ScriptableObject obj)
        {
            System.OnSubAssetRemoved(obj);
        }

        private void OnAddAsset(ScriptableObject obj)
        {
            System.OnSubAssetCreated(obj);
        }

        public void OnModifyAsset()
        {
            System.OnModifyAsset();
        }

        void ClearCurrentGraph()
        {
            if (System == null || CurrentGraphAsset == null) return;

            _graphView.ClearView();

            if (IsAsset) CurrentGraphAsset.Nodes.ForEach(AssetDatabase.RemoveObjectFromAsset);

            foreach(var nodeasset in CurrentGraphAsset.Nodes)
            {
                System.OnSubAssetRemoved(nodeasset);
            }

            CurrentGraphAsset.Nodes.Clear();
            Toast("Graph clean");
        }

        #endregion

        public void Toast(string message, float timeout = .5f)
        {
            ShowNotification(new GUIContent(message), timeout);
        }

        public void OnChangePlayModeState(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingPlayMode)
            {
                System = null;
                IsAsset = false;
                IsRuntime = false;
                Refresh();
            }
        }
    }
}
