using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.Core.Perceptions
{
    /// <summary>
    /// A perception that is not tied to an IPerceptionHandler, but is triggered externally.
    /// </summary>
    public class PushPerception : ICloneable
    {
        public List<IPushActivable> PushListeners;

        public PushPerception(params IPushActivable[] listeners)
        {
            PushListeners = listeners.ToList();
        }
        public PushPerception(List<IPushActivable> listeners)
        {
            PushListeners = listeners.ToList();
        }

        public object Clone()
        {
            PushPerception perception = (PushPerception)MemberwiseClone();
            perception.PushListeners = new List<IPushActivable>();
            return perception;
        }

        public void Fire() => PushListeners.ForEach(p => p?.Fire());


    }
}
