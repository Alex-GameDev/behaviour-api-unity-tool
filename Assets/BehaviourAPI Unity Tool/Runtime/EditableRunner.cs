using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BehaviourAPI.Unity.Runtime
{
    public class EditableRunner : BehaviourRunner
    {
        [HideInInspector] public SystemAsset System;

        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            return null;
        }

        protected override void OnAwake()
        {
        }

        protected override void OnStart()
        {
        }

        protected override void OnUpdate()
        {

        }
    }
}
