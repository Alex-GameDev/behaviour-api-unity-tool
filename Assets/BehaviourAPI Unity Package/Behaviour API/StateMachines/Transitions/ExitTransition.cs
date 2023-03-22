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

        public override bool Perform()
        {
            bool canBePerformed = base.Perform();
            if (canBePerformed) _fsm.Finish(ExitStatus);
            return canBePerformed;
        }

        #endregion
    }
}
