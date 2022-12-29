using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class StatusPerceptionView : PerceptionView<StatusPerception>
    {
        public StatusPerceptionView(StatusPerception perception) : base(perception, BehaviourAPISettings.instance.StatusPerceptionLayout)
        {
        }

        protected override void AddLayout()
        {

        }
    }
}
