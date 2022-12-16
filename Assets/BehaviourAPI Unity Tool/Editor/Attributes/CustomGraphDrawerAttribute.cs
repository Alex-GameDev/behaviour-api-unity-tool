using BehaviourAPI.Core;
using System;

namespace BehaviourAPI.Unity.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomGraphDrawerAttribute : Attribute
    {
        public Type GraphType;
        public CustomGraphDrawerAttribute(Type graphType)
        {
            if(graphType.IsSubclassOf(typeof(BehaviourGraph)))
            {
                GraphType = graphType;
            }
            else
            {
                throw new ArgumentException("Error: type argument must be a subclass of BehaviourGraph");
            }
        }
    }
}

