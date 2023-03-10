using BehaviourAPI.Unity.Editor;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.UnityTool.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.New.Unity.Editor
{
    public class GraphInspector : Inspector<GraphData>
    {

        public GraphInspector() : base("graph", Side.Right)
        {

        }

        public override void UpdateInspector(GraphData element)
        {
            base.UpdateInspector(element);

            var index = EditorWindow.Instance.System.data.graphs.IndexOf(element);

            if (index == -1) return;

            var path = GetPropertyPath(index);

            IMGUIContainer container = new IMGUIContainer(() =>
            {
                var obj = new SerializedObject(EditorWindow.Instance.System);

                EditorGUILayout.PropertyField(obj.FindProperty(path + ".name"));
                EditorGUILayout.Space(10f);
                EditorGUILayout.PropertyField(obj.FindProperty(path + ".graph"), true);

                //var prop = obj.FindProperty(path + ".graph");
                //var end = obj.FindProperty(path + ".nodes");
                //bool child = true;
                //while (prop.Next(child) && !SerializedProperty.EqualContents(prop, end))
                //{
                //    EditorGUILayout.PropertyField(prop, true);
                //    child = false;
                //}
                obj.ApplyModifiedProperties();

            });
            _inspectorContent.Add(container);
        }

        string GetPropertyPath(int graphIndex)
        {
            return $"data.graphs.Array.data[{graphIndex}]";
        }
    }
    public class PushPerceptionInspector : Inspector<PushPerceptionData>
    {
        BehaviourSystemData data;
        protected static string itemPath => BehaviourAPISettings.instance.EditorLayoutsPath + "/listitem.uxml";

        ListView _listView, _targetListView;

        public PushPerceptionInspector() : base("push perceptions", Side.Right)
        {
            _listView = AddListView();
            _mainContainer.Add(new Button(AddElement) { text = "Add element" });
        }

        public override void UpdateInspector(PushPerceptionData element)
        {
            base.UpdateInspector(element);
            _targetListView = new ListView(element.targetNodeIds, -1, MakeItem, BindtargetItem);
            _targetListView.selectionType = SelectionType.Single;
            _targetListView.style.maxHeight = new StyleLength(150);
            _targetListView.style.marginTop = new StyleLength(5);
            _targetListView.style.marginBottom = new StyleLength(5);

            _inspectorContent.Add(_targetListView);
            _inspectorContent.Add(new Button(SearchTargetNode) { text = "Add target" });
        }

        private void BindtargetItem(VisualElement arg1, int arg2)
        {
            throw new NotImplementedException();
        }

        private void SearchTargetNode()
        {
            
        }

        ListView AddListView()
        {
            var listView = new ListView(GetList(), -1, MakeItem, BindItem);
            listView.selectionType = SelectionType.Single;
            listView.onItemsChosen += OnItemsChosen;
            listView.onSelectionChange += OnItemsChosen;
            listView.style.maxHeight = new StyleLength(150);
            listView.style.marginTop = new StyleLength(5);
            listView.style.marginBottom = new StyleLength(5);
            listView.reorderable = true;
            _mainContainer.Add(listView);

            return listView;
        }

        void AddElement()
        {
            var pushPerceptionData = new PushPerceptionData();
            EditorWindow.Instance.System.data.pushPerception.Add(pushPerceptionData);
            EditorWindow.Instance.OnModifyAsset();
            RefreshList();
        }

        void OnItemsChosen(IEnumerable<object> items)
        {
            if (items.Count() != 1) return;
            var selectedElement = items.First() as PushPerceptionData;

            UpdateInspector(selectedElement);
        }

        VisualElement MakeItem()
        {
            var element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(itemPath).Instantiate();
            return element;
        }

        void BindItem(VisualElement element, int id)
        {
            var item = GetList()[id];
            var label = element.Q<Label>("li-name");
            var button = element.Q<Button>("li-remove-btn");
            label.bindingPath = $"data.pushPerception.Array.data[{id}].name";
            label.Bind(new SerializedObject(EditorWindow.Instance.System));
            button.clicked += () => RemoveListItem(item);
        }

        void RemoveListItem(PushPerceptionData item)
        {
            if (_selectedElement == item) _inspectorContent.Clear();

            EditorWindow.Instance.System.data.pushPerception.Remove(item);
            EditorWindow.Instance.OnModifyAsset();
            RefreshList();
        }

        void RefreshList()
        {
            _listView.itemsSource = GetList();
            _listView.RefreshItems();
        }

        private List<PushPerceptionData> GetList() => data.pushPerception;
    }
}
