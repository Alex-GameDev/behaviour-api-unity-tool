using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.UtilitySystems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;

namespace BehaviourAPI.UnityTool.Framework
{
    public class LayoutHandler
    {
        public static readonly Vector2 nodeOffset = new Vector2(300, 200);

        Dictionary<string, NodeData> _nodeIdMap;
        Dictionary<int, float> levelDistMap;
        Dictionary<NodeData, int> nodeLevelMap;

        public void ComputeLayout(GraphData graphData)
        {
            if (graphData == null) return;

            if (graphData.nodes.Count == 0) return;

            _nodeIdMap = graphData.GetNodeIdMap();

            switch (graphData.graph)
            {
                case BehaviourTree: ComputeTreeLayout(graphData); break;
                case FSM: ComputeCyclicLayout(graphData); break;
                case UtilitySystem: ComputeLayeredLayout(graphData); break;
            }
        }

        #region ------------------------------- Tree layout -------------------------------

        /// <summary>
        /// Compute the layout as a tree graph
        /// </summary>
        /// <param name="graphData"></param>
        void ComputeTreeLayout(GraphData graphData)
        {
            var root = graphData.nodes.First();
            levelDistMap = new Dictionary<int, float>();
            ProcessTreeNode(graphData.nodes.First(), 0, 0);
        }

        /// <summary>
        /// Compute the position of a tree node based on its deep and the horizontal position of the previous nodes.
        /// </summary>

        float ProcessTreeNode(NodeData node, int currentLevel, float targetPos)
        {
            float x;
            if (node.childIds.Count == 0)
            {
                if (levelDistMap.TryGetValue(currentLevel, out float currentLevelValue))
                {
                    x = Mathf.Max(currentLevelValue + 1, targetPos);
                    levelDistMap[currentLevel] = x;
                }
                else
                {
                    x = targetPos;
                    levelDistMap[currentLevel] = targetPos;
                }
            }
            else
            {
                if (levelDistMap.TryGetValue(currentLevel, out float currentLevelValue))
                {
                    targetPos = MathF.Max(targetPos, currentLevelValue + 1);
                }

                float firstX = 0f, lastX = 0f;
                for (int i = 0; i < node.childIds.Count; i++)
                {
                    var child = _nodeIdMap[node.childIds[i]];
                    var childOffset = i - (node.childIds.Count - 1f) / 2f;
                    var childTargetPos = childOffset + targetPos;
                    var realChildPos = ProcessTreeNode(child, currentLevel + 1, childTargetPos);
                    targetPos = Mathf.Max(realChildPos - childOffset, targetPos);
                    if (i == 0) firstX = realChildPos;
                    if (i == node.childIds.Count - 1) lastX = realChildPos;
                }

                x = (firstX + lastX) / 2f;
                levelDistMap[currentLevel] = x;
            }


            node.position = new Vector2(x, currentLevel) * nodeOffset;
            levelDistMap[currentLevel] = x;
            return x;
        }

        #endregion

        #region ------------------------------ Cyclic layout ------------------------------

        void ComputeCyclicLayout(GraphData graphData)
        {
            var nodeCount = graphData.nodes.Count;

            var states = graphData.nodes.FindAll(n => n.node != null && n.node.MaxInputConnections == -1);

            Dictionary<NodeData, HashSet<NodeData>> nearNodeMap = states.ToDictionary(n => n, n => GetNearNodes(n).ToHashSet());

            var orderedStates = states.OrderByDescending(s => nearNodeMap[s].Count);
            Dictionary<NodeData, Vector2Int> statePositionMap = new Dictionary<NodeData, Vector2Int>();

            HashSet<Vector2Int> validPositions = new HashSet<Vector2Int>();
            HashSet<Vector2Int> occupedPositions = new HashSet<Vector2Int>();

            validPositions.Add(new Vector2Int(0, 0));

            var mostConnectedState = orderedStates.First();
            Queue<NodeData> queue = new Queue<NodeData>();
            HashSet<NodeData> visitedNodes = new HashSet<NodeData>();

            queue.Enqueue(mostConnectedState);
            while (queue.Count > 0)
            {
                var currentState = queue.Dequeue();
                visitedNodes.Add(currentState);
                var nearStates = nearNodeMap[currentState];

                var nearStatePos = new List<Vector2Int>();
                foreach (var st2 in nearStates)
                {
                    if (statePositionMap.TryGetValue(st2, out var pos)) nearStatePos.Add(pos);
                    if (!visitedNodes.Contains(st2))
                    {
                        visitedNodes.Add(st2);
                        queue.Enqueue(st2);
                    }
                }

                var bestPos = ComputeBetterPosition(currentState, nearStatePos, validPositions);
                statePositionMap[currentState] = bestPos;
                currentState.position = nodeOffset * bestPos * 2f;
                AddValidPositions(validPositions, occupedPositions, bestPos);
            }

            //Compute transition positions
            foreach (var tr in graphData.nodes.Except(states))
            {
                if (tr.parentIds.Count == 0) return;
                if (tr.childIds.Count == 0)
                {
                    tr.position = _nodeIdMap[tr.parentIds.First()].position + nodeOffset * Vector2.up;
                    //Debug.Log($"Setting position of {tr.Name} above {tr.Parents.First().Name}");
                }
                else if (tr.childIds.Count == 1)
                {
                    tr.position = Vector2.Lerp(_nodeIdMap[tr.parentIds.First()].position, _nodeIdMap[tr.childIds.First()].position, .5f);
                    //Debug.Log($"Setting position of {tr.Name} between its states");
                }
            }
        }

