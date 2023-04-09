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
            BehaviourAPISettings.instance.ReloadAssemblies();
        }

        static void RefreshBehaviourEditorWindow(PlayModeStateChange playModeStateChange)
        {
            if (CustomEditorWindow.instance != null)
                CustomEditorWindow.instance.OnChangePlayModeState(playModeStateChange);
        }
    }
}
