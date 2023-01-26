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

            var root = graphAsset.Nodes.FirstOrDefault();

            if (root == null) return;

            ProcessTreeNode(root, new Dictionary<int, float>(), 0, 0f);
        }

        private static float ProcessTreeNode(NodeAsset node, Dictionary<int, float> levelMap, int currentLevel, float targetPos)
        {
            float x;
            if (node.Childs.Count == 0)
            {                
                if (levelMap.TryGetValue(currentLevel, out float currentLevelValue))
                {
                    x = currentLevelValue + 1;
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
                    targetPos = realChildPos - childOffset;
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
    }
}