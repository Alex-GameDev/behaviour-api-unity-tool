using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using Core.Perceptions;
    using Framework.Adaptations;

    [CustomPropertyDrawer(typeof(CompoundPerceptionWrapper))]
    public class CompoundPerceptionPropertyDrawer : PropertyDrawer
    {
        Vector2 _scrollPos;

        private void AddSubPerception(SerializedProperty arrayProperty, System.Type perceptionType)
        {
            arrayProperty.arraySize++;
            var lastElementProperty = arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1).FindPropertyRelative("perception");

            if (perceptionType.IsSubclassOf(typeof(CompoundPerception)))
            {
                var compound = (CompoundPerception)System.Activator.CreateInstance(perceptionType);
                lastElementProperty.managedReferenceValue = new CompoundPerceptionWrapper(compound);
            }
            else
            {
                lastElementProperty.managedReferenceValue = (Perception)System.Activator.CreateInstance(perceptionType);
            }

            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue != null)
            {
                var compoundPerceptionProperty = property.FindPropertyRelative("compoundPerception");
                var subPerceptionProperty = property.FindPropertyRelative("subPerceptions");

                EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width));
                var labelsize = position.width - 50;

                EditorGUILayout.LabelField(compoundPerceptionProperty.managedReferenceValue?.TypeName(), GUILayout.Width(220));

                bool removed = false;
                if (GUILayout.Button("X"))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    removed = true;
                }
                EditorGUILayout.EndHorizontal();

                if (!removed)
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

                    if (subPerceptionProperty != null)
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
            }
        }
    }
}
