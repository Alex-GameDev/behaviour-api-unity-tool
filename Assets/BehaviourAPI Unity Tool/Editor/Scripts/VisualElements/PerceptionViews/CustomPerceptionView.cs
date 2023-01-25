using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class CustomPerceptionView : PerceptionView<CustomPerception>
    {
        public CustomPerceptionView(CustomPerception perception) :
            base(perception, BehaviourAPISettings.instance.CustomTaskLayout)
        {
        }

        protected override void AddLayout()
        {
            this.Q<Label>("cac-label").text = "Custom perception";
        }
    }
}
