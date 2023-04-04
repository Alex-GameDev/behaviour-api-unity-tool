namespace BehaviourAPI.Core
{
    /// <summary>
    /// Defines a point in a 2D space.
    /// </summary>
    [System.Serializable]
    public struct Vector2
    {
        /// <summary>
        /// The horizontal coordinate.
        /// </summary>
        public float x;

        /// <summary>
        /// The vertical coordinate.
        /// </summary>
        public float y;

        /// <summary>
        /// Creates a new Vector2 struct with the given coordinates.
        /// </summary>
        /// <param name="x">The horizontal coordinate.</param>
        /// <param name="y">The vertical coordinate.</param>
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Add two vectors.
        /// </summary>
        /// <param name="a">The first addend.</param>
        /// <param name="b">The second addend.</param>
        /// <returns>The component sum of the vectors.</returns>
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        /// <summary>
        /// Substract two vectors.
        /// </summary>
        /// <param name="a">The first element.</param>
        /// <param name="b">The second element.</param>
        /// <returns>The component substraction of the vectors.</returns>
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }
    }
}
