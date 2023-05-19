using System.Collections.Generic;

namespace BehaviourAPI.SmartObjects
{
    using Core;

    /// <summary> 
    /// An object that can provide behaviour to a smart agent and cover some of its needs.
    /// </summary>
    public interface ISmartObject<T> where T : ISmartAgent
    {
        /// <summary>   
        /// Request the interaction. 
        /// </summary>
        /// <param name="agent"> The agent who request the interaction. </param>
        /// <returns> </returns>
        SmartInteraction<T> RequestInteraction(T agent, string requestData);

        /// <summary>
        /// Validates the agent described by agent. 
        /// </summary>
        /// <param name="agent"> The agent who request the interaction. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool ValidateAgent(T agent);

        /// <summary> 
        /// Gets a capability value. 
        /// </summary>
        /// <param name="capabilityName"> Name of the capability. </param>
        /// <returns>  The capability. </returns>
        float GetCapabilityValue(string capabilityName);

        /// <summary>
        /// Called when the agent gets the interaction.
        /// </summary>
        /// <param name="agent">The agent that requested the interaction.</param>
        void OnInitInteraction(T agent);

        /// <summary>
        /// Called when the interaction provided by the object ends with success or failure. 
        /// </summary>
        /// <param name="agent"> The agent who request the interaction. </param>
        void OnCompleteInteraction(T agent, Status status);

        /// <summary>
        /// Called when the agent releases the interaction, even if the action was not completed.
        /// </summary>
        /// <param name="agent">The agent that requested the interaction.</param>
        void OnReleaseInteraction(T agent);

        /// <summary>
        /// Called when the agent pauses the interaction.
        /// </summary>
        /// <param name="agent">The agent that requested the interaction.</param>
        void OnPauseInteraction(T agent);

        /// <summary>
        /// Called when the agent unpauses the interaction.
        /// </summary>
        /// <param name="agent">The agent that requested the interaction.</param>
        void OnUnpauseInteraction(T agent);

        /// <summary> 
        /// Gets all the capabilities that this smart object has. 
        /// </summary>
        /// <returns> An enumerator that allows foreach to be used to process the capabilities.
        /// </returns>
        IEnumerable<string> GetCapabilities();
    }
}
