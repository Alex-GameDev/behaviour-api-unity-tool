using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Core;
using System.Linq;
using System;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BehaviourGraphAsset))]
    public class BehaviourGraphAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BehaviourGraphAsset graphAsset = target as BehaviourGraphAsset;

            if(graphAsset.Graph != null)
            {
                EditorGUILayout.LabelField($"Root graph: {graphAsset.Graph.GetType().Name}");
                if (GUILayout.Button($"Edit graph"))
                {
                    BehaviourGraphEditorWindow.OpenGraph(graphAsset);
                }
                if (GUILayout.Button($"Clear graph"))
                {
                    graphAsset.Clear();
                }
            }
            else
            {
                var assemblyNames = VisualSettings.GetOrCreateSettings().assemblies;
                var assemblies = assemblyNames.Select(a => System.Reflection.Assembly.Load(a));
                var types = TypeUtilities.GetTypesDerivedFrom(typeof(BehaviourGraph), assemblies);
                types = types.FindAll(t => !t.IsAbstract);

                for(int i = 0; i < types.Count; i++)
                {
                    if (GUILayout.Button($"Create {types[i].Name}"))
                    {
                        graphAsset.BindGraph(types[i]);
                        BehaviourGraphEditorWindow.OpenGraph(graphAsset);
                    }
                }
            }
        }
    }
}
