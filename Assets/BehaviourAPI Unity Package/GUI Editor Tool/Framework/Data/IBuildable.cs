namespace BehaviourAPI.Unity.Framework
{
    /// <summary>
    /// Indicates that an element of a behavior system has to build internal references before it is used.
    /// </summary>
    public interface IBuildable
    {
        /// <summary>
        /// Set the element references using <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The behaviour system data</param>
        public void Build(BuildData data);
    }
}
