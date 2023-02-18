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

        public IBehaviourSystem System;
        public bool IsRuntime;
        public bool IsAsset;

        GraphAsset _currentGraphAsset;

        BehaviourGraphView _graphView;
        VisualElement _noGraphPanel, _container;

        NodeInspectorView _nodeInspector;

        BehaviourGraphInspectorView _graphInspector;
        PullPerceptionInspectorView _pullPerceptionInspector;
        PushPerceptionInspectorView _pushPerceptionInspector;

        ActionCreationWindow _actionCreationWindow;
        PerceptionCreationWindow _perceptionCreationWindow;

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

            _graphView.NodeSelected += _nodeInspector.UpdateInspector;
            _graphView.NodeAdded += OnAddAsset;
            _graphView.NodeRemoved += OnRemoveAsset;

            _pushPerceptionInspector.PushPerceptionCreated += OnAddAsset;
            _pushPerceptionInspector.PushPerceptionRemoved += OnRemoveAsset;

            _pullPerceptionInspector.PerceptionCreated += OnAddPerception;
            _pullPerceptionInspector.PerceptionRemoved += OnRemovePerception;

            rootVisualElement.Q<Button>("im-pullperceptions-btn").clicked += () => ChangeInspector(_pullPerceptionInspector);
            rootVisualElement.Q<Button>("im-graph-btn").clicked += () => ChangeInspector(_graphInspector);
            rootVisualElement.Q<Button>("im-pushperceptions-btn").clicked += () => ChangeInspector(_pushPerceptionInspector);

            // Toolbar:
            SetUpToolbar();
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
            //_editToolbar.Q<ToolbarButton>("bw-toolbar-clear-btn").clicked += OpenClearGraphWindow;
            _editToolbar.Q<ToolbarButton>("bw-toolbar-delete-btn").clicked += DisplayDeleteGraphAlertWindow;
            //_editToolbar.Q<ToolbarButton>("bw-toolbar-save-btn").clicked += SaveSystemData;
            //_editToolbar.Q<ToolbarButton>("bw-toolbar-generatescript-btn").clicked += OpenCreateScriptWindow;

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
                _graphView.ClearView();
            }
            UpdateGraphSelectionToolbar();
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
            Debug.Log("Update toolbar");
            _selectGraphMenu.menu.MenuItems().Clear();

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
                        status: _currentGraphAsset == graph ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal
                    );
                }                
            }
        }

        void DisplayGraph(GraphAsset graphAsset, bool forceRefresh = false)
        {
            if (graphAsset != null && _currentGraphAsset == graphAsset && !forceRefresh) return;

            _currentGraphAsset = graphAsset;

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
            //OnAddAsset(graphAsset);

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

            System.RemoveGraph(_currentGraphAsset);

            DisplayGraph(System.MainGraph);

            Toast("Graph deleted");
        }

        private void ChangeMainGraph()
        {
            if (System == null || System.MainGraph == _currentGraphAsset) return;
            System.MainGraph = _currentGraphAsset;
            UpdateGraphSelectionToolbar();
            Toast("Main graph changed");
        }

        private void OnAddPerception(PerceptionAsset obj)
        {
            throw new System.NotImplementedException();
        }

        private void OnRemovePerception(PerceptionAsset obj)
        {
            throw new System.NotImplementedException();
        }

        private void OnRemoveAsset(ScriptableObject obj)
        {
            throw new System.NotImplementedException();
        }

        private void OnAddAsset(ScriptableObject obj)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        public void Toast(string message, float timeout = .5f)
        {
            ShowNotification(new GUIContent(message), timeout);
        }
    }
}
