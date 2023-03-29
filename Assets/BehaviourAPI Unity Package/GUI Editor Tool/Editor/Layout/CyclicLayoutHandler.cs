using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using Framework;

    /// <summary>
    /// 
    /// </summary>
    public class CyclicLayoutHandler : LayoutHandler
    {
        private static readonly Vector2 k_nodeOffset = new Vector2(300, 200);

        Dictionary<NodeData, HashSet<NodeData>> nearNodeMap;

        HashSet<Vector2Int> validPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> occupedPositions = new HashSet<Vector2Int>();
        Dictionary<NodeData, Vector2Int> statePositionMap = new Dictionary<NodeData, Vector2Int>();

        /// <summary>
        /// <inheritdoc/>
        /// 
        /// </summary>
        /// <param name="graphData"><inheritdoc/></param>
        protected override void ComputeLayout(GraphData graphData)
        {
            var states = graphData.nodes.FindAll(n => n.node.MaxInputConnections == -1);

            nearNodeMap = states.ToDictionary(n => n, n => GetNearNodes(n).ToHashSet());

            var orderedStates = states.OrderByDescending(s => nearNodeMap[s].Count);

            validPositions.Add(new Vector2Int(0, 0));

            var mostConnectedState = orderedStates.First();

            Queue<NodeData> queue = new Queue<NodeData>();
            HashSet<NodeData> visitedNodes = new HashSet<NodeData>();

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

                var bestPos = ComputeBetterPosition(currentState, nearStatePos);
                statePositionMap[currentState] = bestPos;
                currentState.position = k_nodeOffset * bestPos * 2f;
                AddValidPositions(bestPos);
            }

            foreach (var tr in graphData.nodes.Except(states))
            {
                if (tr.parentIds.Count == 0) return;
                if (tr.childIds.Count == 0)
                {
                    tr.position = nodeIdMap[tr.parentIds.First()].position + k_nodeOffset * Vector2.up;
                }
                else if (tr.childIds.Count == 1)
                {
                    tr.position = Vector2.Lerp(nodeIdMap[tr.parentIds.First()].position, nodeIdMap[tr.childIds.First()].position, .5f);
                }
            }
        }

        void AddValidPositions(Vector2Int lastOccupedPos)
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

        Vector2Int ComputeBetterPosition(NodeData state, List<Vector2Int> nearStatePos)
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
            return nodeData.childIds.Select(id => nodeIdMap[id].childIds).SelectMany(idList => idList.Select(id => nodeIdMap[id]))
                .Union(nodeData.parentIds.Select(id => nodeIdMap[id].parentIds).SelectMany(idList => idList.Select(id => nodeIdMap[id])))
                .Distinct();
        }
    }
}
