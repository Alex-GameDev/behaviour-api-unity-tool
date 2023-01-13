using BehaviourAPI.Core.Perceptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class CompoundPerception : Perception
    {
        [SerializeReference] Perception perceptionA;
        [SerializeReference] Perception perceptionB;
        [SerializeField] LogicOperation logicOperation;
        public override bool Check()
        {
            bool result, target;
            if (logicOperation == LogicOperation.AND)
            {
                result = false;
                target = true;
            }
            else if(logicOperation == LogicOperation.OR)
            {
                result = false;
                target = true;
            }
            else
                result = false;

            return result;
        }
    }

    public enum LogicOperation
    {
        AND,
        OR
    }
}
