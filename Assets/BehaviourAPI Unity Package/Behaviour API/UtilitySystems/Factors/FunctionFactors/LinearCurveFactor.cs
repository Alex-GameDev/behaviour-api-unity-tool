namespace BehaviourAPI.UtilitySystems
{
    public class LinearCurveFactor : CurveFactor
    {
        public float Slope = 1f, YIntercept = 0f;

        public LinearCurveFactor SetSlope(float slope)
        {
            this.Slope = slope;
            return this;
        }

        public LinearCurveFactor SetYIntercept(float yIntercept)
        {
            this.YIntercept = yIntercept;
            return this;
        }

        protected override float Evaluate(float x) => Slope * x + YIntercept;
    }
}
