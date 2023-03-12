using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
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

        protected override SystemData GetEditorSystemData() => data;

        #endregion
    }
}
