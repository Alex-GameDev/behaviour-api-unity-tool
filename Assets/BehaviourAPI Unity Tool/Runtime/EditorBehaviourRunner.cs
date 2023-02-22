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

        protected BehaviourGraph _buildedMainGraph;
        public List<GraphAsset> Graphs => graphs;
        public List<PushPerceptionAsset> PushPerceptions => pushPerceptions;
        public List<PerceptionAsset> PullPerceptions => pullPerceptions;

        public BehaviourSystemAsset ExecutionSystem { get; private set; }

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
            return ExecutionSystem;
        }

        protected override void OnAwake()
        {
            if(MainGraph != null)
            {
                ExecutionSystem = BuildSystem();
                _buildedMainGraph = ExecutionSystem.MainGraph.Graph;
                _buildedMainGraph.SetExecutionContext(new UnityExecutionContext(gameObject));
                Debug.Log("Build graph");
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
                Debug.Log("Start graph");
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

        private BehaviourSystemAsset BuildSystem()
        {
            var time = DateTime.Now;
            var duplicator = new Duplicator();
            var originalSystem = BehaviourSystemAsset.CreateSystem(Graphs, PullPerceptions, PushPerceptions);

            var executionSystem = duplicator.Duplicate(originalSystem);

            if (MainGraph.Nodes[0] == executionSystem.MainGraph.Nodes[0]) Debug.LogWarning("Asset didnt copy");
            if (MainGraph.Nodes[0] == executionSystem.MainGraph.Nodes[0]) Debug.LogWarning("Asset didnt copy");
            if (MainGraph.Nodes[0].Node == executionSystem.MainGraph.Nodes[0].Node) Debug.LogWarning("Nodes didnt copy");

            executionSystem.PullPerceptions.ForEach(p => p.Build());
            executionSystem.Graphs.ForEach(p => p.Build(nodeNamingSettings));
            executionSystem.PushPerceptions.ForEach(p => p.Build());
            if (executionSystem.MainGraph.Graph == MainGraph.Graph) Debug.LogError("!!!!!!!!!!!!!!!!");

            Debug.Log("Totaltime: " + (DateTime.Now - time).TotalMilliseconds);
            return executionSystem;
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

        public void RemovePerception(PerceptionAsset pushPerception)
        {
            if (pullPerceptions.Remove(pushPerception))
            {
                RemoveSubElement(pushPerception);
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
            AssetDatabase.SaveAssetIfDirty(gameObject);
        }

        private void AddSubElement(ScriptableObject scriptable)
        {
            if (gameObject.scene.name == null)
            {
                scriptable.name = scriptable.GetType().Name;
                AssetDatabase.AddObjectToAsset(scriptable, gameObject);
                AssetDatabase.Refresh();
            }
            EditorUtility.SetDirty(this);
        }

        private void RemoveSubElement(ScriptableObject scriptable)
        {
            if (gameObject.scene.name == null)
            {
                AssetDatabase.RemoveObjectFromAsset(scriptable);
                AssetDatabase.Refresh();
            }
            DestroyImmediate(scriptable, true);
            EditorUtility.SetDirty(this);
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
