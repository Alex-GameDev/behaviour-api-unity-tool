using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class ExecutionStatusPerceptionAsset : PerceptionAsset
    {
        [SerializeField] NodeAsset target;
        [SerializeField] Status status;

        public static ExecutionStatusPerceptionAsset Create(string name)
        {
            var executionStatusPerceptionAsset = CreateInstance<ExecutionStatusPerceptionAsset>();
            executionStatusPerceptionAsset.Name = name;
            return executionStatusPerceptionAsset;
        }
    }
}