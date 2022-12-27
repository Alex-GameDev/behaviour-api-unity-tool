using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class SubgraphActionView : ActionView<SubgraphAction>
    {
        VisualElement _emptyDiv, _assignedDiv;
        Label _subgraphLabel;

        public SubgraphActionView(SubgraphAction subgraphAction) :
            base(subgraphAction, VisualSettings.GetOrCreateSettings().UnityActionLayout)
        {
            UpdateLayout();
        }
        protected override void AddLayout()
        {
            this.Q<Button>("sgc-set-btn").clicked += OpenGraphSelectionMenu;
            this.Q<Button>("sgc-remove-btn").clicked += RemoveSubgraph;

            _emptyDiv = this.Q("sgc-empty-div");
            _assignedDiv = this.Q("sgc-assigned-div");
            _subgraphLabel = this.Q<Label>("sgc-graph-label");

            _subgraphLabel.bindingPath = "Name";
        }

        void OpenGraphSelectionMenu()
        {
            // TODO: Añadir menú para elegir subgrafo y llamar al método SetSubgraph
        }

        void SetSubgraph(GraphAsset graphAsset)
        {
            _action.Subgraph = graphAsset;
            UpdateLayout();
        }

        void RemoveSubgraph()
        {
            _action.Subgraph = null;
            UpdateLayout();
        }

        void UpdateLayout()
        {
            var subgraph = _action.Subgraph;
            if (subgraph != null)
            {
                _subgraphLabel.Bind(new UnityEditor.SerializedObject(_action.Subgraph));
                _assignedDiv.style.display = DisplayStyle.Flex;
                _emptyDiv.style.display = DisplayStyle.None;
            }
            else
            {
                _subgraphLabel.Unbind();
                _assignedDiv.style.display = DisplayStyle.None;
                _emptyDiv.style.display = DisplayStyle.Flex;
            }

        }
    }
}
