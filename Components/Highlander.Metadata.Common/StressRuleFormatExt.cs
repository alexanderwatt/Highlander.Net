/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Highlander.Metadata.Common
{
    /// <summary>
    /// 
    /// </summary>
    public partial class StressRule : IComparable<StressRule>
    {
        public int CompareTo(StressRule rule)
        {
            return rule.Priority - Priority; // descending (highest to lowest)
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ScenarioRule : IComparable<ScenarioRule>
    {
        public int CompareTo(ScenarioRule rule)
        {
            return rule.Priority - Priority; // descending (highest to lowest)
        }
    }
}
