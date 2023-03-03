using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.UtilitySystems
{
    public class MinFusionFactor : FusionFactor
    {
        protected override float Evaluate(List<float> utilities)
        {
            return utilities.Min();
        }
    }
}
