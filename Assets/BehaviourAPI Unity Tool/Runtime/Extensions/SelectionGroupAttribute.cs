using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class SelectionGroupAttribute : Attribute
    {
        public string name;

        public SelectionGroupAttribute(string name)
        {
            this.name = name;
        }
    }
}
