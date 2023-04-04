using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class GraphTypeEntry : VisualElement
    {
        public Action<GraphTypeEntry> Selected;

        public Type type;

        private VisualElement container;

        public GraphTypeEntry(Type graphType)
        {
            type = graphType;
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(BehaviourAPISettings.instance.EditorLayoutsPath + "/graphtypeitem.uxml");
            asset.CloneTree(this);

            container = this.Q("gti-main");
            this.Q<Label>("gti-name").text = graphType.Name.CamelCaseToSpaced();
            RegisterCallback<ClickEvent>(OnClick);

            var adapter = GraphAdapter.GetAdapter(graphType);
            if(adapter != null)
            {
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(adapter.IconPath);
                this.Q<VisualElement>("gti-icon").style.backgroundImage = icon;
            }
        }

        public void Select()
        {
            container.ChangeBackgroundColor(new Color(0.3f, 0.3f, 0.3f, 1f));
        }

        public void Unselect()
        {
            container.ChangeBackgroundColor(new Color(0.2f, 0.2f, 0.2f, 1f));
        }

        private void OnClick(ClickEvent evt)
        {
            if(evt.button == 0)
            {
                Selected?.Invoke(this);
            }              
        }
    }
}
