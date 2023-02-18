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

namespace BehaviourAPI.Unity.Editor
{
    public class PushPerceptionInspectorView : ListInspectorView<PushPerceptionAsset>
    {
        ListView _pushHandlerListView;
        public NodeSearchWindow nodeSearchWindow { get; set; }

        public Action<PushPerceptionAsset> PushPerceptionCreated, PushPerceptionRemoved;

        BehaviourSystemAsset _systemAsset;

        public PushPerceptionInspectorView() : base("Push Perceptions", Side.Right)
        {
        }

        public override void AddElement()
        {
            if (_systemAsset == null) return;

            var asset = _systemAsset.CreatePushPerception("new pushperception");
            RefreshList();
            PushPerceptionCreated?.Invoke(asset);
        }

        protected override List<PushPerceptionAsset> GetList()
        {
           if (_systemAsset == null) return new List<PushPerceptionAsset>();
           return _systemAsset.PushPerceptions;
        }

        protected override void RemoveElement(PushPerceptionAsset asset)
        {
            _systemAsset.RemovePushPerception(asset);
            PushPerceptionRemoved?.Invoke(asset);
        }

        public override void UpdateInspector(PushPerceptionAsset asset)
        {
            base.UpdateInspector(asset);
            if (asset == null) return;

            var subtitleLabel = new Label("Targets");
            _inspectorContent.Add(subtitleLabel);

            _pushHandlerListView = new ListView(asset.Targets, -1, MakeItem, BindItem);
            _pushHandlerListView.selectionType = SelectionType.Single;
            _pushHandlerListView.style.maxHeight = new StyleLength(150);
            _pushHandlerListView.style.marginTop = new StyleLength(5);
            _pushHandlerListView.style.marginBottom = new StyleLength(5);

            _inspectorContent.Add(_pushHandlerListView);
            _inspectorContent.Add(new Button(OpenNodeSearchWindow) { text = "Add Target" });
        }

        private void OpenNodeSearchWindow()
        {
            if (nodeSearchWindow == null) Debug.Log("Error");
            nodeSearchWindow.Open(nodeAsset => nodeAsset.Node is IPushActivable && !_selectedElement.Targets.Contains(nodeAsset), AddPushHandler);
        }

        void AddPushHandler(NodeAsset obj)
        {
            _selectedElement.Targets.Add(obj);
            _pushHandlerListView.RefreshItems();
        }

        VisualElement MakeItem()
        {
            var element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(itemPath).Instantiate();
            return element;
        }

        void BindItem(VisualElement element, int id)
        {
            var targetNode = _selectedElement.Targets[id];
            var label = element.Q<Label>("li-name");
            var button = element.Q<Button>("li-remove-btn");
            label.bindingPath = "Name";
            label.Bind(new SerializedObject(targetNode));
            button.clicked += () => RemovePushHandlerListItem(targetNode);
        }

        void RemovePushHandlerListItem(NodeAsset asset)
        {
            _selectedElement.Targets.Remove(asset);
            _pushHandlerListView.RefreshItems();
        }

        public void ForceRefresh()
        {
            RefreshList();
            UpdateInspector(_selectedElement);
        }

        public void SetSystem(BehaviourSystemAsset systemAsset)
        {
            _systemAsset = systemAsset;
            _selectedElement = null;
            ResetList();
            UpdateInspector(null);
        }
    }
}
