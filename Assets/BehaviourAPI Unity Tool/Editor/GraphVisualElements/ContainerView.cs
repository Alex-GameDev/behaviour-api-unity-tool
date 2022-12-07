using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class ContainerView : VisualElement
    {
        NodeAsset nodeAsset;
        VisualElement _assignDiv, _actionDiv;
        Button _assignActionBtn, _removeActionBtn;
  
        public ContainerView(NodeAsset asset)
        {
            nodeAsset = asset;
            AddLayout();
            if(nodeAsset.ActionAsset != null)
            {
                _assignDiv.style.display = DisplayStyle.None;
            }
            else
            {
                _actionDiv.style.display = DisplayStyle.None;
            }
        }

        void AddLayout()
        {
            var visualTree = VisualSettings.GetOrCreateSettings().ContainerLayout;
            var inspectorFromUXML = visualTree.Instantiate();
            Add(inspectorFromUXML);
            _assignDiv = this.Q("container-assign-div");
            _actionDiv = this.Q("container-action-div");
            _assignActionBtn = this.Q<Button>("container-assign-btn");
            _removeActionBtn = this.Q<Button>("container-remove-btn");

            _assignActionBtn.clicked += AssignAction;
            _removeActionBtn.clicked += RemoveAction;
        }

        void AssignAction()
        {
            var actionAsset = ScriptableObject.CreateInstance<ExitActionAsset>();
            nodeAsset.ActionAsset = actionAsset;
            AssetDatabase.AddObjectToAsset(nodeAsset, actionAsset);
            AssetDatabase.SaveAssets();
            _assignDiv.style.display = DisplayStyle.None;
            _actionDiv.style.display = DisplayStyle.Flex;
        }

        void RemoveAction()
        {
            AssetDatabase.RemoveObjectFromAsset(nodeAsset.ActionAsset);
            nodeAsset.ActionAsset = null;
            AssetDatabase.SaveAssets();
            _assignDiv.style.display = DisplayStyle.Flex;
            _actionDiv.style.display = DisplayStyle.None;
        }
    }
}
