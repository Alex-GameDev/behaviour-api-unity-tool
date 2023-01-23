using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using System.Linq;
using System.Reflection;
using System;
using BehaviourAPI.Core.Actions;
using Action = BehaviourAPI.Core.Actions.Action;
using BehaviourAPI.Core.Perceptions;
using UnityEngine.Events;
using UnityAction = BehaviourAPI.Unity.Runtime.UnityAction;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Convert graph assets to code and code to graph assets
    /// </summary>
    public abstract class GraphConverter
    {
        public abstract GraphAsset ConvertCodeToAsset(BehaviourGraph graph);
        public abstract void ConvertAssetToCode(GraphAsset asset, ScriptTemplate scriptTemplate);

        public static GraphConverter FindConverter(BehaviourGraph graph)
        {
            var types = BehaviourAPISettings.instance.GetTypes().FindAll(t =>
                t.IsSubclassOf(typeof(GraphConverter)) &&
                t.GetCustomAttributes().Any(a => a is CustomConverterAttribute ccAttrib && ccAttrib.type == graph.GetType()));

            if (types.Count() > 0) return Activator.CreateInstance(types[0]) as GraphConverter;
            else return null;
        }

        public virtual string AddCreateGraphLine(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            return scriptTemplate.AddVariableInstantiationLine(asset.Graph.TypeName(), asset.Name, asset);
        }

        protected void AddAction(Action action, ScriptTemplate scriptTemplate)
        {
            //if (action is CustomAction customAction)
            //{
            //    scriptTemplate.OpenVariableCreation(nameof(FunctionalAction));
            //    if (customAction.start != null)
            //    {
            //        scriptTemplate.AddParameter($"{customAction.start.component.name}.{customAction.start.methodName}");
            //    }
            //    if (customAction.update != null)
            //    {
            //        scriptTemplate.AddParameter($"{customAction.update.component.name}.{customAction.update.methodName}");
            //    }
            //    if (customAction.stop != null)
            //    {
            //        scriptTemplate.AddParameter($"{customAction.stop.component.name}.{customAction.stop.methodName}");
            //    }

            //    scriptTemplate.CloseMethodOrVariableAsignation();
            //}
            //else if (action is UnityAction unityAction)
            //{
            //    scriptTemplate.OpenVariableCreation(unityAction.GetType().Name);
            //    // Add arguments
            //    scriptTemplate.CloseMethodOrVariableAsignation();
            //}
            //else if (action is SubgraphAction subgraphAction)
            //{
            //    scriptTemplate.OpenVariableCreation(nameof(SubsystemAction));
            //    scriptTemplate.AddParameter(subgraphAction.Subgraph.Name);
            //    // Add arguments
            //    scriptTemplate.CloseMethodOrVariableAsignation();
            //}
        }

        protected void AddPerception(Perception perception, ScriptTemplate scriptTemplate)
        {
        //    if (perception is CustomPerception customPerception)
        //    {
        //        scriptTemplate.OpenVariableCreation(nameof(ConditionPerception));

        //        if (customPerception.init != null)
        //        {
        //            scriptTemplate.AddParameter($"{customPerception.init.component.name}.{customPerception.init.methodName}");
        //        }
        //        if (customPerception.check != null)
        //        {
        //            scriptTemplate.AddParameter($"{customPerception.check.component.name}.{customPerception.check.methodName}");
        //        }
        //        if (customPerception.reset != null)
        //        {
        //            scriptTemplate.AddParameter($"{customPerception.reset.component.name}.{customPerception.reset.methodName}");
        //        }

        //        scriptTemplate.CloseMethodOrVariableAsignation();
        //    }
        //    else if (perception is UnityPerception unityPerception)
        //    {
        //        scriptTemplate.OpenVariableCreation(unityPerception.GetType().Name);
        //        // Add arguments
        //        scriptTemplate.CloseMethodOrVariableAsignation();
        //    }
        }
    }
}
