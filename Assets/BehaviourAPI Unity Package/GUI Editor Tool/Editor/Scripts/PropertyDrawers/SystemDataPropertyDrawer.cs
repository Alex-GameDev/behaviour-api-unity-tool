using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomPropertyDrawer(typeof(SystemData))]
    public class SystemDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.Label(position,"");
        }
    }
}
