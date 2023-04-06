using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.UtilitySystems
{
    using Core;

    /// <summary>
    /// Create a curve factor with an with a linear function defined with points.
    /// </summary>
    public class PointedCurveFactor : CurveFactor
    {
        /// <summary>
        /// The points used to define the function. Must be ordered in its x coord to avoid errors.
        /// </summary>
        public List<Vector2> Points = new List<Vector2>();

        /// <summary>
        /// Set the points of the pointed curve factor.
        /// </summary>
        /// <param name="points">The new exponent.</param>
        /// <returns>The <see cref="PointedCurveFactor"/> itself. </returns>
        public PointedCurveFactor SetPoints(List<Vector2> points)
        {
            Points = points;
            return this;
        }

        /// <summary>
        /// Set the points of the pointed curve factor.
        /// </summary>
        /// <param name="points">The new exponent.</param>
        /// <returns>The <see cref="PointedCurveFactor"/> itself. </returns>
        public PointedCurveFactor SetPoints(params Vector2[] points)
        {
            Points = points.ToList();
            return this;
        }


        /// <summary>
        /// Compute the utility using a linear function defined with points.
        /// <para>If x is lower than the first point x coord, the value will be its y coord.</para>
        /// <para>If x is higher than the last point x coord, the value will be its y coord.</para>
        /// </summary>
        /// <param name="x">The child utility. </param>
        /// <returns>The result of apply the function to <paramref name="x"/>.</returns>
        protected override float Evaluate(float x)
        {
            if(Points.Count == 0) return 0;

            int id = FindClosestLowerId(x);

            if (id == -1)
            {
                return Points[0].y;
            }

            else if(id == Points.Count - 1) 
                return Points[Points.Count - 1].y;
            else
            {
                var delta = (x - Points[id].x) / (Points[id + 1].x - Points[id].x);
                return Points[id].y * (1 - delta) + Points[id + 1].y * delta;
            }
        }

        int FindClosestLowerId(float x)
        {
            int id = 0;
            while (id < Points.Count && Points[id].x <= x)
            {
                id++;
            }
            return id - 1;          
        }

        public override object Clone()
        {
            PointedCurveFactor function = (PointedCurveFactor)base.Clone();
            function.Points = Points.ToList();
            return function;
        }
    }
}
