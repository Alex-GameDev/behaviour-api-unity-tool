using UnityEngine;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Framework
{
    using BehaviourAPI.Core;
    public class SimpleAction : Core.Actions.SimpleAction
    {
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
