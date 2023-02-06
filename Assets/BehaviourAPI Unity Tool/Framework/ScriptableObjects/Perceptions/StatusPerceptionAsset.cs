using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    public class StatusPerceptionAsset : PerceptionAsset, ISerializationCallbackReceiver
    {
        [HideInInspector] public NodeAsset target;

        public void OnAfterDeserialize()
        {
            return;
        }

        public void OnBeforeSerialize()
        {
            if(perception is ExecutionStatusPerception statusPerception)
                statusPerception.StatusHandler = target.Node as IStatusHandler;
        }
    }
}