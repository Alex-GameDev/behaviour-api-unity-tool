using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace behaviourAPI.Unity.Framework.Adaptations
{
    public interface IPerceptionAssignable
    {
        public PerceptionAsset PerceptionReference { get; set; }
    }
}
