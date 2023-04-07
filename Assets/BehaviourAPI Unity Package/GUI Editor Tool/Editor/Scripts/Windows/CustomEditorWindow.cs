using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomEditorWindow : EditorWindow
    {
        private static readonly Vector2 k_MinWindowSize = new Vector2(500, 300);
        private static readonly string k_WindowTitle = "Behaviour System Editor";
        private static readonly string k_WindowPath = "gwindow.uxml";

        static readonly string[] k_inspectorOptions = new string[]
        {
            "Graph",
            "Node",
        };

        /// <summary>
        /// The singleton instance of the window.
        /// </summary>
        public static CustomEditorWindow instance { get; private set; }

        /// <summary>
        /// The reference to the system that is currently being edited
        /// </summary>
        public IBehaviourSystem System { get; private set; }

        /// <summary>
        /// True if the window is in runtime mode with editor tools disabled.
        /// </summary>
        public bool IsRuntime { get; private set; }

        #region ---------------------------------- Editor data ----------------------------------

        private SerializedObject serializedObject;
        private SerializedProperty rootProperty;
        private SerializedProperty graphsProperty;
        private SerializedProperty pushPerceptionsProperty;

        private SerializedProperty selectedPushPerceptionProperty;

        private Vector2 pushPerceptionScrollPos, pushPerceptionTargetScrollPos;

        private int inspectorMode = 0;

        private int selectedGraphIndex = -1;
        private List<int> selectedNodeIndexList = new List<int>();

        #endregion

        private DropdownField selectGraphDropdown;

        private GraphDataView graphDataView;

        CreateGraphPanel createGraphPanel;
        GenerateScriptPanel generateScriptPanel;

        ToolPanel currentPanel;

        IMGUIContainer m_InspectorContainer;

        Label m_PathLabel;
        Label m_ModeLabel;

        #region -------------------------------------------- Create window --------------------------------------------

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoOperationPreformed;
            instance = this;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoOperationPreformed;
            instance = null;
        }
        private void OnUndoOperationPreformed()
        {
            UpdateSelectionMenu();
        }

        /// <summary>
        /// Open the window without a behaviour system assigned.
        /// </summary>
        public static void Create()
        {
            CustomEditorWindow window = GetWindow<CustomEditorWindow>();
            window.minSize = k_MinWindowSize;
            window.titleContent = new GUIContent(k_WindowTitle);
        }

        /// <summary>
        /// Open the window from a behaviour system.
        /// </summary>
        /// <param name="system">The system object that will be edited.</param>
        /// <param name="runtime">True if the window is in runtime mode and the editor tools are disabled.</param>
        public static void Create(IBehaviourSystem system, bool runtime = false)
        {
            CustomEditorWindow window = GetWindow<CustomEditorWindow>();
            window.minSize = k_MinWindowSize;
            window.titleContent = new GUIContent(k_WindowTitle);
            window.UpdateSystem(system, runtime);
        }

        private void CreateGUI()
        {
            string windowAssetFullPath = BehaviourAPISettings.instance.EditorLayoutsPath + k_WindowPath;
            var windowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(windowAssetFullPath);

            if (!windowAsset)
            {
                Debug.LogWarning($"Window layout path was not found ({windowAssetFullPath}). Check the path in BehaviourAPISettings script");
                return;
            }

            windowAsset.CloneTree(rootVisualElement);

            selectGraphDropdown = rootVisualElement.Q<DropdownField>("bw-graph-select");
            selectGraphDropdown.RegisterValueChangedCallback(OnSelectedGraphChanges);

            var mainContainer = rootVisualElement.Q("bw-main");

            // Add graph:
            createGraphPanel = new CreateGraphPanel(CreateGraph);
            mainContainer.Add(createGraphPanel);
            createGraphPanel.Disable();
            var addGraphBtn = rootVisualElement.Q<Button>("bw-add-graph-btn");
            addGraphBtn.clicked += OnClickAddGraphBtn;

            // Delete graph
            var deleteGraphBtn = rootVisualElement.Q<Button>("bw-remove-graph-btn");
            deleteGraphBtn.clicked += OnClickRemoveGraphBtn;

            // generate script
            generateScriptPanel = new GenerateScriptPanel(this);
            mainContainer.Add(generateScriptPanel);
            generateScriptPanel.Disable();
            var generateScriptBtn = rootVisualElement.Q<Button>("bw-script-btn");
            generateScriptBtn.clicked += OnClickGenerateScriptBtn;

            // Set main
            var setMainBtn = rootVisualElement.Q<Button>("bw-setmain-graph-btn");
            setMainBtn.clicked += OnClickSetMainBtn;

            // Clear graph
            var clearBtn = rootVisualElement.Q<Button>("bw-clear-graph-btn");
            clearBtn.clicked += OnClickClearBtn;

            m_InspectorContainer = rootVisualElement.Q<IMGUIContainer>("bw-inspector");
            //m_EditorInspector = new EditorInspector();
            //m_InspectorContainer.Add(m_EditorInspector);

            m_InspectorContainer.onGUIHandler = OnGUIHandler;

            graphDataView = new GraphDataView(this);
            graphDataView.StretchToParentSize();
            var graphContainer = rootVisualElement.Q("bw-graph");
            graphContainer.Insert(0, graphDataView);
            graphDataView.DataChanged += () => EditorUtility.SetDirty(System.ObjectReference);
            graphDataView.SelectionNodeChanged += OnSelectionNodeChanged;

            m_PathLabel = rootVisualElement.Q<Label>("bw-path-label");
            m_ModeLabel = rootVisualElement.Q<Label>("bw-mode-label");
        }

        private void OnClickClearBtn()
        {
            if (selectedGraphIndex < 0) return;

            var pos = Event.current.mousePosition + position.position;
            AlertWindow.CreateAlertWindow("Clear selected graph?", pos, ClearSelectedGraph);
        }

        private void OnClickSetMainBtn()
        {
            if (selectedGraphIndex <= 0) return;

            var pos = Event.current.mousePosition + position.position;
            AlertWindow.CreateAlertWindow("Convert selected graph to the main graph?", pos, ChangeMainGraph);
        }

        private void OnSelectionNodeChanged(List<int> selectedNodeIndexList)
        {
            this.selectedNodeIndexList = selectedNodeIndexList;
            inspectorMode = selectedNodeIndexList.Count > 0 ? 1 : 0;
        }

        private void UpdateSystem(IBehaviourSystem system, bool runtime)
        {
            System = system;
            IsRuntime = runtime;

            if (system != null && !runtime)
            {
                m_ModeLabel.text = runtime ? "Runtime" : "Editor";

                if(system.ObjectReference != null)
                {
                    if(AssetDatabase.Contains(system.ObjectReference))
                    {
                        m_PathLabel.text = AssetDatabase.GetAssetPath(system.ObjectReference);
                    }
                    else
                    {
                        m_PathLabel.text = system.ObjectReference.TypeName();
                    }
                }
                else
                {
                    m_PathLabel.text = "-";
                }
                serializedObject = new SerializedObject(system.ObjectReference);

                rootProperty = serializedObject.FindProperty("data");
                graphsProperty = rootProperty.FindPropertyRelative("graphs");
                pushPerceptionsProperty = rootProperty.FindPropertyRelative("pushPerceptions");
                m_InspectorContainer.Enable();
            }
            else
            {
                m_InspectorContainer.Disable();
            }

            UpdateSelectionMenu();
            ChangeSelectedGraph(System.Data.graphs.Count > 0 ? 0 : -1);
        }

        #endregion

        #region ----------------------------------- Toolbar events --------------------------------------

        private void OnClickAddGraphBtn() => ChangeToolPanel(createGraphPanel, true);

        private void OnClickGenerateScriptBtn() => ChangeToolPanel(generateScriptPanel, true);

        private void OnClickRemoveGraphBtn()
        {
            if (System == null || selectedGraphIndex < 0) return;

            var pos = Event.current.mousePosition + position.position;
            var currentGraph = System.Data.graphs[selectedGraphIndex];
            AlertWindow.CreateAlertWindow($"Delete current graph?\n({currentGraph.name}) \n(This action can't be undo.)", pos, DeleteSelectedGraph);
        }

        private void UpdateSelectionMenu()
        {
            selectGraphDropdown.choices.Clear();
            if (System.Data.graphs.Count == 0)
            {
                selectedGraphIndex = -1;
                selectGraphDropdown.value = "-";
            }
            else
            {
                for (int i = 0; i < System.Data.graphs.Count; i++)
                {
                    var graph = System.Data.graphs[i];
                    var graphName = string.IsNullOrWhiteSpace(graph.name) ? "unnamed" : graph.name;
                    selectGraphDropdown.choices.Add((i + 1) + "\\" + System.Data.graphs.Count + " - " + graphName);          
                }

                if(selectedGraphIndex < 0 || selectedGraphIndex >= System.Data.graphs.Count)
                {
                    selectedGraphIndex = 0;
                }

                selectGraphDropdown.value = selectGraphDropdown.choices[selectedGraphIndex];
            }
        }


        private void OnSelectedGraphChanges(ChangeEvent<string> evt)
        {           
            if (evt.previousValue == evt.newValue) return;
            if (selectGraphDropdown.index == selectedGraphIndex) return;

            ChangeSelectedGraph(selectGraphDropdown.index);
        }

        private void ChangeSelectedGraph(int graphIndex)
        {
            //Debug.Log("Selected graph index: " +graphIndex);
            if (graphIndex == -1)
            {
                createGraphPanel.Open(false);
            }
            selectedGraphIndex = graphIndex;
            selectGraphDropdown.index = selectedGraphIndex;

            Undo.ClearUndo(System.ObjectReference);
            selectedNodeIndexList.Clear();
            UpdateGraphView();
        }

        private void ChangeToolPanel(ToolPanel toolPanel, bool canClose)
        {
            if (selectedGraphIndex < 0) return;

            currentPanel?.ClosePanel();
            toolPanel?.Open(canClose);
            currentPanel = toolPanel;
        }

        #endregion

        #region ------------------------------------- Modify data ----------------------------------

        private void CreateGraph(string graphName, Type graphType)
        {
            if (IsRuntime) return;

            //RegisterOperation("Create graph");
            var graphData = new GraphData(graphType);
            graphData.name = graphName;
            System.Data.graphs.Add(graphData);
            serializedObject.Update();

            UpdateSelectionMenu();
            ChangeSelectedGraph(System.Data.graphs.Count - 1);

            ShowNotification(new GUIContent("Graph created"));
            EditorUtility.SetDirty(System.ObjectReference);
        }

        private void DeleteSelectedGraph()
        {
            if (IsRuntime) return;

            //RegisterOperation("Delete graph");
            System.Data.graphs.RemoveAt(selectedGraphIndex);
            serializedObject.Update();

            UpdateSelectionMenu();
            ChangeSelectedGraph(System.Data.graphs.Count - 1);

            ShowNotification(new GUIContent("Graph deleted"));
            EditorUtility.SetDirty(System.ObjectReference);
        }

        private void ChangeMainGraph()
        {
            if (IsRuntime) return;

            if (System.Data.graphs.Count == 0 || selectedGraphIndex == 0) return;

            //RegisterOperation("Change main graph");
            var currentGraph = System.Data.graphs[selectedGraphIndex];
            System.Data.graphs.MoveAtFirst(currentGraph);
            serializedObject.Update();

            UpdateSelectionMenu();
            ChangeSelectedGraph(0);

            ShowNotification(new GUIContent("Graph converted to main"));
            EditorUtility.SetDirty(System.ObjectReference);
        }

        private void ClearSelectedGraph()
        {
            RegisterOperation("Clear graph");
            System.Data.graphs[selectedGraphIndex].nodes.Clear();
            serializedObject.Update();

            selectedNodeIndexList.Clear();
            ShowNotification(new GUIContent("Graph clean"));
            UpdateGraphView();
        }

        #endregion

        #region -------------------------------------- GraphView -----------------------------------

        private void UpdateGraphView()
        {
            //Debug.Log("Update graph view: " + selectedGraphIndex);
            if (selectedGraphIndex >= 0)
            {
                if(!IsRuntime)
                {
                    var nodesProperty = graphsProperty.GetArrayElementAtIndex(selectedGraphIndex).FindPropertyRelative("nodes");
                    graphDataView.UpdateGraph(System.Data.graphs[selectedGraphIndex], nodesProperty);
                }
                else
                {
                    graphDataView.UpdateGraph(System.Data.graphs[selectedGraphIndex]);
                }
            }
            else graphDataView.UpdateGraph(null);
        }

        #endregion

        #region --------------------------------- Editor Inspector ---------------------------------

        private void OnGUIHandler()
        {
            if (System == null || serializedObject == null) return;


            using(var changeCheck = new EditorGUI.ChangeCheckScope())
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");

                DrawInspectorTab();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();                

                if(changeCheck.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    graphDataView.RefreshSelectedNodesProperties();
                    UpdateSelectionMenu();
                }
            }           
        }

        private void DrawInspectorTab()
        {
            inspectorMode = GUILayout.Toolbar(inspectorMode, k_inspectorOptions);

            switch(inspectorMode)
            {
                case 0:
                    DisplayCurrentGraphProperty();
                    DisplayPushPerceptionListProperty();
                    break;
                case 1:
                    DisplayCurrentNodeProperty();
                    break;
            }
        }

        private void DisplayCurrentNodeProperty()
        {
            using(var nodeChangeCheck = new EditorGUI.ChangeCheckScope())
            {
                if (selectedNodeIndexList.Count == 0)
                {
                    EditorGUILayout.HelpBox("No property selected", MessageType.Info);
                }
                else if (selectedNodeIndexList.Count == 1)
                {
                    var nodeIndex = selectedNodeIndexList[0];
                    var selectedGraphProperty = graphsProperty.GetArrayElementAtIndex(selectedGraphIndex);
                    var selectedNodeProperty = selectedGraphProperty.FindPropertyRelative("nodes").GetArrayElementAtIndex(nodeIndex);
                    var nodeProperty = selectedNodeProperty.FindPropertyRelative("node");
                    var nodeType = nodeProperty.managedReferenceValue.TypeName();
                    EditorGUILayout.LabelField("Type", nodeType, EditorStyles.wordWrappedLabel);
                    DrawPropertyField(selectedNodeProperty, "name");
                    DrawAllFieldsWithoutFoldout(nodeProperty);
                }
                else
                {
                    EditorGUILayout.HelpBox("Multiedit is not enabled", MessageType.Info);
                }

                if(nodeChangeCheck.changed)
                {
                    graphDataView.RefreshSelectedNodesProperties();
                }
            }
        }

        private void DisplayCurrentGraphProperty()
        {

            if(selectedGraphIndex >= 0)
            {
                var selectedGraphProperty = graphsProperty.GetArrayElementAtIndex(selectedGraphIndex);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Graph", EditorStyles.centeredGreyMiniLabel);

                EditorGUILayout.LabelField("Type", selectedGraphProperty.FindPropertyRelative("graph").managedReferenceValue.TypeName(), EditorStyles.wordWrappedLabel);
                DrawPropertyField(selectedGraphProperty, "name");
                DrawAllFieldsWithoutFoldout(selectedGraphProperty.FindPropertyRelative("graph"));

                EditorGUILayout.Space(20);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("This system has no graphs. Create a graph to start editing", MessageType.Info);
            }
        }

        private void DisplayPushPerceptionListProperty()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Push perceptions", EditorStyles.centeredGreyMiniLabel);

            pushPerceptionScrollPos = EditorGUILayout.BeginScrollView(pushPerceptionScrollPos, "window", GUILayout.MinHeight(100));
            if (pushPerceptionsProperty == null) return;

            for(int i = 0; i < pushPerceptionsProperty.arraySize; i++)
            {
                SerializedProperty p = pushPerceptionsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(p.displayName, GUILayout.ExpandWidth(true)))
                {
                    selectedPushPerceptionProperty = p;
                }

                if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    selectedPushPerceptionProperty = null;
                    pushPerceptionsProperty.DeleteArrayElementAtIndex(i);

                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Create push perception"))
            {
                int size = pushPerceptionsProperty.arraySize;
                pushPerceptionsProperty.InsertArrayElementAtIndex(size);
                pushPerceptionsProperty.GetArrayElementAtIndex(size).FindPropertyRelative("name").stringValue = "newpushPerception";
            }

            if (selectedPushPerceptionProperty == null) return;

            DrawPropertyField(selectedPushPerceptionProperty, "name");

            pushPerceptionTargetScrollPos = EditorGUILayout.BeginScrollView(pushPerceptionTargetScrollPos, "window", GUILayout.MinHeight(100));

            SerializedProperty targetNodesProperty = selectedPushPerceptionProperty.FindPropertyRelative("targetNodeIds");

            Dictionary<string, NodeData> nodeIdMap = System.Data.GetNodeIdMap();
            for (int i = 0; i < targetNodesProperty.arraySize; i++)
            {
                SerializedProperty p = targetNodesProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(nodeIdMap.GetValueOrDefault(p.stringValue)?.name ?? "missing node");

                if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    targetNodesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add target"))
            {
                var provider = ElementSearchWindowProvider<NodeData>.Create<NodeSearchWindowProvider>((n) => OnSelectTargetNode(targetNodesProperty, n), n => n.node is IPushActivable);
                provider.Data = System.Data;
                SearchWindow.Open(new SearchWindowContext(Event.current.mousePosition + position.position), provider);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void OnSelectTargetNode(SerializedProperty prop, NodeData nodeData)
        {
            int size = prop.arraySize;
            prop.InsertArrayElementAtIndex(size);
            prop.GetArrayElementAtIndex(size).stringValue = nodeData.id;
            prop.serializedObject.ApplyModifiedProperties();
        }

        private void DrawPropertyField(SerializedProperty property, string propName)
        {
            if (property != null)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative(propName), true);
            }
        }

        private void DrawAllFieldsWithoutFoldout(SerializedProperty property)
        {
            int deep = property.propertyPath.Count(c => c == '.');
            foreach (SerializedProperty p in property)
            {
                if (p.propertyPath.Count(c => c == '.') == deep + 1)
                {
                    EditorGUILayout.PropertyField(p, true);
                }
            }
        }

        internal void RegisterOperation(string v)
        {
            Undo.RegisterCompleteObjectUndo(System.ObjectReference, v);
        }

        #endregion
    }

    // TODO: Use undo in add/delete graph operations can throw errors.
    // Undo is disabled for that operations.
}