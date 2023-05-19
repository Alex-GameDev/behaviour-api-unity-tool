using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;

namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Core.Actions;
    using Framework.Adaptations;

    /// <summary>
    /// Default property drawer for actions
    /// </summary>
    [CustomPropertyDrawer(typeof(Action))]
    public class ActionPropertyDrawer : PropertyDrawer
    {
        private void AssignAction(SerializedProperty property, System.Type actionType)
        {
            if (actionType.IsSubclassOf(typeof(CompoundAction)))
            {
                var compound = (CompoundAction)System.Activator.CreateInstance(actionType);
                property.managedReferenceValue = new CompoundActionWrapper(compound);
            }
            else
            {
                property.managedReferenceValue = (Action)System.Activator.CreateInstance(actionType);
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null)
            {
                if (GUILayout.Button("Assign action"))
                {
                    SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                        ElementCreatorWindowProvider.Create<ActionCreationWindow>((aType) => AssignAction(property, aType)));
                }
            }
            else
            {
                var labelRect = new Rect(position.x, position.y, position.width * 0.8f - 5, position.height);
                var removeRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, position.height);
                EditorGUI.LabelField(labelRect, property.managedReferenceValue.TypeName());
                if (GUI.Button(removeRect, "X"))
                {
                    property.managedReferenceValue = null;
                }
                else
                {
                    int deep = property.propertyPath.Count(c => c == '.');
                    foreach (SerializedProperty p in property)
                    {
                        if (p.propertyPath.Count(c => c == '.') == deep + 1)
                        {
                            EditorGUILayout.PropertyField(p, true);
                        }
                    }
                }
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
