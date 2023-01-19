using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomConverterAttribute : Attribute
    {
        public Type type;

        public CustomConverterAttribute(Type type)
        {
            this.type = type;
        }
    }
}
