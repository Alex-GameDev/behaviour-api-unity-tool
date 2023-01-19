using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using System.Linq;
using System.Reflection;
using System;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Convert graph assets to code and code to graph assets
    /// </summary>
    public abstract class GraphConverter
    {
        public abstract GraphAsset ConvertCodeToAsset(BehaviourGraph graph);
        public abstract void ConvertAssetToCode(GraphAsset asset, ScriptTemplate scriptTemplate);

        public static GraphConverter FindRenderer(BehaviourGraph graph)
        {
            var types = BehaviourAPISettings.instance.GetTypes().FindAll(t =>
                t.IsSubclassOf(typeof(GraphConverter)) &&
                t.GetCustomAttributes().Any(a => a is CustomConverterAttribute ccAttrib && ccAttrib.type == graph.GetType()));

            if (types.Count() > 0) return Activator.CreateInstance(types[0]) as GraphConverter;
            else return null;
        }
    }
}
