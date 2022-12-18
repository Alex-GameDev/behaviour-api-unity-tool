using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviourAPI.Core;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a node as an unity object
    /// </summary>
    public class NodeAsset : ScriptableObject
    {
        public string Name;

        [SerializeReference] Node node;
        [HideInInspector][SerializeField] Vector2 position;
        [HideInInspector][SerializeField] List<NodeAsset> parents = new List<NodeAsset>();
        [HideInInspector][SerializeField] List<NodeAsset> childs = new List<NodeAsset>();

        [HideInInspector][SerializeField] ActionAsset customAction;
        [HideInInspector][SerializeField] GraphAsset subGraph;
        [HideInInspector][SerializeField] Status exitStatus;

        [HideInInspector][SerializeField] PerceptionAsset customPerception;
        [HideInInspector][SerializeField] NodeAsset targetNode;
        [HideInInspector][SerializeField] Status targetStatus;

        public Node Node { get => node; set => node = value; }

        public Vector2 Position { get => position; set => position = value; }
        public List<NodeAsset> Parents { get => parents; private set => parents = value; }
        public List<NodeAsset> Childs { get => childs; private set => childs = value; }

        #region -------------------- Action --------------------

        public ActionAsset CustomAction { get => customAction; set => customAction = value; }
        public GraphAsset SubGraph { get => subGraph; set => subGraph = value; }
        public Status ExitStatus { get => exitStatus; set => exitStatus = value; }

        #endregion

        #region ------------------ Perception ------------------

        public PerceptionAsset CustomPerception { get => customPerception; set => customPerception = value; }
        public NodeAsset TargetNode { get => targetNode; set => targetNode = value; }
        public Status TargetStatus { get => targetStatus; set => targetStatus = value; }

        #endregion

        public static NodeAsset Create(Type type, Vector2 pos)
        {
            var nodeAsset = CreateInstance<NodeAsset>();
            nodeAsset.Position = pos;
            nodeAsset.Node = (Node)Activator.CreateInstance(type);
            return nodeAsset;
        }

        public bool HasActionAssigned()
        {
            return CustomAction != null || SubGraph != null || ExitStatus != Status.None;
        }

        public bool HasPerceptionAssigned()
        {
            return CustomPerception != null;
        }

        public void ClearPerception()
        {
            CustomPerception = null;
        }

        public void ClearAction()
        {
            CustomAction = null;
            SubGraph = null;
            ExitStatus = Status.None;
        }
    }
}