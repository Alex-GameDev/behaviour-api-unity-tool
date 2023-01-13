using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class StatusPerception : Perception
    {
        [SerializeField] NodeAsset target;
        [SerializeField] Status status;

        IStatusHandler statusHandler;
        public override bool Check()
        {
            return statusHandler.Status == status;
        }
    }
}
