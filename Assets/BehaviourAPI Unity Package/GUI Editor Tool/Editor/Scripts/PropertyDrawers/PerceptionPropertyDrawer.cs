using System;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using Core.Perceptions;
    using Framework;
    using Framework.Adaptations;

    [CustomPropertyDrawer(typeof(Perception))]
    public class PerceptionPropertyDrawer : PropertyDrawer
    {
        private void AssignPerception(SerializedProperty property, Type perceptionType)
        {
            if (perceptionType.IsSubclassOf(typeof(CompoundPerception)))
            {
                var compound = (CompoundPerception)Activator.CreateInstance(perceptionType);
                property.managedReferenceValue = new CompoundPerceptionWrapper(compound);
            }
            else
            {
                property.managedReferenceValue = (Perception)Activator.CreateInstance(perceptionType);
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if(property.managedReferenceValue == null)
            {
                if(GUILayout.Button("Assign perception"))
                {
                    SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), ElementCreatorWindowProvider.Create<PerceptionCreationWindow>((pType) => AssignPerception(property, pType)));
                }
            }
            else
            {
                var labelRect = new Rect(position.x, position.y, position.width * 0.8f - 5, position.height);
                var removeRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, position.height);
                EditorGUI.LabelField(labelRect, property.managedReferenceValue.TypeName());
                if(GUI.Button(removeRect, "X"))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }

            }
        }
    }

    [CustomPropertyDrawer(typeof(CustomPerception))]
    public class CustomPerceptionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null)
            {
            }
            else
            {
                EditorGUI.BeginProperty(position, label, property);

                var lineHeight = position.height / 8;
                var space = lineHeight / 3;

                
                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var labelRect = new Rect(position.x, position.y, position.width * 0.8f - 5, lineHeight);
                var removeRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, lineHeight);
                var initComponentRect = new Rect(position.x, position.y + lineHeight + space, position.width, lineHeight);
                var initMethodRect = new Rect(position.x, position.y + lineHeight * 2 + space, position.width, lineHeight);
                var checkComponentRect = new Rect(position.x, position.y + lineHeight * 3 + space * 2, position.width, lineHeight);
                var checkMethodRect = new Rect(position.x, position.y + lineHeight * 4 + space * 2, position.width, lineHeight);
                var resetComponentRect = new Rect(position.x, position.y + lineHeight * 5 + space * 3, position.width, lineHeight);
                var resetMethodRect = new Rect(position.x, position.y + lineHeight * 6 + space * 3, position.width, lineHeight);

                SerializedProperty initComponentProp = property.FindPropertyRelative("init.componentName");
                SerializedProperty initMethodProp = property.FindPropertyRelative("init.methodName");
                SerializedProperty checkComponentProp = property.FindPropertyRelative("check.componentName");
                SerializedProperty checkMethodProp = property.FindPropertyRelative("check.methodName");
                SerializedProperty resetComponentProp = property.FindPropertyRelative("reset.componentName");
                SerializedProperty resetMethodProp = property.FindPropertyRelative("reset.methodName");

                EditorGUI.LabelField(labelRect, "Custom perception", EditorStyles.boldLabel);

                if(GUI.Button(removeRect, "X"))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    EditorGUI.PropertyField(initComponentRect, initComponentProp, new GUIContent("Init - Component"));
                    EditorGUI.PropertyField(initMethodRect, initMethodProp, new GUIContent("Init - Method"));
                    EditorGUI.PropertyField(checkComponentRect, checkComponentProp, new GUIContent("Check - Component"));
                    EditorGUI.PropertyField(checkMethodRect, checkMethodProp, new GUIContent("Check - Method"));
                    EditorGUI.PropertyField(resetComponentRect, resetComponentProp, new GUIContent("Reset - Component"));
                    EditorGUI.PropertyField(resetMethodRect, resetMethodProp, new GUIContent("Reset - Method"));
                }

                EditorGUI.EndProperty();
                EditorGUI.indentLevel = indent;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 8f;
        }
    }

    [CustomPropertyDrawer(typeof(CompoundPerceptionWrapper))]
    public class CompoundPerceptionPropertyDrawer : PropertyDrawer
    {
        Vector2 _scrollPos;

        private void AddSubPerception(SerializedProperty arrayProperty, Type perceptionType)
        {
            arrayProperty.arraySize++;
            var lastElementProperty = arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1).FindPropertyRelative("perception");
            
            if(perceptionType.IsSubclassOf(typeof(CompoundPerception)))
            {
                var compound = (CompoundPerception)Activator.CreateInstance(perceptionType);
                lastElementProperty.managedReferenceValue = new CompoundPerceptionWrapper(compound);
            }
            else
            {
                lastElementProperty.managedReferenceValue = (Perception)Activator.CreateInstance(perceptionType);
            }
           
            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null)
            {
            }
            else
            {
                var compoundPerceptionProperty = property.FindPropertyRelative("compoundPerception");
                var subPerceptionProperty = property.FindPropertyRelative("subPerceptions");

                EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width));
                var labelsize = position.width - 50;

                EditorGUILayout.LabelField(compoundPerceptionProperty.managedReferenceValue?.TypeName(), GUILayout.Width(220));

                bool removed = false;
                if(GUILayout.Button("X"))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    removed = true;
                }
                EditorGUILayout.EndHorizontal();

                if(!removed)
                {
                    if (GUILayout.Button("Add element", EditorStyles.popup))
                    {
                        //subPerceptionProperty.arraySize++;

                        SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), 
                            ElementCreatorWindowProvider.Create<PerceptionCreationWindow>((pType) => AddSubPerception(subPerceptionProperty, pType)));
                        //subPerceptionProperty.GetArrayElementAtIndex(subPerceptionProperty.arraySize - 1).FindPropertyRelative("perception").managedReferenceValue = new CustomPerception();
                        //property.serializedObject.ApplyModifiedProperties();
                    }

                    GUIStyle centeredLabelstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

                    EditorGUILayout.LabelField("Sub perceptions", centeredLabelstyle);
                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "window", GUILayout.MinHeight(300));

                    if(subPerceptionProperty != null)
                    {
                        for (int i = 0; i < subPerceptionProperty.arraySize; i++)
                        {
                            var subperception = subPerceptionProperty.GetArrayElementAtIndex(i);
                            var p = subperception.FindPropertyRelative("perception");

                            EditorGUILayout.PropertyField(p);
                            if (GUILayout.Button("Remove"))
                            {
                                subPerceptionProperty.DeleteArrayElementAtIndex(i);
                                property.serializedObject.ApplyModifiedProperties();
                                break;
                            }
                            EditorGUILayout.Space(5);
                        }
                    }

                    EditorGUILayout.EndScrollView();
                }
                
                //EditorGUI.BeginProperty(position, label, property);

                //var lineHeight = position.height / lines;
                //int indent = EditorGUI.indentLevel;
                //EditorGUI.indentLevel = 0;

                //var compoundPerceptionProperty = property.FindPropertyRelative("compoundPerception");

                //var labelRect = new Rect(position.x, position.y, position.width * 0.8f - 5, lineHeight);
                //var removeRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, lineHeight);

                //var sizeRect = new Rect(position.x, position.y + lineHeight, position.width, lineHeight);

                //if (compoundPerceptionProperty.managedReferenceValue != null)
                //{
                //    var typeName = compoundPerceptionProperty.managedReferenceValue.TypeName();
                //    EditorGUI.LabelField(labelRect, typeName);
                //}
                //else
                //{
                //    EditorGUI.LabelField(labelRect, "missing type");
                //}

                //if (GUI.Button(removeRect, "X"))
                //{
                //    property.managedReferenceValue = null;
                //    property.serializedObject.ApplyModifiedProperties();
                //}
                //else
                //{
                //    var subPerceptionProperty = property.FindPropertyRelative("subPerceptions");
                //    if(GUI.Button(sizeRect, "Add subPerception"))
                //    {
                //        subPerceptionProperty.arraySize++;
                //        subPerceptionProperty.GetArrayElementAtIndex(subPerceptionProperty.arraySize - 1).FindPropertyRelative("perception").managedReferenceValue = new CustomPerception();
                //        property.serializedObject.ApplyModifiedProperties();

                //        //(compoundPerceptionProperty.managedReferenceValue as CompoundPerceptionWrapper).subPerceptions.Add(new PerceptionWrapper());
                //        //EditorUtility.SetDirty()
                //    }

                //    for(int i = 0; i < subPerceptionProperty.arraySize; i++)
                //    {
                //        var rect = new Rect(position.x, position.y + lineHeight * (i * 8 + 2), position.width, lineHeight * 8);
                //        EditorGUI.PropertyField(rect, subPerceptionProperty.GetArrayElementAtIndex(i).FindPropertyRelative("perception"));
                //    }

                //    var deleteRect = new Rect(position.x, position.y + lineHeight * (subPerceptionProperty.arraySize * 8 + 2), position.width, lineHeight);
                //    if (GUI.Button(deleteRect, "Remove last"))
                //    {
                //        if (subPerceptionProperty.arraySize > 0)
                //        {
                //            subPerceptionProperty.DeleteArrayElementAtIndex(subPerceptionProperty.arraySize - 1);
                //            property.serializedObject.ApplyModifiedProperties();
                //        }
                //    }
                //}

                //GUI.BeginScrollView
                //EditorGUI.EndProperty();
                //EditorGUI.indentLevel = indent;
            }
        }

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    var lineCount = 3;
        //    var subPerceptionProp = property.FindPropertyRelative("subPerceptions");
        //    var subElements = subPerceptionProp.arraySize;
        //    for (int i = 0; i < subElements; i++)
        //    {
        //        lineCount += 8;
        //    }

        //    lines = lineCount;
        //    return base.GetPropertyHeight(property, label) * 10;
        //}
    }
}
