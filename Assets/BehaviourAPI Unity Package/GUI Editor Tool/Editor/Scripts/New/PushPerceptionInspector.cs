using BehaviourAPI.New.Unity.Editor;
using BehaviourAPI.UnityTool.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class PushPerceptionInspector : Inspector<PushPerceptionData>
    {
        public PushPerceptionInspector() : base("Push perceptions", Side.Right)
        {
        }
    }
}
