using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class DebugAction : UnityAction
    {

        public string Message;

        public override string DisplayInfo => "Debug Log $Message";

        protected override void OnUpdate()
        {
            Debug.Log(Message);
            Success();
        }
    }
}