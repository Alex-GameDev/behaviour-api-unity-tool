using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class PullPerceptionInspectorView : ListInspectorView<PerceptionAsset>
    {
        IBehaviourSystem System => BehaviourEditorWindow.Instance.System;

        public BehaviourGraphView _graphView { get; set; }

        ListView _compoundPerceptionListView;

        public PullPerceptionInspectorView() : base("Perceptions", Side.Right)
        {
        }

        public override void AddElement()
        {
            if (System == null) return;

            _graphView.PerceptionCreationWindow.Open(AddElement);
        }

        void AddElement(Type type)
        {
            System.CreatePerception("new pushperception", type);
            RefreshList();
        }

        protected override List<PerceptionAsset> GetList()
        {
            if (System == null) return new List<PerceptionAsset>();
            return System.PullPerceptions;
        }

        protected override void RemoveElement(PerceptionAsset asset)
        {
            System.RemovePerception(asset);
        }

        public override void UpdateInspector(PerceptionAsset asset)
        {
            base.UpdateInspector(asset);
            if(asset == null) return;

            if(asset is CompoundPerceptionAsset cpa)
            {
                var subtitleLabel = new Label("Sub perceptions");
                _inspectorContent.Add(subtitleLabel);

                _compoundPerceptionListView = new ListView(cpa.subperceptions, -1, MakeItem, BindItem);
                _compoundPerceptionListView.selectionType = SelectionType.Single;
                _compoundPerceptionListView.style.maxHeight = new StyleLength(150);
                _compoundPerceptionListView.style.marginTop = new StyleLength(5);
                _compoundPerceptionListView.style.marginBottom = new StyleLength(5);

                _inspectorContent.Add(_compoundPerceptionListView);
                _inspectorContent.Add(new Button(() => OpenPerceptionSearchWindow(cpa)) { text = "Add Sub Perception" });
            }
        }

        private void OpenPerceptionSearchWindow(CompoundPerceptionAsset cpa)
        {
            if (_graphView.PerceptionSearchWindow == null) Debug.Log("Error");
            _graphView.PerceptionSearchWindow.OpenWindow((p) => AddSubPerceptionAsset(cpa, p), p => p != cpa && !cpa.subperceptions.Contains(p));
        }

        private void AddSubPerceptionAsset(CompoundPerceptionAsset cpa, PerceptionAsset perception)
        {
            cpa.subperceptions.Add(perception);
            BehaviourEditorWindow.Instance.OnModifyAsset();
            _compoundPerceptionListView.RefreshItems();
        }

        VisualElement MakeItem()
        {
            var element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(itemPath).Instantiate();
            return element;
        }

        void BindItem(VisualElement element, int id)
        {
            if(_selectedElement is CompoundPerceptionAsset cpa)
            {
                var targetPerception = cpa.subperceptions[id];
                var label = element.Q<Label>("li-name");
                var button = element.Q<Button>("li-remove-btn");
                label.bindingPath = "Name";
                label.Bind(new SerializedObject(targetPerception));
                button.clicked += () => RemovePushHandlerListItem(cpa, targetPerception);
            }
        }

        void RemovePushHandlerListItem(CompoundPerceptionAsset cpa, PerceptionAsset asset)
        {
            cpa.subperceptions.Remove(asset);
            BehaviourEditorWindow.Instance.OnModifyAsset();
            _compoundPerceptionListView.RefreshItems();
        }

        public void ForceRefresh()
        {
            RefreshList();
            UpdateInspector(_selectedElement);
        }
    }
}
