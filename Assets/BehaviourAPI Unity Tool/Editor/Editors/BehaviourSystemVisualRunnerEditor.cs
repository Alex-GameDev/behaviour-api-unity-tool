using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BehaviourGraphVisualRunner))]
    public class BehaviourSystemVisualRunnerEditor : UnityEditor.Editor
    {
        private UnityEditor.Editor editor;
        public override void OnInspectorGUI()
        {
            var runner = (BehaviourGraphVisualRunner)target;
            if(runner.SystemAsset == null)
            {
                if(GUILayout.Button("Bind new BehaviourSystem"))
                {
                    runner.SystemAsset = CreateInstance<BehaviourSystemAsset>();
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    Repaint();
                }

                if (GUILayout.Button("Bind BehaviourSystem from Asset"))
                {

                }
            }
            else
            {                
                var asset = serializedObject.FindProperty("SystemAsset");
                CreateCachedEditor(asset.objectReferenceValue, null, ref editor);
                editor.OnInspectorGUI();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
