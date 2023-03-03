using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviourAPI.Core.Perceptions
{
    /// <summary>
    /// Perception that is compound itself by multiple perceptions.
    /// </summary>
    public abstract class CompoundPerception : Perception
    {
        public List<Perception> Perceptions;

        public CompoundPerception()
        {
            Perceptions = new List<Perception>();
        }

        public CompoundPerception(List<Perception> perceptions)
        {
            Perceptions = perceptions;
        }

        public CompoundPerception(params Perception[] perceptions)
        {
            Perceptions = perceptions.ToList();
        }

        public override void Initialize()
        {
            Perceptions.ForEach(p => p.Initialize());
        }

        public override void Reset()
        {
            Perceptions.ForEach(p => p.Reset());
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            Perceptions.ForEach(p => p.SetExecutionContext(context));
        }

        public override object Clone()
        {
            CompoundPerception perception = (CompoundPerception)base.Clone();
            perception.Perceptions = Perceptions.Select(p => (Perception)p.Clone()).ToList();
            return perception;
        }
    }
}
