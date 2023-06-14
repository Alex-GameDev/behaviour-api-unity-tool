namespace BehaviourAPI.UnityToolkit.GUIDesigner.Framework
{
    public class SimpleAction : Core.Actions.SimpleAction, IBuildable
    {
        public ContextualSerializedAction method;

        public void Build(BuildData data)
        {
            action = method.CreateDelegate(data.Runner);
        }

        public override object Clone()
        {
            var copy = (SimpleAction)base.Clone();
            copy.method = (ContextualSerializedAction)method?.Clone();
            return copy;
        }

        public override string ToString()
        {
            return "SimpleAction(" + method.ToString() + ")";
        }
    }
}
