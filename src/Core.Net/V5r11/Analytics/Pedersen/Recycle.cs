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

namespace Highlander.Reporting.Analytics.V5r3.Pedersen
{
    public class Recycle
    {
        #region Declarations

        public double[][] ImpliedVolatility { get; private set; }

        public double[][] ImpliedVolatilitySquare { get; private set; }

        public double[][][] ImpliedVolatilitySquareLarge { get; private set; }

        private readonly Parameters _param;

        #endregion

        public Recycle(Parameters p)
        {
            _param = p;
        }

        public void Initialise()
        {
            ImpliedVolatility = new double[_param.UnderlyingExpiry][];
            ImpliedVolatilitySquare = new double[_param.UnderlyingExpiry][];
            ImpliedVolatilitySquareLarge = new double[_param.UnderlyingExpiry][][];
            for (int i = 0; i < _param.UnderlyingExpiry; i++)
            {
                ImpliedVolatility[i] = new double[_param.UnderlyingTenor - i];
                ImpliedVolatilitySquare[i] = new double[_param.UnderlyingTenor - i];
                ImpliedVolatilitySquareLarge[i] = new double[_param.UnderlyingTenor - i][];
                for (int j = 0; j < _param.UnderlyingTenor - i; j++)
                {
                    ImpliedVolatilitySquareLarge[i][j] = new double[i + 1];
                }
            }
        }
    }
}
