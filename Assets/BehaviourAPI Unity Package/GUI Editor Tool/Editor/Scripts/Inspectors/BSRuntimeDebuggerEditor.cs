using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BSRuntimeDebugger))]
    public class BSRuntimeDebuggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var runtimeDebugger = (BSRuntimeDebugger)target;

            if (GUILayout.Button("Open Window debugger"))
            {
                if(!Application.isPlaying)
                {
                    EditorWindow.GetWindow<BehaviourEditorWindow>().ShowNotification(new GUIContent("Runtime debugger must be opened in play mode"));
                }
                else
                {
                    BehaviourEditorWindow.OpenSystem(runtimeDebugger, runtime: true);
                    //else EditorWindow.GetWindow<BehaviourEditorWindow>().ShowNotification(new GUIContent("Runtime debugger is not ready"));
                }
            }
        }
    }
}
