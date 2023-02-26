using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class EditorBehaviourRunner : DataBehaviourRunner, IBehaviourSystem, ISerializationCallbackReceiver
    {

        [HideInInspector] [SerializeField] List<GraphAsset> graphs = new List<GraphAsset>();
        [HideInInspector] [SerializeField] List<PushPerceptionAsset> pushPerceptions = new List<PushPerceptionAsset>();
        [HideInInspector] [SerializeField] List<PerceptionAsset> pullPerceptions = new List<PerceptionAsset>();

        public List<GraphAsset> Graphs => graphs;
        public List<PushPerceptionAsset> PushPerceptions => pushPerceptions;
        public List<PerceptionAsset> PullPerceptions => pullPerceptions;

        /// <summary>
        /// The main graph of the runner
        /// </summary>
        public GraphAsset MainGraph
        {
            get
            {
                if (graphs.Count == 0) return null;
                else return graphs[0];
            }
            set
            {
                if (graphs.Contains(value))
                {
                    graphs.MoveAtFirst(value);
                    EditorUtility.SetDirty(this);
                }
            }
        }

        protected override BehaviourSystemAsset GetEditorSystem()
        {
            return BehaviourSystemAsset.CreateSystem(Graphs, PullPerceptions, PushPerceptions);
        }       

        #region --------------------------- Create elements ---------------------------

#if UNITY_EDITOR

        public GraphAsset CreateGraph(string name, Type type)
        {
            var graphAsset = GraphAsset.Create(name, type);

            if (graphAsset != null)
            {
                graphs.Add(graphAsset);
                AddSubElement(graphAsset);
            }

            return graphAsset;
        }

        public PushPerceptionAsset CreatePushPerception(string name)
        {
            var pushPerceptionAsset = PushPerceptionAsset.Create(name);

            if (pushPerceptionAsset != null)
            {
                pushPerceptions.Add(pushPerceptionAsset);
                AddSubElement(pushPerceptionAsset);
            }

            return pushPerceptionAsset;
        }

        public PerceptionAsset CreatePerception(string name, Type type)
        {
            var perceptionAsset = PerceptionAsset.Create(name, type);

            if (perceptionAsset != null)
            {
                pullPerceptions.Add(perceptionAsset);
                AddSubElement(perceptionAsset);
            }

            return perceptionAsset;
        }

        public void OnSubAssetCreated(ScriptableObject asset)
        {
            if(asset != null) AddSubElement(asset);
        }

        public void OnSubAssetRemoved(ScriptableObject asset)
        {
            if (asset != null) RemoveSubElement(asset);
        }

        public void RemoveGraph(GraphAsset graph)
        {
            if (graphs.Remove(graph))
            {
                graph.Nodes.ForEach(n => RemoveSubElement(n));
                RemoveSubElement(graph);
            }
        }

        public void RemovePushPerception(PushPerceptionAsset pushPerception)
        {
            if(pushPerceptions.Remove(pushPerception))
            {
                RemoveSubElement(pushPerception);
            };
        }

        public void RemovePerception(PerceptionAsset pullPerception)
        {
            if (pullPerceptions.Remove(pullPerception))
            {
                RemoveSubElement(pullPerception);
            }
        }

        public void Save()
        {
            if(!PrefabUtility.IsPartOfAnyPrefab(this))
            {
                if(!EditorSceneManager.IsPreviewScene(gameObject.scene))
                {
                    EditorSceneManager.SaveScene(gameObject.scene);
                }
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(gameObject);
            AssetDatabase.Refresh();
        }

        private void AddSubElement(ScriptableObject scriptable)
        {
            if (gameObject.scene.name == null)
            {
                scriptable.name = scriptable.GetType().Name;
                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(scriptable);
                AssetDatabase.AddObjectToAsset(scriptable, this);
                AssetDatabase.SaveAssetIfDirty(this);
                AssetDatabase.Refresh();
            }
            else
            {
                EditorUtility.SetDirty(this);
            }
        }

        private void RemoveSubElement(ScriptableObject scriptable)
        {
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(scriptable);

            if (gameObject.scene.name == null)
            {
                AssetDatabase.RemoveObjectFromAsset(scriptable);
                AssetDatabase.SaveAssetIfDirty(this);
                AssetDatabase.Refresh();
            }
        }

        public void OnModifyAsset()
        {
            EditorUtility.SetDirty(this);
        }

#endif
        #endregion

        public void OnBeforeSerialize()
        {
            return;
        }

        public void OnAfterDeserialize()
        {
            graphs.RemoveAll(g => g == null);
            pushPerceptions.RemoveAll(p => p == null);
            pullPerceptions.RemoveAll(p => p == null);
        }
    }
}
