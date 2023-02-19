using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Action = BehaviourAPI.Core.Actions.Action;
using Object = UnityEngine.Object;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Displays a <see cref="Action"/> from a <see cref="Node"/> inside a <see cref="NodeView"/>
    /// </summary>
    public class PerceptionContainerView : VisualElement
    {
        NodeAsset nodeAsset;
        string propertyPath;

        NodeView _nodeView;

        Button _assignButton;
        VisualElement _container;

        public PerceptionContainerView(NodeAsset asset, SerializedProperty perceptionProperty, NodeView nodeView)
        {
            nodeAsset = asset;
            propertyPath = perceptionProperty.propertyPath;

            _nodeView = nodeView;
            AddLayout();
            SetUpContextualMenu();
            UpdateView();
        }

        void AddLayout()
        {
            _container = new VisualElement();
            Add(_container);

            _assignButton = new Button(OnAssignPerception) { text = "Assign perception" };
            Add(_assignButton);
        }

        private void SetUpContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Clear perception", dd => ClearPerception(),
                    (_) => GetSerializedProperty().objectReferenceValue != null ?
                    DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
        }

        void OnAssignPerception()
        {
            _nodeView.GraphView.PerceptionSearchWindow.OpenWindow(SetPerception);
        }

        private void ClearPerception()
        {
            UpdatePerceptionValue(null);
            UpdateView();
        }

        void SetPerception(PerceptionAsset perceptionAsset)
        {
            UpdatePerceptionValue(perceptionAsset);
            UpdateView();
        }

        void UpdatePerceptionValue(Object perceptionAsset)
        {
            var obj = new SerializedObject(nodeAsset);
            obj.FindProperty(propertyPath).objectReferenceValue = perceptionAsset;
            obj.ApplyModifiedPropertiesWithoutUndo();
            BehaviourEditorWindow.Instance.OnModifyAsset();
        }

        SerializedProperty GetSerializedProperty()
        {
            var obj = new SerializedObject(nodeAsset);
            return obj.FindProperty(propertyPath);
        }

        void UpdateView()
        {
            var perceptionProperty = GetSerializedProperty();
            if (perceptionProperty.objectReferenceValue == null)
            {
                _assignButton.Enable();
                _container.Clear();
                _container.Disable();
            }
            else
            {
                _assignButton.Disable();
                _container.Enable();

                var perceptionAsset = perceptionProperty.objectReferenceValue as PerceptionAsset;

                var label = new Label($"if {GetPerceptionDescription(perceptionAsset)}");
                label.AddToClassList("node-text");
                _container.Add(label);
            }
        }

        public void RefreshView() => UpdateView();

        public static string GetPerceptionDescription(PerceptionAsset perceptionAsset)
        {
            if (perceptionAsset == null || perceptionAsset.perception == null) return "";
            else
            {
                if(perceptionAsset is CompoundPerceptionAsset cpa)
                {
                    if (cpa.subperceptions.Count == 0) return "false";

                    string separator = (cpa.perception is AndPerception ? " && " : " || ");
                    return $"({cpa.subperceptions.Select(sub => GetPerceptionDescription(sub)).Join(separator)})";
                }
                else if(perceptionAsset is StatusPerceptionAsset spa)
                {
                    if (spa.target != null) return $"check {spa.target.Name} status";
                    else return "check node status";
                }
                else
                {
                    var perception = perceptionAsset.perception;
                    
                    if (perception is UnityPerception up)
                    {
                        return up.DisplayInfo;
                    }
                    else
                    {
                        return "custom perception";
                    }
                }              
            }
        }
    }
}
