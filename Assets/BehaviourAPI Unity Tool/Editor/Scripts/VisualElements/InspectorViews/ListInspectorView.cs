using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using BehaviourAPI.Unity.Framework;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class ListInspectorView<T> : InspectorView<T> where T : ScriptableObject
    {
        protected static string itemPath => BehaviourAPISettings.instance.EditorLayoutsPath + "/list item.uxml";
        protected BehaviourSystemAsset _systemAsset;
        public Action<T> OnCreateElement;
        public Action<T> OnRemoveElement;

        ListView _listView;

        public ListInspectorView(BehaviourSystemAsset systemAsset, string title, Side side) : base(title, side)
        {
            if (systemAsset == null) return;

            _systemAsset = systemAsset;
            _listView = AddListView();
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
            _mainContainer.Add(new Button(AddElement) { text = "Add element" });
            return listView;
        }

        public abstract void AddElement();

        VisualElement MakeItem()
        {
            var element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(itemPath).Instantiate();
            return element;
        }

        void BindItem(VisualElement element, int id)
        {
            var actionAsset = GetList()[id];
            var label = element.Q<Label>("li-name");
            var button = element.Q<Button>("li-remove-btn");
            label.bindingPath = "Name";
            label.Bind(new SerializedObject(actionAsset));
            button.clicked += () => RemoveListItem(actionAsset);
        }

        void RemoveListItem(T asset)
        {
            if (_selectedElement == asset) _inspectorContent.Clear();

            RemoveElement(asset);
            OnRemoveElement?.Invoke(asset);
            _listView.RefreshItems();
        }               

        void OnItemsChosen(IEnumerable<object> items)
        {
            if (items.Count() != 1) return;
            var selectedElement = items.First() as T;

            UpdateInspector(selectedElement);
        }
        protected void RefreshList() => _listView.RefreshItems();

        protected abstract List<T> GetList();

        protected abstract void RemoveElement(T asset);
    }
}
