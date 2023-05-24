using UnityEngine;
using UnityEditor;
using System.Linq;

namespace BehaviourAPI.Unity.Editor
{
    using Runtime;
    [CustomEditor(typeof(BehaviourSystem))]
    public class BehaviourSystemAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            BehaviourSystem system = target as BehaviourSystem;

            EditorGUILayout.Space(10f);
            if (GUILayout.Button("OPEN EDITOR"))
            {
                if (Application.isPlaying && !AssetDatabase.Contains(system))
                {
                    EditorWindow.GetWindow<BehaviourSystemEditorWindow>().ShowNotification(new GUIContent("Cannot edit binded behaviour system on runtime"));
                    return;
                }

                BehaviourSystemEditorWindow.Create(system);
            }
        }
    }
}
