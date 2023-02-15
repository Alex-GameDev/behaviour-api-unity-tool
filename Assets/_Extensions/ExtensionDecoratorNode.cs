using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionesCustom
{
    public class ExtensionDecoratorNode : DecoratorNode
    {
        protected override Status UpdateStatus()
        {
            return Status.None;
        }
    }
}