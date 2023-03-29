using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    using Framework;

    /// <summary>
    /// Subclass of  <see cref="BehaviourRunner"/> used to edit a behaviour runner with the editor tools.
    /// </summary>
    public abstract class EditorBehaviourRunner : DataBehaviourRunner, IBehaviourSystem
    {
        #region ------------------------------- Private fields --------------------------------

        [SerializeField] SystemData data = new SystemData();

        #endregion

        #region --------------------------------- Properties ----------------------------------

        public SystemData Data => data;

        public Object ObjectReference => this;

        #endregion

        #region --------------------------------- properties ----------------------------------

        protected sealed override SystemData GetEditorSystemData() => data;

        #endregion
    }
}
