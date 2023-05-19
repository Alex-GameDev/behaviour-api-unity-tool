namespace BehaviourAPI.SmartObjects
{
    using Core.Actions;

    /// <summary>   
    /// Represents an interaction between a smart agent and a smart object. 
    /// When the interaction is completed, a agent's need is covered.
    /// </summary>
    public class SmartInteraction<T> where T : ISmartAgent
    {
        /// <summary> 
        /// Gets the smartobject that has provided that interaction. </summary>
        /// <value> The interaction action. </value>
        public ISmartObject<T> SmartObject { get; private set; }

        /// <summary> 
        /// Gets the action that this interaction executes. </summary>
        /// <value> The interaction action. </value>
        public Action Action { get; private set; }

        /// <summary> 
        /// Constructor. 
        /// </summary>
        /// <param name="smartObject">  The interaction object. </param>
        /// <param name="action">       The interaction action. </param>
        public SmartInteraction(ISmartObject<T> smartObject, Action action)
        {
            SmartObject = smartObject;
            Action = action;
        }
    }
}
