using BehaviourAPI.Core;
using System.Collections.Generic;

namespace BehaviourAPI.SmartObjects
{
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
        SmartInteraction<T> RequestInteraction(T agent);

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
        /// Invoked when the interaction provided by the object ends with failure. 
        /// Commonly used to apply the properties
        /// </summary>
        /// <param name="agent"> The agent who request the interaction. </param>
        void OnComplete(T agent, Status status);

        /// <summary> 
        /// Gets all the capabilities that this smart object has. 
        /// </summary>
        /// <returns> An enumerator that allows foreach to be used to process the capabilities.
        /// </returns>
        IEnumerable<string> GetCapabilities();

        /// <summary>
        /// Invoked when the agent ends its interaction with the smart object. 
        /// </summary>
        /// <param name="agent"> The agent who request the interaction. </param>
        void ReleaseInteraction(T agent);

        /// <summary>
        /// Invoked when the agent ends its interaction with the smart object. 
        /// </summary>
        /// <param name="agent"> The agent who request the interaction. </param>
        void InitInteraction(T agent);

    }
}
