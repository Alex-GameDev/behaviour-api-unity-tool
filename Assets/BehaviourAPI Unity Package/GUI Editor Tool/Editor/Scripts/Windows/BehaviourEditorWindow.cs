using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;

using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Unity <see cref="EditorWindow"/> to edit and debug behaviour systems.
    /// </summary>
    public class BehaviourEditorWindow : EditorWindow
    {
        #region --------------------------------- File paths ----------------------------------

        private static string PATH => BehaviourAPISettings.instance.EditorLayoutsPath + "windows/behavioursystemwindow.uxml";        
        private static string EMPTYPANELPATH => BehaviourAPISettings.instance.EditorLayoutsPath + "emptygraphpanel.uxml";

        #endregion

        #region ------------------------------- public fields --------------------------------

        /// <summary>
        /// The singleton instance of the window.
        /// </summary>
        public static BehaviourEditorWindow Instance;

        /// <summary>
        /// The system that is being edited
        /// </summary>
        public IBehaviourSystem System { get; private set; }

        /// <summary>
        /// Is the window in runtime mode?
        /// </summary>
        public bool IsRuntime { get; private set; }

        /// <summary>
        /// Is <see cref="System"/> an asset.
        /// </summary>
        public bool IsAsset { get; private set; }

        /// <summary>
        /// The current selected graph.
        /// </summary>
        public GraphAsset CurrentGraphAsset { get; private set; }

        #endregion

        #region ------------------------------- private fields --------------------------------

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

        #endregion

        #region --------------------------- Create window methods ----------------------------

        [MenuItem("BehaviourAPI/Open editor window")]
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

        #endregion

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
            _graphView.NodeAdded += OnAddSubAsset;
            _graphView.NodeRemoved += OnRemoveSubAsset;

            rootVisualElement.Q<Button>("im-pullperceptions-btn").clicked += () => ChangeInspector(_pullPerceptionInspector);
            rootVisualElement.Q<Button>("im-graph-btn").clicked += () => ChangeInspector(_graphInspector);
            rootVisualElement.Q<Button>("im-pushperceptions-btn").clicked += () => ChangeInspector(_pushPerceptionInspector);

            // Toolbar:
            SetUpToolbar();

            Undo.undoRedoPerformed += Refresh;
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
            AlertWindow.CreateAlertWindow("�Clear current graph?", ClearCurrentGraph);
        }

        void DisplayDeleteGraphAlertWindow()
        {
            if (System == null || System.Graphs.Count == 0) return;
            AlertWindow.CreateAlertWindow("Are you sure to delete the current graph?", DeleteCurrentGraph);
        }

        #endregion

        #region ---------------------------- Update layout ----------------------------

        /// <summary>
        /// Repaint the window according to <see cref="System"/>.
        /// </summary>
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

        /// <summary>
        /// Change the selected inspector in the left side.
        /// </summary>
        /// <param name="inspector"> The displayed inspector.</param>
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

        /// <summary>
        /// Update the graph selection bar after add, remove or select other graph.
        /// </summary>
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

        /// <summary>
        /// Change the graph referenced in <see cref="CurrentGraphAsset"/> and repaint the graph view.
        /// </summary>
        /// <param name="graphAsset">The new selected graph.</param>
        /// <param name="forceRefresh">True for repaint even if the selected graph didn't changed.</param>
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

        /// <summary>
        /// Create a new <see cref="GraphAsset"/> in <see cref="System"/>.
        /// </summary>
        /// <param name="name">The name of the graph asset.</param>
        /// <param name="type">The type of the <see cref="BehaviourGraph"></see>/> that the asset stores.</param>
        private void CreateGraph(string name, Type type)
        {
            if (System == null)
            {
                Debug.LogWarning("Can't create graph if no system is selected");
                return;
            }

            var graphAsset = GraphAsset.Create(name, type);

            if (graphAsset != null)
            {
                System.Graphs.Add(graphAsset);
                OnAddSubAsset(graphAsset);
            }

            DisplayGraph(graphAsset);
            Toast("Graph created");
        }

        /// <summary>
        /// Removes <see cref="CurrentGraphAsset"/> from the graph asset list in <see cref="System"/>.
        /// </summary>
        private void DeleteCurrentGraph()
        {
            if (System == null)
            {
                Debug.LogWarning("Can't delete graph if no system is selected");
                return;
            }

            if (System.Graphs.Remove(CurrentGraphAsset))
            {
                CurrentGraphAsset.Nodes.ForEach(n => OnRemoveSubAsset(n));
                OnRemoveSubAsset(CurrentGraphAsset);
            }

            DisplayGraph(System.MainGraph);

            Toast("Graph deleted");
        }

        /// <summary>
        /// Sets <see cref="CurrentGraphAsset"/> as the main graph of <see cref="System"/> 
        /// </summary>
        private void ChangeMainGraph()
        {
            if (System == null || System.MainGraph == CurrentGraphAsset) return;
            System.MainGraph = CurrentGraphAsset;
            OnModifyAsset();

            UpdateGraphSelectionToolbar();
            Toast("Main graph changed");
        }

        /// <summary>
        /// Notifies unity that <see cref="System"/> was modified.
        /// </summary>
        public void OnModifyAsset()
        {
            EditorUtility.SetDirty(System.ObjectReference);
        }

        /// <summary>
        /// Notifies unity that <paramref name="subAsset"/> was added to <see cref="System"/> as as sub asset.
        /// </summary>
        /// <param name="subAsset">The sub asset added.</param>
        public void OnAddSubAsset(ScriptableObject subAsset)
        {
            subAsset.name = subAsset.GetType().Name;
            EditorUtility.SetDirty(System.ObjectReference);

            if (System.AssetReference != null)
            {
                AssetDatabase.AddObjectToAsset(subAsset, System.AssetReference);
                AssetDatabase.SaveAssetIfDirty(System.AssetReference);
            }
            else if (System.ComponentReference != null)
            {
                EditorUtility.SetDirty(subAsset);
                if (System.ComponentReference.gameObject.scene.name == null)
                {
                    AssetDatabase.AddObjectToAsset(subAsset, System.ComponentReference);
                    AssetDatabase.SaveAssetIfDirty(this);
                    AssetDatabase.Refresh();
                }                
            }
        }

        /// <summary>
        /// Notifies unity that <paramref name="subAsset"/> was removed from <see cref="System"/>.
        /// </summary>
        /// <param name="subAsset">The sub asset removed.</param>
        public void OnRemoveSubAsset(ScriptableObject subAsset)
        {
            EditorUtility.SetDirty(System.ObjectReference);

            if(System.AssetReference != null)
            {
                AssetDatabase.RemoveObjectFromAsset(subAsset);
                AssetDatabase.SaveAssetIfDirty(System.AssetReference);
            }
            else if(System.ComponentReference != null)
            {
                if (System.ComponentReference.gameObject.scene.name == null)
                {
                    AssetDatabase.RemoveObjectFromAsset(subAsset);
                    AssetDatabase.SaveAssetIfDirty(System.ComponentReference);
                    AssetDatabase.Refresh();
                }
            }
        }

        /// <summary>
        /// Remove all nodes from <see cref="CurrentGraphAsset"/>.
        /// </summary>
        private void ClearCurrentGraph()
        {
            if (System == null || CurrentGraphAsset == null) return;

            _graphView.ClearView();

            if (IsAsset) CurrentGraphAsset.Nodes.ForEach(AssetDatabase.RemoveObjectFromAsset);

            foreach(var nodeasset in CurrentGraphAsset.Nodes)
            {
                OnRemoveSubAsset(nodeasset);
            }

            CurrentGraphAsset.Nodes.Clear();
            Toast("Graph clean");
        }

        #endregion

        /// <summary>
        /// Displays a notification in the current window
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        public void Toast(string message, float timeout = .5f)
        {
            ShowNotification(new GUIContent(message), timeout);
        }

        /// <summary>
        /// Resets the window when unity exits play mode.
        /// </summary>
        /// <param name="playModeStateChange">The unity play mode state.</param>
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
