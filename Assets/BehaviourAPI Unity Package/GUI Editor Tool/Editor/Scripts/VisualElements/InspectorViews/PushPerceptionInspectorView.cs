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
        public NodeSearchWindow nodeSearchWindow { get; set; }

        IBehaviourSystem System => BehaviourEditorWindow.Instance.System;

        ListView _pushHandlerListView;

        public PushPerceptionInspectorView() : base("Push Perceptions", Side.Right)
        {
        }

        public override void AddElement()
        {
            if (System == null) return;

            var pushPerceptionAsset = PushPerceptionAsset.Create("new PushPerception");

            if (pushPerceptionAsset != null)
            {
                System.PushPerceptions.Add(pushPerceptionAsset);
                BehaviourEditorWindow.Instance.OnAddSubAsset(pushPerceptionAsset);
                RefreshList();
            }
            else
            {
                Debug.LogWarning("Error creating the push perception.");
            }
        }

        protected override List<PushPerceptionAsset> GetList()
        {
           if (System == null) return new List<PushPerceptionAsset>();
           return System.PushPerceptions;
        }

        protected override void RemoveElement(PushPerceptionAsset asset)
        {
            if (System == null) return;

            if (System.PushPerceptions.Remove(asset))
            {
                BehaviourEditorWindow.Instance.OnRemoveSubAsset(asset);
            }            
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
            nodeSearchWindow.OpenWindow(AddPushHandler, nodeAsset => nodeAsset.Node is IPushActivable && !_selectedElement.Targets.Contains(nodeAsset));
        }

        void AddPushHandler(NodeAsset obj)
        {
            _selectedElement.Targets.Add(obj);
            BehaviourEditorWindow.Instance.OnModifyAsset();
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
            BehaviourEditorWindow.Instance.OnModifyAsset();
            _pushHandlerListView.RefreshItems();
        }

        public void ForceRefresh()
        {
            RefreshList();
            UpdateInspector(_selectedElement);
        }
    }
}
