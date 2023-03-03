using System;

namespace BehaviourAPI.UtilitySystems
{
    public class ExponentialCurveFactor : CurveFactor
    {
        public float Exponent = 1f, DespX = 0f, DespY = 0f;

        public ExponentialCurveFactor SetExponent(float exp)
        {
            Exponent = exp;
            return this;
        }

        public ExponentialCurveFactor SetDespX(float despX)
        {
            DespX = despX;
            return this;
        }

        public ExponentialCurveFactor SetDespY(float despY)
        {
            DespY = despY;
            return this;
        }

        protected override float Evaluate(float x) => (float)Math.Pow(x - DespX, Exponent) + DespY;
    }
}
