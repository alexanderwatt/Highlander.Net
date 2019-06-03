using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Configuration
{
    public partial class StressRule : IComparable<StressRule>
    {
        public int CompareTo(StressRule rule)
        {
            return (rule.Priority - this.Priority); // descending (highest to lowest)
        }
    }
    public partial class ScenarioRule : IComparable<ScenarioRule>
    {
        public int CompareTo(ScenarioRule rule)
        {
            return (rule.Priority - this.Priority); // descending (highest to lowest)
        }
    }
}
