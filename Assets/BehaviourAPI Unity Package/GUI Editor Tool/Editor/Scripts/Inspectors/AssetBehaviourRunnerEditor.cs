using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(AssetBehaviourRunner), editorForChildClasses: true)]
    public class AssetBehaviourRunnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AssetBehaviourRunner runner = (AssetBehaviourRunner)target;

            bool isOnScene = runner.gameObject.scene.name != null;
            bool isOnPreviewScene = isOnScene && EditorSceneManager.IsPreviewScene(runner.gameObject.scene);

            if (Application.isPlaying && isOnScene && !isOnPreviewScene)
            {
                if (GUILayout.Button("OPEN DEBUGGER"))
                {
                    BehaviourSystemEditorWindow.Create(runner, runtime: Application.isPlaying);
                }               
            }
        }
    }
}
