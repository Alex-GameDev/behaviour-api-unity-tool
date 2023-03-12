using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.VersionControl;
using System;
using UnityEngine;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System.Linq;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Inspector for push perception in a behaviour system
    /// </summary>
    public class PushPerceptionInspectorView : Inspector<PushPerceptionData>
    {
        static string itemPath => BehaviourAPISettings.instance.EditorLayoutsPath + "/listitem.uxml";

        IBehaviourSystem System => BehaviourEditorWindow.Instance.System;

        ListView _listView, _targetListView;

        public PushPerceptionInspectorView() : base("Push Perceptions", Side.Right)
        {
            _listView = AddListView();
            _mainContainer.Add(new Button(AddElement) { text = "Add push perception" });
        }

        ListView AddListView()
        {
            var listView = new ListView(GetList(), -1, MakeItem, BindPushPerceptionItem);
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

        List<PushPerceptionData> GetList() => System?.Data.pushPerceptions;

        void AddElement()
        {
            var pushPerceptionData = new PushPerceptionData();
            BehaviourEditorWindow.Instance.System.Data.pushPerceptions.Add(pushPerceptionData);
            BehaviourEditorWindow.Instance.RegisterChanges();
            ForceRefresh();
        }

        protected void RemoveElement(PushPerceptionData data)
        {
            if (System == null) return;

            if (System.Data.pushPerceptions.Remove(data))
            {
                BehaviourEditorWindow.Instance.RegisterChanges();
            }
            ForceRefresh();
        }

        void OnItemsChosen(IEnumerable<object> items)
        {
            if (items.Count() != 1) return;
            var selectedElement = items.First() as PushPerceptionData;

            UpdateInspector(selectedElement);
        }

        public override void UpdateInspector(PushPerceptionData element)
        {
            base.UpdateInspector(element);
            if (element == null) return;

            int pIndex = System.Data.pushPerceptions.IndexOf(element);
            _inspectorContent.Add(new IMGUIContainer(() =>
            {
                var propPath = $"data.pushPerceptions.Array.data[{pIndex}].name";
                var obj = new SerializedObject(System.ObjectReference);
                var prop = obj.FindProperty(propPath);
                EditorGUILayout.PropertyField(prop);
                obj.ApplyModifiedProperties();
            }));

            _targetListView = new ListView(element.targetNodeIds, -1, MakeItem, BindTargetItem);
            _targetListView.selectionType = SelectionType.Single;
            _targetListView.style.maxHeight = new StyleLength(150);
            _targetListView.style.marginTop = new StyleLength(5);
            _targetListView.style.marginBottom = new StyleLength(5);

            _inspectorContent.Add(_targetListView);
            _inspectorContent.Add(new Button(SearchTargetNode) { text = "Add target" });
        }

        private void SearchTargetNode()
        {
            BehaviourEditorWindow.Instance.OpenSearchNodeWindow(AddPushHandler, nodeAsset => nodeAsset.node is IPushActivable && !_selectedElement.targetNodeIds.Contains(nodeAsset.id));
        }

        void AddPushHandler(NodeData obj)
        {
            _selectedElement.targetNodeIds.Add(obj.id);
            BehaviourEditorWindow.Instance.RegisterChanges();
            _targetListView.RefreshItems();
        }

        VisualElement MakeItem()
        {
            var element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(itemPath).Instantiate();
            return element;
        }

        void BindPushPerceptionItem(VisualElement element, int id)
        {
            var pushPerceptionData = GetList()[id];
            
            var label = element.Q<Label>("li-name");
            label.bindingPath = $"data.pushPerception.Array.data[{id}].name";
            label.Bind(new SerializedObject(System.ObjectReference));

            var button = element.Q<Button>("li-remove-btn");
            button.clicked += () => RemoveElement(pushPerceptionData);
        }

        void BindTargetItem(VisualElement element, int id)
        {
            var targetNode = _selectedElement.targetNodeIds[id];

            var label = element.Q<Label>("li-name");

            var button = element.Q<Button>("li-remove-btn");
            button.clicked += () => RemovePushHandlerListItem(targetNode);
        }


        void RemovePushHandlerListItem(string targetId)
        {
            _selectedElement.targetNodeIds.Remove(targetId);
            BehaviourEditorWindow.Instance.RegisterChanges();
            _targetListView.RefreshItems();
        }

        public void ForceRefresh()
        {
            RefreshList();
            UpdateInspector(_selectedElement);
        }

        void RefreshList()
        {
            _listView.itemsSource = GetList();
            _listView.RefreshItems();
        }
    }
}
