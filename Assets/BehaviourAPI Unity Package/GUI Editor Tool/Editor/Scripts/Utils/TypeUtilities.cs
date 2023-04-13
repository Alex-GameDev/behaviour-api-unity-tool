using System.Collections.Generic;

namespace BehaviourAPI.Unity.Editor
{
    public static class TypeUtilities
    {
        public static string GetTypeName(this System.Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var genericType = type.GetGenericArguments()[0];
                return "List<" + genericType.Name + ">";
            }
            else
            {
                return type.Name;
            }
        }
    }
}