using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class NodeAsset : ScriptableObject
    {
        [SerializeReference] Node node;
        List<ConnectionAsset> InputConnections;
        List<ConnectionAsset> OutputConnections;


        /// <summary>
        /// Node internal connection references didn't serialize, so they need to be created runtime.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void AssembleConnections()
        {
            OutputConnections.ForEach(asset => node.OutputConnections.Add(asset.Connection));
            InputConnections.ForEach(asset => node.InputConnections.Add(asset.Connection));
        }
    }

}