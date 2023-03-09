using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class ExtendedEditorWindow : UnityEditor.EditorWindow
    {
        protected SerializedObject serializedObject;
        protected SerializedProperty currentProperty;

        private string selectedpropertyPath;
        protected SerializedProperty selectedProperty;

        protected void DrawProperties(SerializedProperty prop, bool drawChildren)
        {
            string lastProp = string.Empty;
            foreach(SerializedProperty p in prop)
            {
                if(p.isArray && p.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.BeginHorizontal();
                    p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                    EditorGUILayout.EndHorizontal();

                    if(p.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawProperties(p, drawChildren);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if(!string.IsNullOrEmpty(lastProp) && p.propertyPath.Contains(lastProp)) { continue; }
                    lastProp = p.propertyPath;
                    EditorGUILayout.PropertyField(p, drawChildren);
                }
            }
        }

        protected void DrawSidebar(SerializedProperty prop)
        {
            foreach(SerializedProperty p in prop)
            {
                if(GUILayout.Button(p.displayName))
                {
                    selectedpropertyPath = p.propertyPath;
                }
            }

            if(!string.IsNullOrEmpty(selectedpropertyPath))
            {
                selectedProperty = serializedObject.FindProperty(selectedpropertyPath);
            }
        }

        protected void DrawField(string propName, bool relative)
        {
            if(relative && currentProperty != null)
            {
                EditorGUILayout.PropertyField(currentProperty.FindPropertyRelative(propName), true);
            }
            else if(serializedObject != null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(propName), true);
            }
        }

        protected void Apply()
        {
            serializedObject.ApplyModifiedProperties();
        }

    }
}
