namespace BehaviourAPI.SmartObjects
{
    /// <summary>
    /// Class that stores the information sent to smart object to request an interaction.
    /// </summary>
    [System.Serializable]
    public class RequestData
    {
        /// <summary>
        /// The name of the interaction requested.
        /// </summary>
        public string InteractionName;

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

        public static implicit operator RequestData(string interactionName) => new RequestData(interactionName);
    }
}