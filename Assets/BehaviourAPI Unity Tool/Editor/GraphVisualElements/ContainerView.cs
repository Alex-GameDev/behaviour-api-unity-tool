using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class ContainerView : VisualElement
    {
        NodeAsset nodeAsset;
        VisualElement _assignDiv;
        VisualElement _actionDiv;
  
        public ContainerView(NodeAsset asset)
        {
            nodeAsset = asset;
            AddLayout();

        }

        void AddLayout()
        {
            var visualTree = VisualSettings.GetOrCreateSettings().ContainerLayout;
            var inspectorFromUXML = visualTree.Instantiate();
            Add(inspectorFromUXML);
            _assignDiv = this.Q("container-assign-div");
            _assignDiv = this.Q("container-action-div");
        }
    }
}
