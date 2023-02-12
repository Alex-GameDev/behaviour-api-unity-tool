using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.StateMachines;
using BehaviourAPI.UtilitySystems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    public class LayoutUtilities : MonoBehaviour
    {
        public static readonly Vector2 nodeOffset = new Vector2(300, 200);

        public static void ComputeLayout(GraphAsset graphAsset)
        {
            if (graphAsset == null) return;

            if (graphAsset.Nodes.Count == 0) return;

            if (graphAsset.Graph is BehaviourTree)
            {
                var root = graphAsset.Nodes.FirstOrDefault();
                if (root == null) return;
                ProcessTreeNode(root, new Dictionary<int, float>(), 0, 0f);
            }
            else if (graphAsset.Graph is UtilitySystem)
            {
                ComputeUSLayout(graphAsset);
            }
            else if (graphAsset.Graph is FSM)
            {
                ComputeCyclicLayout(graphAsset);
            }
        }

        private static void ComputeCyclicLayout(GraphAsset asset)
        {
            var nodeCount = asset.Nodes.Count();

            var states = asset.Nodes.FindAll(n => n.Node != null && n.Node.MaxInputConnections == -1);

            var centerPos = new Vector2Int(states.Count / 2, states.Count / 2);

            Dictionary<NodeAsset, HashSet<NodeAsset>> nearNodeMap = states.ToDictionary(n => n, n => GetNearNodes(n).ToHashSet());

            var orderedStates = states.OrderByDescending(s => nearNodeMap[s].Count);
            Dictionary<NodeAsset, Vector2Int> statePositionMap = new Dictionary<NodeAsset, Vector2Int>();

            HashSet<Vector2Int> validPositions = new HashSet<Vector2Int>();
            HashSet<Vector2Int> occupedPositions = new HashSet<Vector2Int>();

            validPositions.Add(new Vector2Int(0, 0));

            var mostConnectedState = orderedStates.First();
            Queue<NodeAsset> queue = new Queue<NodeAsset>();
            HashSet<NodeAsset> visitedNodes = new HashSet<NodeAsset>();

            queue.Enqueue(mostConnectedState);
            while(queue.Count > 0)
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
                currentState.Position = nodeOffset * bestPos * 2f;
                AddValidPositions(validPositions, occupedPositions, bestPos);
            }

            //Compute transition positions
            foreach (var tr in asset.Nodes.Except(states))
            {
                if (tr.Parents.Count == 0) return;
                if (tr.Childs.Count == 0)
                {
                    tr.Position = tr.Parents.First().Position + nodeOffset * Vector2.up;
                    //Debug.Log($"Setting position of {tr.Name} above {tr.Parents.First().Name}");
                }
                else if (tr.Childs.Count == 1)
                {
                    tr.Position = Vector2.Lerp(tr.Parents.First().Position, tr.Childs.First().Position, .5f);
                    //Debug.Log($"Setting position of {tr.Name} between its states");
                }
            }
        }

        static void AddValidPositions(HashSet<Vector2Int> validPositions, HashSet<Vector2Int> occupedPositions, Vector2Int lastOccupedPos)
        {
            occupedPositions.Add(lastOccupedPos);
            validPositions.Remove(lastOccupedPos);
            int[] hDirs = { 1, 1, 0, -1, -1, -1, 0, 1};
            int[] vDirs = { 0, 1, 1, 1, 0, -1, -1, -1};

            for(int i = 0; i < 8; i++)
            {
                var pos = lastOccupedPos + new Vector2Int(hDirs[i], vDirs[i]);
                if (!occupedPositions.Contains(pos))
                {
                    validPositions.Add(pos);
                }
            }
        }

        static Vector2Int ComputeBetterPosition(NodeAsset state, List<Vector2Int> nearStatePos, HashSet<Vector2Int> validPositions)
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
                if(dist < currentMinDist)
                {
                    //Debug.Log("Better pos");
                    bestPos = validPos;
                    currentMinDist = dist;
                }
            }
            return bestPos;
        }        

        static IEnumerable<NodeAsset> GetNearNodes(NodeAsset nodeAsset)
        {
            return nodeAsset.Childs.SelectMany(n => n.Childs)
                .Union(nodeAsset.Parents.SelectMany(n => n.Parents))
                .Distinct();
        }

        static float ProcessTreeNode(NodeAsset node, Dictionary<int, float> levelMap, int currentLevel, float targetPos)
        {
            float x;
            if (node.Childs.Count == 0)
            {                
                if (levelMap.TryGetValue(currentLevel, out float currentLevelValue))
                {
                    x = Mathf.Max(currentLevelValue + 1, targetPos);
                    levelMap[currentLevel] = x;
                }
                else
                {
                    x = targetPos;
                    levelMap[currentLevel] = targetPos;
                }
            }
            else
            {
                if (levelMap.TryGetValue(currentLevel, out float currentLevelValue))
                {
                    targetPos = MathF.Max(targetPos, currentLevelValue + 1);
                }

                float firstX = 0f, lastX = 0f;
                for (int i = 0; i < node.Childs.Count; i++)
                {
                    var child = node.Childs[i];
                    var childOffset = i - (node.Childs.Count - 1f) / 2f;
                    var childTargetPos = childOffset + targetPos;
                    var realChildPos = ProcessTreeNode(child, levelMap, currentLevel + 1, childTargetPos);
                    targetPos = Mathf.Max(realChildPos - childOffset, targetPos);
                    if (i == 0) firstX = realChildPos;
                    if (i == node.Childs.Count - 1) lastX = realChildPos;
                }

                x = (firstX + lastX) / 2f;
                levelMap[currentLevel] = x;
            }

            node.Position = new Vector2(x, currentLevel) * nodeOffset;
            levelMap[currentLevel] = x;
            return x;
        }

        public static void ComputeUSLayout(GraphAsset graphAsset)
        {
            Dictionary<NodeAsset, int> nodeLevelMap = new Dictionary<NodeAsset, int>();

            foreach(var node in graphAsset.Nodes)
            {
                if(!nodeLevelMap.ContainsKey(node)) CheckLevel(node, nodeLevelMap);
            }

            var list = nodeLevelMap.ToList();

            var maxLevel = list.Max(kvp => kvp.Value);

            for(int i = 0; i <= maxLevel; i++)
            {
                List<NodeAsset> nodes = list.FindAll(kvp => kvp.Value == i).Select(kvp => kvp.Key).ToList();
                var dist = i;
                ComputePositions(nodes, dist);
            }
        }

        static int CheckLevel(NodeAsset asset, Dictionary<NodeAsset, int> nodeLevelMap)
        {
            int currentLevel = 0;
            foreach(var child in asset.Childs)
            {
                var childValue = nodeLevelMap.TryGetValue(child, out int level) ? level : CheckLevel(child, nodeLevelMap);
                if(childValue + 1 > currentLevel) currentLevel = childValue + 1;
            }
            nodeLevelMap[asset] = currentLevel;
            return currentLevel;
        }

        static void ComputePositions(List<NodeAsset> nodes, int level)
        {
            Dictionary<NodeAsset, float> targetPositionMap = new Dictionary<NodeAsset, float>();

            for (int i = 0; i < nodes.Count; i++)
            {
                if (level == 0)
                {
                    nodes[i].Position = new Vector2(level, i) * nodeOffset;
                }
                else
                {
                    nodes[i].Position = new Vector2(level, nodes[i].Childs.Average(child => child.Position.y));
                }
            }

            if (level != 0)
            {
                var midPos = nodes.Average(n => n.Position.y);
                if (nodes.Count == 1)
                {
                    var dist = level * nodeOffset.x;
                    nodes[0].Position = new Vector2(dist, midPos);
                }
                else
                {
                    nodes = nodes.OrderBy(n => n.Position.y).ToList();
                    var dist = level * nodeOffset.x;
                    var midCount = (nodes.Count - 1) / 2f;

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        nodes[i].Position = new Vector2(dist, midPos) + nodeOffset * new Vector2(0, i - midCount);
                    }
                }

            }
        }
    }
}