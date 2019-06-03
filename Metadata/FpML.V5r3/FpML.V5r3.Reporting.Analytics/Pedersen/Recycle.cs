/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Orion.Analytics.Pedersen
{
    public class Recycle
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
