using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.StateMachines;
using BehaviourAPI.StateMachines.StackFSMs;
using BehaviourAPI.UtilitySystems;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class CreateGraphPanel : VisualElement
    {
        Action<string, Type> m_OnCreategraphCallback;

        public GraphTypeEntry SelectedEntry;

        TextField graphNameField;

        public CreateGraphPanel(Action<string, Type> onCreategraphCallback)
        {
            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(BehaviourAPISettings.instance.EditorLayoutsPath + "/creategraphpanel.uxml");
            asset.CloneTree(this);
            this.StretchToParentSize();

            Button createBtn = this.Q<Button>("cgp-create-btn");
            createBtn.clicked += OnCreateButton;

            Button closeBtn = this.Q<Button>("cgp-close-btn");
            closeBtn.clicked += ClosePanel;

            graphNameField = this.Q<TextField>("cgp-name-field");
            
            m_OnCreategraphCallback = onCreategraphCallback;

            ScrollView graphScrollView = this.Q<ScrollView>("cgp-graph-sv");

            foreach(var kvp in BehaviourAPISettings.instance.Metadata.GraphAdapterMap)
            {
                var entry = new GraphTypeEntry(kvp.Key);
                entry.Selected += ChangeSelectedEntry;
                graphScrollView.Add(entry);
            }
        }

        private void ChangeSelectedEntry(GraphTypeEntry entry)
        {
            if (SelectedEntry != null) SelectedEntry.Unselect();
            SelectedEntry = entry;
            if (SelectedEntry != null) SelectedEntry.Select();
        }

        private void OnCreateButton()
        {
            if (SelectedEntry == null) return;

            Type selectedType = SelectedEntry.type;
            Debug.Log(selectedType.Name);
            m_OnCreategraphCallback?.Invoke(graphNameField.value, selectedType);
            ClosePanel();
        }

        public void ClosePanel()
        {
            this.Disable();
        }
    }
}

