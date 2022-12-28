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
        public static List<Type> GetAllTypes(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(a => a.GetTypes()).ToList();
        }

        public static List<Type> GetTypesDerivedFrom(Type type, IEnumerable<Assembly> assemblies)
        {
            var types = GetAllTypes(assemblies);
            return types.FindAll(t => t.IsSubclassOf(type) || t == type);
        }

        public static List<Type> GetTypesDerivedFrom(Type type, IEnumerable<string> assemblyNames)
        {
            var assemblies = assemblyNames.ToList().Select(a => Assembly.Load(a));
            return GetTypesDerivedFrom(type, assemblies);
        }

        public static List<Type> GetAllGraphTypes()
        {
            return GetTypesDerivedFrom(typeof(BehaviourGraph), VisualSettings.GetOrCreateSettings().assemblies)
                .FindAll(t => !t.IsAbstract);
        }
    }

}