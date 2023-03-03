using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.UtilitySystems
{
    public class WeightedFusionFactor : FusionFactor
    {
        public float[] Weights = new float[0];
        public WeightedFusionFactor SetWeights(params float[] weights)
        {
            Weights = weights;
            return this;
        }

        public Factor SetWeights(IEnumerable<float> weights)
        {
            Weights = weights.ToArray();
            return this;
        }

        protected override float Evaluate(List<float> utilities)
        {
            return utilities.Zip(Weights, (utility, weight) => utility * weight).Sum();
        }

        public override object Clone()
        {
            WeightedFusionFactor fusion = (WeightedFusionFactor)base.Clone();
            fusion.Weights = Weights.ToArray();
            return fusion;
        }
    }
}
