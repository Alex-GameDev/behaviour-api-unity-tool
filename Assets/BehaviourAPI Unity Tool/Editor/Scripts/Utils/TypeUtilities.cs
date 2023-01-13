using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public static class TypeUtilities
    {
        public static List<Type> GetAllTypes()
        {
            return BehaviourAPISettings.instance.GetAssemblies().SelectMany(a => a.GetTypes()).ToList();
        }

        public static List<Type> GetSubClasses(this Type type, bool includeSelf = false)
        {
            return GetAllTypes().FindAll(t => t.IsSubclassOf(type) || (includeSelf && t == type));
        }
    }
}