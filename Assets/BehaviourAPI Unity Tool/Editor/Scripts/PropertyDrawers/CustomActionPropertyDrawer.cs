using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomPropertyDrawer(typeof(CustomAction))]
    public class CustomActionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var componentProperty = property.FindPropertyRelative("component");
            if (!DisplayComponentProperty(componentProperty)) return;
            var methodNameProperty = property.FindPropertyRelative("methodName");
            DisplayMethodNameProperty(methodNameProperty, componentProperty.objectReferenceValue as Component);
        }

        private bool DisplayComponentProperty(SerializedProperty componentProperty)
        {
            EditorGUILayout.ObjectField(componentProperty);
            return componentProperty.objectReferenceValue != null;
        }

        private void DisplayMethodNameProperty(SerializedProperty methodNameProperty, Component component)
        {
            var methodName = methodNameProperty.stringValue;
            var methods = component.GetType().GetMethods().ToList()
                .FindAll(x => x.ReturnParameter.ParameterType == typeof(Status))
                .FindAll(x => x.GetParameters().Length == 0);

            var methodNames = methods.Select(x => x.Name).ToList();

            int methodNameIndex = methodNames.FindIndex(str => str.Equals(methodName));
            methodNameIndex = EditorGUILayout.Popup(methodNameIndex, methodNames.ToArray());

            if (methodNameIndex != -1)
            {
                methodNameProperty.stringValue = methodNames[methodNameIndex];
            }
        }
    }
}
