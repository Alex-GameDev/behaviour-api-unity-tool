using BehaviourAPI.Core;
using BehaviourAPI.Core.Exceptions;
using BehaviourAPI.Core.Perceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviourAPI.StateMachines
{
    public class ExitTransition : Transition
    {
        #region ------------------------------------------ Properties -----------------------------------------
        public override int MaxOutputConnections => 0;

        public Status ExitStatus;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Perform()
        {
            base.Perform();
            _fsm.Finish(ExitStatus);
        }

        #endregion
    }
}
