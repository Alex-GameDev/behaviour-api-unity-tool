using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class PerceptionInspectorView : InspectorView<PerceptionAsset>
    {
        BehaviourSystemAsset _systemAsset;

        public Action<Type> OnCreatePerception;
        public Action<PerceptionAsset> OnRemovePerception;

        ListView _perceptionListView;

        public PerceptionInspectorView(BehaviourSystemAsset systemAsset) : base("Perceptions", Side.Right)
        {
            _systemAsset = systemAsset;

            if (_systemAsset == null) return;

            _perceptionListView = SetUpListView();
            _mainContainer.Add(_perceptionListView);
        }

        protected override void AddLayout()
        {
            base.AddLayout();
            _mainContainer.Add(new Button(CreateCustomPerception) { text = "Create custom perception" });
            _mainContainer.Add(new Button(CreateUnityAction) { text = "Create unity perception" });
        }            

        ListView SetUpListView()
        {
            var listView = new ListView(_systemAsset.Perceptions, -1, MakeItem, BindItem);
            listView.selectionType = SelectionType.Single;
            listView.onItemsChosen += OnItemsChosen;
            listView.onSelectionChange += OnItemsChosen;
            listView.style.maxHeight = new StyleLength(150);
            listView.style.marginTop = new StyleLength(5);
            listView.style.marginBottom = new StyleLength(5);
            return listView;
        }
         
        VisualElement MakeItem()
        {
            var element = VisualSettings.GetOrCreateSettings().ListItemLayout.Instantiate();
            return element;
        }

        void BindItem(VisualElement element, int id)
        {
            var perceptionAsset = _systemAsset.Perceptions[id];
            var label = element.Q<Label>("li-name");
            var button = element.Q<Button>("li-remove-btn");
            label.bindingPath = "Name";
            label.Bind(new SerializedObject(perceptionAsset));
            button.clicked += () => RemovePerception(perceptionAsset);
        }

        void CreateCustomPerception()
        {
            OnCreatePerception?.Invoke(typeof(CustomPerceptionAsset));
            _perceptionListView.RefreshItems();
        }

        void CreateUnityAction()
        {

        }

        void RemovePerception(PerceptionAsset perceptionAsset)
        {
            if(_selectedElement == perceptionAsset) _inspectorContent.Clear();

            OnRemovePerception?.Invoke(perceptionAsset);
            _perceptionListView.RefreshItems();
        }

        void OnItemsChosen(IEnumerable<object> items)
        {
            if (items.Count() != 1) return;
            var selectedPerception = items.First() as PerceptionAsset;

            UpdateInspector(selectedPerception);
        }
    }
}
