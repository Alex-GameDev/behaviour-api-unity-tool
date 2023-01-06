using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviourAPI.Unity.Editor.Assets.BehaviourAPI_Unity_Tool.Editor.Scripts.Utils
{
    public static class Extensions
    {
        public static HashSet<NodeAsset> GetPathFromRoot(this NodeAsset asset)
        {
            HashSet<NodeAsset> visitedNodes = new HashSet<NodeAsset>();
            HashSet<NodeAsset> unvisitedNodes = new HashSet<NodeAsset>();

            unvisitedNodes.Add(asset);
            while (unvisitedNodes.Count > 0)
            {
                var node = unvisitedNodes.First();
                unvisitedNodes.Remove(node);
                visitedNodes.Add(node);
                node.Parents.ForEach(c =>
                {
                    if (!visitedNodes.Contains(c))
                        unvisitedNodes.Add(c);
                });
            }
            return visitedNodes;
        }

        /// <summary>
        /// Returns all the child nodes below <paramref name="asset"/>, including it.
        /// </summary>
        /// <param name="asset">The node checked</param>
        /// <returns></returns>
        public static HashSet<NodeAsset> GetPathToLeaves(this NodeAsset asset)
        {
            HashSet<NodeAsset> visitedNodes = new HashSet<NodeAsset>();
            HashSet<NodeAsset> unvisitedNodes = new HashSet<NodeAsset>();

            unvisitedNodes.Add(asset);
            while(unvisitedNodes.Count > 0)
            {
                var node = unvisitedNodes.First();
                unvisitedNodes.Remove(node);
                visitedNodes.Add(node);
                node.Childs.ForEach(c =>
                {
                    if (!visitedNodes.Contains(c))
                        unvisitedNodes.Add(c);
                });
            }
            return visitedNodes;           
        }
    }
}
