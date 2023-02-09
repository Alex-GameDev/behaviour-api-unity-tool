using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [InitializeOnLoad]
    public static class EditorSettings
    {
        static EditorSettings()
        {
            EditorApplication.playModeStateChanged += RefreshBehaviourEditorWindow;
        }

        static void RefreshBehaviourEditorWindow(PlayModeStateChange playModeStateChange)
        {
            if (BehaviourSystemEditorWindow.Instance != null)
                BehaviourSystemEditorWindow.Instance.OnChangePlayModeState(playModeStateChange);
        }
    }
}
