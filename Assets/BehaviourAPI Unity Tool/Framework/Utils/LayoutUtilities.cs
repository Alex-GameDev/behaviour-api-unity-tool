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
        public static readonly Vector2 nodeOffset = new Vector2(250, 150);

        public static void ComputeLayout(GraphAsset graphAsset)
        {
            if(graphAsset == null) return;

            if(graphAsset.Graph is BehaviourTree)
            {
                var root = graphAsset.Nodes.FirstOrDefault();
                if (root == null) return;
                ProcessTreeNode(root, new Dictionary<int, float>(), 0, 0f);
            }
            else if(graphAsset.Graph is UtilitySystem)
            {
                ComputeUSLayout(graphAsset);
            }
            else if(graphAsset.Graph is FSM)
            {

            }
        }

        private static float ProcessTreeNode(NodeAsset node, Dictionary<int, float> levelMap, int currentLevel, float targetPos)
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