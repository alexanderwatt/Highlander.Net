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

using System;
using System.Collections.Generic;
using Orion.Analytics.Utilities;

namespace Orion.Analytics.Statistics
{
    //public interface IRngTraits
    //{
    //    ulong NextInt32();
    //    Sample<double> Next();
    //    IRngTraits Factory(ulong seed);
    //}

    //public interface IRng
    //{
    //    int Dimension();
    //    Sample<List<double>> NextSequence();
    //    Sample<List<double>> LastSequence();
    //    IRng Factory(int dimensionality, ulong seed);
    //}

    ///*! Random sequence generator based on a pseudo-random number generator RNG
    //    Do not use with low-discrepancy sequence generator.
    //*/
    //public class RandomSequenceGenerator<TRng> : IRng where TRng : IRngTraits, new()
    //{
    //    private readonly int _dimensionality;
    //    private readonly TRng _rng;
    //    private readonly Sample<List<double>> _sequence;
    //    private readonly List<ulong> _int32Sequence;

    //    public RandomSequenceGenerator(int dimensionality, TRng rng)
    //    {
    //        _dimensionality = dimensionality;
    //        _rng = rng;
    //        List<double> ls = new List<double>();
    //        for (int i = 0; i < dimensionality; i++)
    //            ls.Add(0.0);
    //        _sequence = new Sample<List<double>>(ls, 1.0);
    //        _int32Sequence = new List<ulong>(dimensionality);
    //        //dimensionality must be greater than 0
    //    }

    //    public RandomSequenceGenerator(int dimensionality, ulong seed)
    //    {
    //        _dimensionality = dimensionality;
    //        _rng = (TRng) FastActivator<TRng>.Create().Factory(seed);
    //        _sequence = new Sample<List<double>>(new List<double>(dimensionality), 1.0);
    //        _int32Sequence = new List<ulong>(dimensionality);
    //    }

    //    public List<ulong> NextInt32Sequence()
    //    {
    //        for (int i = 0; i < _dimensionality; i++)
    //        {
    //            _int32Sequence[i] = _rng.NextInt32();
    //        }
    //        return _int32Sequence;
    //    }

    //    #region IRGN interface

    //    public Sample<List<double>> NextSequence()
    //    {
    //        _sequence.Weight = 1.0;
    //        for (int i = 0; i < _dimensionality; i++)
    //        {
    //            Sample<double> x = _rng.Next();
    //            _sequence.Value[i] = x.Value;
    //            _sequence.Weight *= x.Weight;
    //        }
    //        return _sequence;
    //    }

    //    public Sample<List<double>> LastSequence()
    //    {
    //        return _sequence;
    //    }

    //    public int Dimension()
    //    {
    //        return _dimensionality;
    //    }

    //    public IRng Factory(int dimensionality, ulong seed)
    //    {
    //        return new RandomSequenceGenerator<TRng>(dimensionality, seed);
    //    }
    //}

    //#endregion

    //}

    //! Random seed generator
    /*! Random number generator used for automatic generation of initialization seeds. */
    //public class SeedGenerator
    //{
    //    private MersenneTwister _rng;
    //    private static readonly SeedGenerator _instance = new SeedGenerator();

    //    private SeedGenerator()
    //    {
    //        _rng = new MersenneTwister(42UL);
    //        Initialize();
    //    }

    //    public ulong Get()
    //    {
    //        return _rng.NextInt32();
    //    }

    //    public static SeedGenerator Instance()
    //    {
    //        return _instance;
    //    }

    //    private void Initialize()
    //    {
    //        // firstSeed is chosen based on clock() and used for the first rng
    //        ulong firstSeed = (ulong) DateTime.Now.Ticks;
    //        MersenneTwister first = new MersenneTwister(firstSeed);
    //        // secondSeed is as random as it could be
    //        // feel free to suggest improvements
    //        ulong secondSeed = first.NextInt32();
    //        MersenneTwister second = new MersenneTwister(secondSeed);
    //        // use the second rng to initialize the final one
    //        ulong skip = second.NextInt32() % 1000;
    //        var init = new List<ulong>(4)
    //        {
    //            [0] = second.NextInt32(),
    //            [1] = second.NextInt32(),
    //            [2] = second.NextInt32(),
    //            [3] = second.NextInt32()
    //        };
    //        _rng = new MersenneTwister(init);
    //        for (ulong i = 0; i < skip; i++)
    //            _rng.NextInt32();
    //    }
    //}
}