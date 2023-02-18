using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BehaviourAPI.Unity.Runtime
{
    public class EditorBehaviourRunner : BehaviourRunner, IBehaviourSystem, ISerializationCallbackReceiver
    {
        public NamingSettings nodeNamingSettings = NamingSettings.IgnoreWhenInvalid;
        public NamingSettings perceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;
        public NamingSettings pullPerceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;

        [HideInInspector] [SerializeField] List<GraphAsset> graphs = new List<GraphAsset>();
        [HideInInspector] [SerializeField] List<PushPerceptionAsset> pushPerceptions = new List<PushPerceptionAsset>();
        [HideInInspector] [SerializeField] List<PerceptionAsset> pullPerceptions = new List<PerceptionAsset>();

        private BehaviourGraph _buildedMainGraph;
        public List<GraphAsset> Graphs => graphs;
        public List<PushPerceptionAsset> PushPerceptions => pushPerceptions;
        public List<PerceptionAsset> PullPerceptions => PullPerceptions;

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

        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            return null;
        }

        protected override void OnAwake()
        {
            if(MainGraph != null)
            {
                BuildSystem();
            }
            else
            {
                Debug.LogWarning("[BehaviourRunner] - This runner has not graphs attached.", this);
                Destroy(this);
            }
        }

        protected override void OnStart()
        {
            if(_buildedMainGraph != null)
            {
                _buildedMainGraph.Start();
            }
            else
            {
                Debug.LogWarning("[BehaviourRunner] - This runner has not graphs attached.", this);
                Destroy(this);
            }
        }

        protected override void OnUpdate()
        {
            if (_buildedMainGraph != null)
            {
                _buildedMainGraph.Update();
            }
            else
            {
                Debug.LogWarning("[BehaviourRunner] - This runner has not graphs attached.", this);
                Destroy(this);
            }
        }

        private void BuildSystem()
        {
            pullPerceptions.ForEach(p => p.Build());
            graphs.ForEach(p => p.Build(nodeNamingSettings));
            pushPerceptions.ForEach(p => p.Build());
        }

        #region --------------------------- Create elements ---------------------------

#if UNITY_EDITOR

        public GraphAsset CreateGraph(string name, Type type)
        {
            var graphAsset = GraphAsset.Create(name, type);

            if (graphAsset != null)
            {
                graphs.Add(graphAsset);
            }

            if(gameObject.scene.name == null)
            {
                AssetDatabase.AddObjectToAsset(graphAsset, gameObject);
            }
            else
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            return graphAsset;
        }

        public PushPerceptionAsset CreatePushPerception(string name)
        {
            var pushPerceptionAsset = PushPerceptionAsset.Create(name);

            if (pushPerceptionAsset != null)
            {
                pushPerceptions.Add(pushPerceptionAsset);
            }
            return pushPerceptionAsset;
        }

        public PerceptionAsset CreatePerception(string name, Type type)
        {
            var perceptionAsset = PerceptionAsset.Create(name, type);

            if (perceptionAsset != null)
            {
                pullPerceptions.Add(perceptionAsset);
            }
            return perceptionAsset;
        }

        public void RemoveGraph(GraphAsset graph)
        {
            if (gameObject.scene.name == null)
            {
                AssetDatabase.RemoveObjectFromAsset(graph);
            }
            else
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            graphs.Remove(graph);
        }

        public void RemovePushPerception(PushPerceptionAsset pushPerception)
        {
            pushPerceptions.Remove(pushPerception);
        }

        public void RemovePerception(PerceptionAsset pushPerception)
        {
            pullPerceptions.Remove(pushPerception);
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
            AssetDatabase.SaveAssetIfDirty(gameObject);
            Debug.Log("Saved");
        }
#endif
        #endregion

        public void OnBeforeSerialize()
        {
            graphs.RemoveAll(g => g == null);
            pushPerceptions.RemoveAll(p => p == null);
            pullPerceptions.RemoveAll(p => p == null);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
