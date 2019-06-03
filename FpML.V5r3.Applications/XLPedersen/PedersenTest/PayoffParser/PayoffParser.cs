namespace PedersenHost.PayoffParser
{
    class PayoffParser
    {
        readonly Economy _eco;
        readonly CounterCollection _cc;
        public Node MasterNode { get; set; }

        public NodeOperation[] NodeOps { get; set; }


        public PayoffParser(Economy e, string s)
        {
            _cc = new CounterCollection();
            _eco = e;
            NodeOps = new NodeOperation[] { new Add(), new Subtract(), new Multiply(), new Divide(), new Power(), 
                new Sqrt(), new Exp(), new Log(), new Ln(), new Abs(), new Max(), new Max(_cc), new Min(), new Min(_cc), 
                new Int(), new Pos(), new Rate(_eco), new ATM(_eco), new SwapRate(_eco), new Discount(_eco), new PV(_eco), 
                new Sum(), new Sum(_cc), new Counter(_cc)};
            // Warning: The order of the first 5 cannot be changed!
            MasterNode = new Node(s, NodeOps);
        }
        public double Evaluate()
        {
            return MasterNode.Evaluate();
        }
    }


}
