using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Action = BehaviourAPI.Core.Actions.Action;

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

        #region ---------------------------------- Editor data ----------------------------------

        private IBehaviourSystem System;

        private SerializedObject serializedObject;
        private SerializedProperty rootProperty;
        private SerializedProperty graphsProperty;
        private SerializedProperty pushPerceptionsProperty;

        private SerializedProperty selectedGraphProperty;
        private SerializedProperty selectedNodeProperty;
        private SerializedProperty selectedPushPerceptionProperty;

        private SerializedProperty currentProperty;

        private Vector2 pushPerceptionScrollPos, pushPerceptionTargetScrollPos;

        private int inspectorMode = 0;

        private int selectedGraphIndex = -1;
        private List<int> selectedNodeIndexList = new List<int>();

        #endregion

        private DropdownField selectGraphDropdown;

        private GraphDataView graphDataView;

        CreateGraphPanel createGraphPanel;

        #region -------------------------------------------- Create window --------------------------------------------

        public static void Create()
        {
            CustomEditorWindow window = GetWindow<CustomEditorWindow>();
            window.minSize = k_MinWindowSize;
            window.titleContent = new GUIContent(k_WindowTitle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="system"></param>
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

            createGraphPanel = new CreateGraphPanel(CreateGraph);
            mainContainer.Add(createGraphPanel);
            createGraphPanel.Disable();

            var addGraphBtn = rootVisualElement.Q<Button>("bw-add-graph-btn");
            addGraphBtn.clicked += OnClickAddGraphBtn;

            var deleteGraphBtn = rootVisualElement.Q<Button>("bw-remove-graph-btn");
            deleteGraphBtn.clicked += OnClickRemoveGraphBtn;

            IMGUIContainer container = rootVisualElement.Q<IMGUIContainer>("bw-inspector");
            container.onGUIHandler = OnGUIHandler;

            graphDataView = new GraphDataView();
            graphDataView.StretchToParentSize();
            var graphContainer = rootVisualElement.Q("bw-graph");
            graphContainer.Insert(0, graphDataView);
            graphDataView.DataChanged += () => EditorUtility.SetDirty(System.ObjectReference);
            graphDataView.SelectionNodeChanged += OnSelectionNodeChanged;
        }

        private void OnSelectionNodeChanged(List<int> selectedNodeIndexList)
        {
            this.selectedNodeIndexList = selectedNodeIndexList;
            inspectorMode = selectedNodeIndexList.Count > 0 ? 1 : 0;

            if(selectedNodeIndexList.Count == 1)
            {
                selectedNodeProperty = selectedGraphProperty.FindPropertyRelative("nodes").GetArrayElementAtIndex(selectedNodeIndexList[0]);
            }
            else
            {
                selectedNodeProperty = null;
            }
        }

        private void UpdateSystem(IBehaviourSystem system, bool runtime)
        {
            System = system;

            if (system != null)
            {
                serializedObject = new SerializedObject(system.ObjectReference);

                rootProperty = serializedObject.FindProperty("data");
                graphsProperty = rootProperty.FindPropertyRelative("graphs");
                pushPerceptionsProperty = rootProperty.FindPropertyRelative("pushPerceptions");
            }

            if (System.Data.graphs.Count > 0)
            {
                selectGraphDropdown.choices = System.Data.graphs.Select(g => g.name).ToList();
                selectGraphDropdown.index = 0;
            }
            selectGraphDropdown.RegisterValueChangedCallback(OnSelectedGraphChanges);
        }

        #endregion

        #region ----------------------------------- Toolbar events --------------------------------------

        private void OnClickAddGraphBtn()
        {
            createGraphPanel.Enable();
        }

        private void OnClickRemoveGraphBtn()
        {
            if (selectedGraphProperty == null) return;
            AlertWindow.CreateAlertWindow($"Delete current graph?\n({selectedGraphProperty.displayName})", DeleteSelectedGraph);
        }

        private void OnSelectedGraphChanges(ChangeEvent<string> evt)
        {
            if (evt.previousValue == evt.newValue) return;

            selectedGraphIndex = selectGraphDropdown.index;

            if (selectedGraphIndex < 0) selectedGraphProperty = null;
            else selectedGraphProperty = graphsProperty.GetArrayElementAtIndex(selectedGraphIndex);

            UpdateGraphView();
        }

        #endregion

        #region ------------------------------------- Modify data ----------------------------------

        private void CreateGraph(string graphName, Type graphType)
        {
            var graphData = new GraphData(graphType);
            graphData.name = graphName;
            System.Data.graphs.Add(graphData);
            serializedObject.Update();

            selectGraphDropdown.choices = System.Data.graphs.Select(g => g.name).ToList();
            selectedNodeIndexList.Clear();
            selectGraphDropdown.index = selectedGraphIndex;
        }

        private void DeleteSelectedGraph()
        {
            graphsProperty.DeleteArrayElementAtIndex(selectedGraphIndex);
            serializedObject.ApplyModifiedProperties();

            selectedNodeIndexList.Clear();
            if (graphsProperty.arraySize > 0) selectedGraphIndex = 0;
            else selectedGraphIndex = -1;

            UpdateGraphView();
        }

        private void ClearSelectedGraph()
        {
            graphsProperty.FindPropertyRelative("nodes").ClearArray();
            serializedObject.ApplyModifiedProperties();

            selectedNodeIndexList.Clear();

            UpdateGraphView();
        }

        #endregion

        #region -------------------------------------- GraphView -----------------------------------

        private void UpdateGraphView()
        {
            if (selectedGraphIndex >= 0) graphDataView.UpdateGraph(System.Data.graphs[selectedGraphIndex], selectedGraphProperty.FindPropertyRelative("nodes"));
            else graphDataView.UpdateGraph(null);
        }

        #endregion

        #region --------------------------------- Editor Inspector ---------------------------------

        private void OnGUIHandler()
        {
            if (System == null || serializedObject == null) return;

            selectGraphDropdown.choices = System.Data.graphs.Select(g => string.IsNullOrWhiteSpace(g.name) ? "unnamed" : g.name).ToList();
            selectGraphDropdown.index = selectedGraphIndex;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            DrawInspectorTab();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
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
            if (selectedNodeIndexList.Count == 0) return;

            else if(selectedNodeIndexList.Count == 1)
            {
                if(selectedNodeProperty != null)
                {
                    currentProperty = selectedNodeProperty;
                    EditorGUILayout.LabelField("Type", selectedNodeProperty.FindPropertyRelative("node").managedReferenceValue.TypeName(), EditorStyles.wordWrappedLabel);
                    DrawField("name", true);
                    DrawFieldsWithoutFold("node", true);
                }
                else
                {
                    EditorGUILayout.HelpBox("No property selected", MessageType.Info);
                }

            }
            else
            {
                EditorGUILayout.HelpBox("Multiedit is not enabled", MessageType.Info);
            }
        }

        private void DisplayCurrentGraphProperty()
        {
            if(selectedGraphProperty != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Graph", EditorStyles.centeredGreyMiniLabel);
                currentProperty = selectedGraphProperty;

                EditorGUILayout.LabelField("Type", selectedGraphProperty.FindPropertyRelative("graph").managedReferenceValue.TypeName(), EditorStyles.wordWrappedLabel);
                DrawField("name", true);
                DrawFieldsWithoutFold("graph", true);

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

            DrawField(selectedPushPerceptionProperty.FindPropertyRelative("name"));

            pushPerceptionTargetScrollPos = EditorGUILayout.BeginScrollView(pushPerceptionTargetScrollPos, "window", GUILayout.MinHeight(100));

            SerializedProperty targetNodesProperty = selectedPushPerceptionProperty.FindPropertyRelative("targetNodeIds");

            for (int i = 0; i < targetNodesProperty.arraySize; i++)
            {
                SerializedProperty p = targetNodesProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(p.stringValue);

                if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    targetNodesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add target"))
            {
                int size = targetNodesProperty.arraySize;
                targetNodesProperty.InsertArrayElementAtIndex(size);
                targetNodesProperty.GetArrayElementAtIndex(size).stringValue = Guid.NewGuid().ToString();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawField(string propName, bool relative)
        {
            if (relative && currentProperty != null)
            {
                EditorGUILayout.PropertyField(currentProperty.FindPropertyRelative(propName), true);
            }
            else if (serializedObject != null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(propName), true);
            }
        }

        private void DrawFieldsWithoutFold(string propName, bool relative)
        {
            SerializedProperty prop = null;
            if (relative && currentProperty != null)
            {
                prop = currentProperty.FindPropertyRelative(propName);
            }
            else if (serializedObject != null)
            {
                prop = serializedObject.FindProperty(propName);                
            }

            int deep = prop.propertyPath.Count(c => c == '.');
            foreach (SerializedProperty p in prop)
            {
                if (p.propertyPath.Count(c => c == '.') == deep + 1)
                {
                    EditorGUILayout.PropertyField(p, true);

                    // Subgrafos:
                    if (p.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        var typeName = prop.managedReferenceFieldTypename.Split(' ').Last();
                        if (typeName == typeof(Action).FullName && prop.managedReferenceValue is SubgraphAction subgraphAction)
                        {
                            DisplaySubgraphProperty(p.FindPropertyRelative("subgraphId"));
                        }
                    }
                }
            }
        }

        private void DrawField(SerializedProperty property) => EditorGUILayout.PropertyField(property, true);

        private void DisplaySubgraphProperty(SerializedProperty subgraphProperty)
        {
            if(!string.IsNullOrEmpty(subgraphProperty.stringValue))
            {
                var subgraph = System.Data.graphs.Find(g => g.id == subgraphProperty.stringValue);
                if(subgraph != null)
                {
                    EditorGUILayout.LabelField(subgraph.name);
                }
                else
                {
                    EditorGUILayout.LabelField("missing subgraph");
                }
            }
            else
            {
                EditorGUILayout.LabelField("Unnasigned");
            }
        }

        #endregion
    }
}
