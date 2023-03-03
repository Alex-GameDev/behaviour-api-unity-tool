using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.UtilitySystems
{
    using Core;

    public class PointedCurveFactor : CurveFactor
    {
        public List<Vector2> Points = new List<Vector2>();

        public PointedCurveFactor SetPoints(List<Vector2> points)
        {
            Points = points;
            return this;
        }

        public PointedCurveFactor SetPoints(params Vector2[] points)
        {
            Points = points.ToList();
            return this;
        }

        protected override float Evaluate(float x)
        {
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
