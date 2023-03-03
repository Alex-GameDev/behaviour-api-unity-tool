using System;

namespace BehaviourAPI.UtilitySystems
{
    public class SigmoidCurveFactor : CurveFactor
    {
        public float GrownRate = 1f, Midpoint = 0.5f;

        public SigmoidCurveFactor SetGrownRate(float grownRate)
        {
            GrownRate = grownRate;
            return this;
        }

        public SigmoidCurveFactor SetMidpoint(float midpoint)
        {

            Midpoint = midpoint;
            return this;
        }

        protected override float Evaluate(float x) => (float)(1f / (1f + Math.Pow(Math.E, -GrownRate * (x - Midpoint))));
    }
}
