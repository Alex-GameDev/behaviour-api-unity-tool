using System;

namespace BehaviourAPI.Unity.Editor.CodeGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomGraphCodeGeneratorAttribute : Attribute
    {
        public Type GraphType { get; private set; }

        public CustomGraphCodeGeneratorAttribute(Type graphType)
        {
            GraphType = graphType;
        }
    }
}
