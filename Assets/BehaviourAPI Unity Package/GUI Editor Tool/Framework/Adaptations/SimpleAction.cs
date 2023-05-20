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
        public ContextualSerializedAction start;

        public override void Start() => start.GetFunction()?.Invoke();

        public override object Clone()
        {
            var copy = (SimpleAction)base.Clone();
            copy.start = (ContextualSerializedAction)start?.Clone();
            return copy;
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            var unityContext = (UnityExecutionContext)context;
            if (unityContext != null)
            {
                start.SetContext(unityContext);
            }
            else
            {
                Debug.LogError("Simple action need an UnityExecutionContext to work");
            }
        }
    }
}
