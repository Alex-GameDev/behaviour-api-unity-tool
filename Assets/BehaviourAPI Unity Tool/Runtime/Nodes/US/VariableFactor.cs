using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class VariableFactor : UtilitySystems.VariableFactor
    {
        [SerializeField] Component component;
        [SerializeField] string methodName;
    }
}
