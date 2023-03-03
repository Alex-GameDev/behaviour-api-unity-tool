using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.UtilitySystems
{
    public class MaxFusionFactor : FusionFactor
    {
        protected override float Evaluate(List<float> utilities)
        {
            return utilities.Max();
        }
    }
}
