using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    public interface IBuildable
    {
        public void Build(SystemData data);
    }
}