        void AddValidPositions(HashSet<Vector2Int> validPositions, HashSet<Vector2Int> occupedPositions, Vector2Int lastOccupedPos)
        {
            occupedPositions.Add(lastOccupedPos);
            validPositions.Remove(lastOccupedPos);
            int[] hDirs = { 1, 1, 0, -1, -1, -1, 0, 1 };
            int[] vDirs = { 0, 1, 1, 1, 0, -1, -1, -1 };

            for (int i = 0; i < 8; i++)
            {
                var pos = lastOccupedPos + new Vector2Int(hDirs[i], vDirs[i]);
                if (!occupedPositions.Contains(pos))
                {
                    validPositions.Add(pos);
                }
            }
        }

        Vector2Int ComputeBetterPosition(NodeData state, List<Vector2Int> nearStatePos, HashSet<Vector2Int> validPositions)
        {
            var bestPos = new Vector2Int(0, 0);
            var currentMinDist = float.MaxValue;

            if (nearStatePos.Count == 0)
            {
                //Debug.Log("Not near states");
                if (validPositions.Count > 0) return validPositions.First();
                else return bestPos;
            }

            foreach (var validPos in validPositions)
            {
                var dist = nearStatePos.Sum(n => Vector2Int.Distance(validPos, n));
                if (dist < currentMinDist)
                {
                    //Debug.Log("Better pos");
                    bestPos = validPos;
                    currentMinDist = dist;
                }
            }
            return bestPos;
        }

        /// <summary>
        /// Get all nodes connected to <paramref name="nodeData"/> with another node between them.
        /// </summary>
        IEnumerable<NodeData> GetNearNodes(NodeData nodeData)
        {
            return nodeData.childIds.Select(id => _nodeIdMap[id].childIds).SelectMany(idList => idList.Select(id => _nodeIdMap[id]))
                .Union(nodeData.parentIds.Select(id => _nodeIdMap[id].parentIds).SelectMany(idList => idList.Select(id => _nodeIdMap[id])))
                .Distinct();
        }

        #endregion

        #region ------------------------------ Layered layout -----------------------------

        /// <summary>
        /// Compute the layout as a layered graph
        /// </summary>
        void ComputeLayeredLayout(GraphData graphAsset)
        {
            nodeLevelMap = new Dictionary<NodeData, int>();

            foreach (var node in graphAsset.nodes)
            {
                if (!nodeLevelMap.ContainsKey(node)) CheckLevel(node);
            }

            var list = nodeLevelMap.ToList();

            var maxLevel = list.Max(kvp => kvp.Value);

            for (int i = 0; i <= maxLevel; i++)
            {
                List<NodeData> nodes = list.FindAll(kvp => kvp.Value == i).Select(kvp => kvp.Key).ToList();
                var dist = i;
                ComputePositions(nodes, dist);
            }
        }

        /// <summary>
        /// Computes the us node x dist bassed on its child levels max + 1
        /// </summary>
        int CheckLevel(NodeData nodeData)
        {
            int currentLevel = 0;

            for(int i = 0; i < nodeData.childIds.Count; i++)
            {
                NodeData child = _nodeIdMap[nodeData.childIds[i]];
                int childValue = nodeLevelMap.TryGetValue(child, out int level) ? level : CheckLevel(child);
                if (childValue + 1 > currentLevel) currentLevel = childValue + 1;
            }
            nodeLevelMap[nodeData] = currentLevel;
            return currentLevel;
        }

        /// <summary>
        /// Compute the final us node position based on its x level. 
        /// </summary>
        private void ComputePositions(List<NodeData> nodes, int level)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (level == 0)
                {
                    nodes[i].position = new Vector2(level, i) * nodeOffset;
                }
                else
                {
                    nodes[i].position = new Vector2(level, nodes[i].childIds.Select(c => _nodeIdMap[c]).Average(child => child.position.y));
                }
            }

            if (level != 0)
            {
                var midPos = nodes.Average(n => n.position.y);
                if (nodes.Count == 1)
                {
                    var dist = level * nodeOffset.x;
                    nodes[0].position = new Vector2(dist, midPos);
                }
                else
                {
                    nodes = nodes.OrderBy(n => n.position.y).ToList();
                    var dist = level * nodeOffset.x;
                    var midCount = (nodes.Count - 1) / 2f;

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        nodes[i].position = new Vector2(dist, midPos) + nodeOffset * new Vector2(0, i - midCount);
                    }
                }
            }
        }

        #endregion
    }
}
