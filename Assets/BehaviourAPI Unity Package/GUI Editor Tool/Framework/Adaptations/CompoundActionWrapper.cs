using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using Core.Actions;

    public class CompoundActionWrapper : Action, IBuildable
    {
        [SerializeField] string a;
        [SerializeReference] public CompoundAction compoundAction;

        public List<SubActionWrapper> subActions = new List<SubActionWrapper>();

        public CompoundActionWrapper()
        {
        }

        public CompoundActionWrapper(CompoundAction compoundAction)
        {
            this.compoundAction = compoundAction;
        }

        public override void Start() => compoundAction.Start();

        public override Status Update() => compoundAction.Update();

        public override void Stop() => compoundAction.Stop();

        public override void Pause() => compoundAction.Pause();

        public override void Unpause() => compoundAction.Unpause();

        public override object Clone()
        {
            var copy = (CompoundActionWrapper)base.Clone();
            copy.compoundAction = (CompoundAction)compoundAction.Clone();
            copy.subActions = subActions.Select(p => new SubActionWrapper((Action)p.action.Clone())).ToList();
            return copy;
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            compoundAction.SetExecutionContext(context);
        }

        public void Build(SystemData data)
        {
            foreach(var subAction in subActions)
                if (subAction.action is IBuildable buildable) buildable.Build(data);

            compoundAction.SubActions = subActions.Select(p => p.action).ToList();
        }
    }
}
