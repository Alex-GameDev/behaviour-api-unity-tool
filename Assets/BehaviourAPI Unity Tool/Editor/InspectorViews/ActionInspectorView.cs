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
    public class ActionInspectorView : InspectorView<ActionAsset>
    {
        BehaviourSystemAsset _systemAsset;

        public Action<Type> OnCreateAction;
        public Action<ActionAsset> OnRemoveAction;

        ListView _actionListView;

        public ActionInspectorView(BehaviourSystemAsset systemAsset) : base("Actions", Side.Right)
        {
            _systemAsset = systemAsset;

            if (_systemAsset == null) return;

            var actionListView = SetUpListView();
            _mainContainer.Add(actionListView);
        }

        protected override void AddLayout()
        {
            base.AddLayout();
            _mainContainer.Add(new Button(CreateCustomAction) { text = "Create custom action" });
            _mainContainer.Add(new Button(CreateUnityAction) { text = "Create unity action" });
        }            

        ListView SetUpListView()
        {
            _actionListView = new ListView(_systemAsset.Actions, -1, MakeItem, BindItem);
            _actionListView.selectionType = SelectionType.Single;
            _actionListView.onItemsChosen += OnItemsChosen;
            _actionListView.onSelectionChange += OnItemsChosen;
            _actionListView.style.maxHeight = new StyleLength(150);
            _actionListView.style.marginTop = new StyleLength(5);
            _actionListView.style.marginBottom = new StyleLength(5);
            return _actionListView;
        }
         
        VisualElement MakeItem()
        {
            var element = VisualSettings.GetOrCreateSettings().ListItemLayout.Instantiate();
            return element;
        }

        void BindItem(VisualElement element, int id)
        {
            var actionAsset = _systemAsset.Actions[id];
            var label = element.Q<Label>("li-name");
            var button = element.Q<Button>("li-remove-btn");
            label.bindingPath = "Name";
            label.Bind(new SerializedObject(actionAsset));
            button.clicked += () => RemoveAction(actionAsset);
        }

        void CreateCustomAction()
        {
            OnCreateAction?.Invoke(typeof(CustomActionAsset));
            _actionListView.RefreshItems();
        }

        void CreateUnityAction()
        {

        }

        void RemoveAction(ActionAsset actionAsset)
        {
            if(_selectedElement == actionAsset) _inspectorContent.Clear();

            OnRemoveAction?.Invoke(actionAsset);
            _actionListView.RefreshItems();
        }

        void OnItemsChosen(IEnumerable<object> items)
        {
            if (items.Count() != 1) return;
            var selectedAction = items.First() as ActionAsset;

            UpdateInspector(selectedAction);
        }
    }
}
