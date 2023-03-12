using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    public interface IBehaviourSystem
    {
        public SystemData Data { get; }
        public Object ObjectReference { get; }       
    }
}