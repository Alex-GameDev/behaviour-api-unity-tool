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
        //TODO: Combinar converter y renderer en adapter
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
                if (customAction.start != null && customAction.start.component != null && !string.IsNullOrEmpty(customAction.start.methodName))
                {
                    var componentName = scriptTemplate.AddPropertyLine(customAction.start.component.TypeName(), customAction.start.component.TypeName().ToLower(), customAction.start.component);
                    parameters.Add($"{componentName}.{customAction.start.methodName}");
                }

                if (customAction.update != null && customAction.update.component != null && !string.IsNullOrEmpty(customAction.update.methodName))
                {
                    var componentName = scriptTemplate.AddPropertyLine(customAction.update.component.TypeName(), customAction.update.component.TypeName().ToLower(), customAction.update.component);
                    parameters.Add($"{componentName}.{customAction.update.methodName}");
                }
                else
                {
                    parameters.Add("() => Status.Running");                
                }

                if (customAction.stop != null && customAction.stop.component != null && !string.IsNullOrEmpty(customAction.stop.methodName))
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

        protected string GetPerceptionCode(Perception perception, ScriptTemplate scriptTemplate)
        {
            if (perception is CustomPerception customPerception)
            {
                var parameters = new List<string>();
                if (customPerception.init != null && customPerception.init.component != null && !string.IsNullOrEmpty(customPerception.init.methodName))
                {
                    var componentName = scriptTemplate.AddPropertyLine(customPerception.init.component.TypeName(), customPerception.init.component.TypeName().ToLower(), customPerception.init.component);
                    parameters.Add($"{componentName}.{customPerception.init.methodName}");
                }

                if (customPerception.check != null && customPerception.check.component != null && !string.IsNullOrEmpty(customPerception.check.methodName))
                {
                    var componentName = scriptTemplate.AddPropertyLine(customPerception.check.component.TypeName(), customPerception.check.component.TypeName().ToLower(), customPerception.check.component);
                    parameters.Add($"{componentName}.{customPerception.check.methodName}");
                }
                else
                {
                    parameters.Add("() => false");
                }

                if (customPerception.reset != null && customPerception.reset.component != null && !string.IsNullOrEmpty(customPerception.reset.methodName))
                {
                    var componentName = scriptTemplate.AddPropertyLine(customPerception.reset.component.TypeName(), customPerception.reset.component.TypeName().ToLower(), customPerception.reset.component);
                    parameters.Add($"{componentName}.{customPerception.reset.methodName}");
                }

                return $"new {nameof(ConditionPerception)}({string.Join(", ", parameters)})";
            }
            else if (perception is UnityPerception unityPerception)
            {
                // Add arguments
                return $"new {unityPerception.TypeName()}()";
            }
            else
                return null;
        }
    }
}
