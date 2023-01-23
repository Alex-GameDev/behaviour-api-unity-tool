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
using System.Collections.Generic;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Convert graph assets to code and code to graph assets
    /// </summary>
    public abstract class GraphConverter
    {
        protected string graphName;
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

        protected string GetActionCode(Action action, ScriptTemplate scriptTemplate)
        {
            if (action is CustomAction customAction)
            {
                var parameters = new List<string>();
                if (customAction.start != null)
                {
                    var componentName = scriptTemplate.AddPropertyLine(customAction.start.component.TypeName(), customAction.start.component.TypeName().ToLower(), customAction.start.component);
                    parameters.Add($"{componentName}.{customAction.start.methodName}");
                }

                if (customAction.update != null)
                {
                    var componentName = scriptTemplate.AddPropertyLine(customAction.update.component.TypeName(), customAction.update.component.TypeName().ToLower(), customAction.update.component);
                    parameters.Add($"{componentName}.{customAction.update.methodName}");
                }
                else
                {
                    parameters.Add("() => Status.Running");                }

                if (customAction.stop != null)
                {
                    var componentName = scriptTemplate.AddPropertyLine(customAction.stop.component.TypeName(), customAction.stop.component.TypeName().ToLower(), customAction.stop.component);
                    parameters.Add($"{componentName}.{customAction.stop.methodName}");
                }

                return $"new {nameof(FunctionalAction)}({string.Join(", ", parameters)})";

            }
            else if (action is UnityAction unityAction)
            {
                // Add arguments
                return $"new {unityAction.TypeName()}( )";
            }
            else if (action is SubgraphAction subgraphAction)
            {
                var graphName = scriptTemplate.FindVariableName(subgraphAction.Subgraph);
                return $"new {nameof(SubsystemAction)}({graphName ?? "null /*ERROR*/"})";
            }
            else 
                return null;
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
