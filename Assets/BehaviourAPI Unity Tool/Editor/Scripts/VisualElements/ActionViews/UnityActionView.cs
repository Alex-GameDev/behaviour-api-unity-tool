using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class UnityActionView : ActionView<UnityAction>
    {
        public UnityActionView(UnityAction unityAction) : 
            base(unityAction, BehaviourAPISettings.instance.UnityActionLayout)
        {
        }

        protected override void AddLayout()
        {
            this.Q<Label>("uac-label").text = _action.DisplayInfo;
        }
    }
}
