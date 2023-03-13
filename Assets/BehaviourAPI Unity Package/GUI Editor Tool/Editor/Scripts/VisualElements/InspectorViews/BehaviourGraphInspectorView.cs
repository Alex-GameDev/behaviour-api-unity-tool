using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class GraphInspector : Inspector<GraphData>
    {
        public GraphInspector() : base("Graph", Side.Right)
        {
        }

        public override void UpdateInspector(GraphData element)
        {
            base.UpdateInspector(element);

            var index = BehaviourEditorWindow.Instance.System.Data.graphs.IndexOf(element);

            if (index == -1) return;

            var path = GetPropertyPath(index);

            IMGUIContainer container = new IMGUIContainer(() =>
            {
                var obj = new SerializedObject(BehaviourEditorWindow.Instance.System.ObjectReference);

                EditorGUILayout.PropertyField(obj.FindProperty(path + ".name"));
                EditorGUILayout.Space(10f);
 
                var prop = obj.FindProperty(path + ".graph");
                var end = obj.FindProperty(path + ".nodes");
                bool child = true;
                while (prop.Next(child) && !SerializedProperty.EqualContents(prop, end))
                {
                    EditorGUILayout.PropertyField(prop, true);
                    child = false;
                }
                obj.ApplyModifiedProperties();

            });
            _inspectorContent.Add(container);
        }

        string GetPropertyPath(int graphIndex)
        {
            return $"data.graphs.Array.data[{graphIndex}]";
        }
    }
}
