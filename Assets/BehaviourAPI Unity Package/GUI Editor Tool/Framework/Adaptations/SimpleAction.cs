using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class SimpleAction : Core.Actions.SimpleAction
    {
        /// <summary>
        /// Method executed when the action started.
        /// </summary>
        public ContextualSerializedAction method;

        public override void Start() => method.GetFunction()?.Invoke();

        public override object Clone()
        {
            var copy = (SimpleAction)base.Clone();
            copy.method = (ContextualSerializedAction)method?.Clone();
            return copy;
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            var unityContext = (UnityExecutionContext)context;
            if (unityContext != null)
            {
                method.SetContext(unityContext);
            }
            else
            {
                Debug.LogError("Simple action need an UnityExecutionContext to work");
            }
        }
    }
}
