using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BSRuntimeDebugger))]
    public class BSRuntimeDebuggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Open debugger"))
            {
                if(!Application.isPlaying)
                {
                    EditorWindow.GetWindow<SceneView>().ShowNotification(new GUIContent("Runtime debugger must be opened in play mode"));
                }
            }
        }
    }
}
