using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class ConnectionAsset : ScriptableObject
    {
        [SerializeReference] Connection connection;

        public NodeAsset Source, target;

        public Connection Connection { get; set; }
    }


}