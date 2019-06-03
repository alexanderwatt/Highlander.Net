namespace PedersenHost
{
    class Recycle
    {
        #region declarations

        public double[][] Ivol { get; private set; }

        public double[][] IvolSq { get; private set; }

        public double[][][] IvolSqLarge { get; private set; }

        private readonly Parameters _param;

        #endregion

        public Recycle(Parameters p)
        {
            _param = p;
        }

        public void Initialise()
        {
            Ivol = new double[_param.Uexpiry][];
            IvolSq = new double[_param.Uexpiry][];
            IvolSqLarge = new double[_param.Uexpiry][][];

            for (int i = 0; i < _param.Uexpiry; i++)
            {
                Ivol[i] = new double[_param.Utenor - i];
                IvolSq[i] = new double[_param.Utenor - i];
                IvolSqLarge[i] = new double[_param.Utenor - i][];
                for (int j = 0; j < _param.Utenor - i; j++)
                {
                    IvolSqLarge[i][j] = new double[i + 1];
                }
            }
        }

    }
}
