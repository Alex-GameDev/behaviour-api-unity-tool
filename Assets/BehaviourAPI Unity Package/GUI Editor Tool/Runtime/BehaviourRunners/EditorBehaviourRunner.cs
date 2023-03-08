using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class EditorBehaviourRunner : DataBehaviourRunner, IBehaviourSystem, ISerializationCallbackReceiver
    {
        #region ------------------------------- Private fields --------------------------------

        [HideInInspector] [SerializeField] List<GraphAsset> graphs = new List<GraphAsset>();
        [HideInInspector] [SerializeField] List<PushPerceptionAsset> pushPerceptions = new List<PushPerceptionAsset>();
        [HideInInspector] [SerializeField] List<PerceptionAsset> pullPerceptions = new List<PerceptionAsset>();

        #endregion

        #region --------------------------------- Properties ----------------------------------

        /// <summary>
        /// The editable graphs of the system. Don't use in code.
        /// </summary>
        public List<GraphAsset> Graphs => graphs;

        /// <summary>
        /// The editable push percepcions of the system. Don't use in code.
        /// </summary>
        public List<PushPerceptionAsset> PushPerceptions => pushPerceptions;

        /// <summary>
        /// The editable perceptions of the system. Don't use in code.
        /// </summary>
        public List<PerceptionAsset> PullPerceptions => pullPerceptions;

        /// <summary>
        /// The main editable graph of the runner. Don't use in code.
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
                }
            }
        }

        public bool IsAsset => false;

        public Object ObjectReference => this;

        public ScriptableObject AssetReference => null;

        public Component ComponentReference => this;

        #endregion

        #region --------------------------------- properties ----------------------------------

        protected override BehaviourSystemAsset GetEditorSystem()
        {
            return BehaviourSystemAsset.CreateSystem(Graphs, PullPerceptions, PushPerceptions);
        }

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
