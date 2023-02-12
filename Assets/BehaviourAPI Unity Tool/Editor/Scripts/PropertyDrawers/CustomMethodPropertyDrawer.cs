using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class CustomMethodPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.LabelField(property.displayName, EditorStyles.largeLabel);
            var componentProperty = property.FindPropertyRelative("component");
            position.y += 20;
            if (!DisplayComponentProperty(componentProperty, position)) return;
            var methodNameProperty = property.FindPropertyRelative("methodName");
            DisplayMethodNameProperty(methodNameProperty, componentProperty.objectReferenceValue as Component, position);
        }

        private bool DisplayComponentProperty(SerializedProperty componentProperty, Rect position)
        {
            EditorGUILayout.ObjectField(componentProperty);
            return componentProperty.objectReferenceValue != null;
        }

        private void DisplayMethodNameProperty(SerializedProperty methodNameProperty, Component component, Rect position)
        {
            var methodName = methodNameProperty.stringValue;
            var methods = component.GetType().GetMethods().ToList()
                .FindAll(x => x.GetCustomAttribute(typeof(CustomMethodAttribute)) != null)
                .FindAll(ValidateMethod);

            var methodNames = methods.Select(x => x.Name).ToList();

            int methodNameIndex = methodNames.FindIndex(str => str.Equals(methodName));
            methodNameIndex = EditorGUILayout.Popup(methodNameIndex, methodNames.ToArray());

            if (methodNameIndex != -1)
            {
                methodNameProperty.stringValue = methodNames[methodNameIndex];
            }
        }

        protected abstract bool ValidateMethod(MethodInfo methodInfo);
    }

    [CustomPropertyDrawer(typeof(SerializedStatusFunction))]
    public class SerializedStatusFunctionPropertyDrawer : CustomMethodPropertyDrawer
    {
        protected override bool ValidateMethod(MethodInfo methodInfo)
        {
            return methodInfo.ReturnParameter.ParameterType == typeof(Status) &&
                methodInfo.GetParameters().Length == 0;
        }
    }

    [CustomPropertyDrawer(typeof(SerializedAction))]
    public class SerializedActionPropertyDrawer : CustomMethodPropertyDrawer
    {
        protected override bool ValidateMethod(MethodInfo methodInfo)
        {
            return methodInfo.ReturnParameter.ParameterType == typeof(void) &&
                methodInfo.GetParameters().Length == 0;
        }
    }

    [CustomPropertyDrawer(typeof(SerializedBoolFunction))]
    public class SerializedBoolFunctionPropertyDrawer : CustomMethodPropertyDrawer
    {
        protected override bool ValidateMethod(MethodInfo methodInfo)
        {
            return methodInfo.ReturnParameter.ParameterType == typeof(bool) &&
                methodInfo.GetParameters().Length == 0;
        }
    }

    [CustomPropertyDrawer(typeof(SerializedFloatFunction))]
    public class SerializedFloatFunctionPropertyDrawer : CustomMethodPropertyDrawer
    {
        protected override bool ValidateMethod(MethodInfo methodInfo)
        {
            return methodInfo.ReturnParameter.ParameterType == typeof(float) &&
               methodInfo.GetParameters().Length == 0;
        }
    }

    [CustomPropertyDrawer(typeof(SerializedFloatFloatFunction))]
    public class SerializedFloatFloatFunctionPropertyDrawer : CustomMethodPropertyDrawer
    {
        protected override bool ValidateMethod(MethodInfo methodInfo)
        {
            return methodInfo.ReturnParameter.ParameterType == typeof(float) &&
               methodInfo.GetParameters().Length == 1 &&
               methodInfo.GetParameters()[0].ParameterType == typeof(float);
        }
    }
}
