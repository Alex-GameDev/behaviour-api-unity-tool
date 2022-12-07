using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    [CreateAssetMenu(fileName = "Exit action")]
    public class ExitActionAsset : ActionAsset
    {
        public Status ExitStatus;
    }
}
