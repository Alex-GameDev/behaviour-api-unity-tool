using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        public static CustomGraphDrawer FindCustomGraphDrawer(Type graphType, IEnumerable<Assembly> assemblies = null)
        {
            if (assemblies == null || assemblies.Count() == 0)
            {
                assemblies = VisualSettings.GetOrCreateSettings().assemblies.Select(a => Assembly.Load(a));
            }

            var c = GetTypesDerivedFrom(typeof(CustomGraphDrawer), assemblies).Find(type =>
            {
                var attribute = type.GetCustomAttribute<CustomGraphDrawerAttribute>();
                return attribute?.GraphType == graphType;
            });

            if (c != null)
                return (CustomGraphDrawer)Activator.CreateInstance(c);
            else
                return new DefaultGraphDrawer();
        }
    }
}