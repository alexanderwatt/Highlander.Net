/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Orion.Analytics.Pedersen
{
    public class PayoffParser
    {
        public Node MasterNode { get; set; }

        public NodeOperation[] NodeOps { get; set; }

        public PayoffParser(Economy e, string s)
        {
            var cc = new CounterCollection();
            var eco = e;
            NodeOps = new NodeOperation[] { new Add(), new Subtract(), new Multiply(), new Divide(), new Power(), 
                new Sqrt(), new Exp(), new Log(), new Ln(), new Abs(), new Max(), new Max(cc), new Min(), new Min(cc), 
                new Int(), new Pos(), new Rate(eco), new ATM(eco), new SwapRate(eco), new Discount(eco), new PV(eco), 
                new Sum(), new Sum(cc), new Counter(cc)};
            // Warning: The order of the first 5 cannot be changed!
            MasterNode = new Node(s, NodeOps);
        }
        public double Evaluate()
        {
            return MasterNode.Evaluate();
        }
    }
}
