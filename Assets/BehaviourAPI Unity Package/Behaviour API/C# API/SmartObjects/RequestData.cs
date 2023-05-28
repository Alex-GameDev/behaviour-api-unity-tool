namespace BehaviourAPI.SmartObjects
{
    /// <summary>
    /// Class that stores the information sent to smart object to request an interaction.
    /// </summary>
    public class RequestData
    {
        /// <summary>
        /// The name of the interaction requested.
        /// </summary>
        public string InteractionName { get; set; }

        /// <summary>
        /// Create a new request data without specify the interaction name.
        /// </summary>
        public RequestData()
        {
        }

        /// <summary>
        /// Create a new request interaction.
        /// </summary>
        /// <param name="interactionName">The name of the interaction requested.</param>
        public RequestData(string interactionName)
        {
            InteractionName = interactionName;
        }
    }
}