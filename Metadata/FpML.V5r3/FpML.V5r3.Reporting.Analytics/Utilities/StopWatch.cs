/*
    Copyright (c) 2006, Corey Goldberg

    StopWatch.cs is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.
*/


using System;

namespace Orion.Analytics.Utilities
{
    public class StopWatch
    {

        private DateTime _startTime;
        private DateTime _stopTime;
        private bool _running;

        public void Start()
        {
            _startTime = DateTime.Now;
            _running = true;
        }

        public void Stop()
        {
            _stopTime = DateTime.Now;
            _running = false;
        }

        // elapsed time in milliseconds
        public double GetElapsedTimeMs()
        {
            TimeSpan interval;
            if (_running)
                interval = DateTime.Now - _startTime;
            else
                interval = _stopTime - _startTime;
            return interval.TotalMilliseconds;
        }

        public TimeSpan GetElapsedTime()
        {
            TimeSpan interval;
            if (_running)
                interval = DateTime.Now - _startTime;
            else
                interval = _stopTime - _startTime;
            return interval;
        }

        // elapsed time in seconds
        public double GetElapsedTimeSecs()
        {
            TimeSpan interval;
            if (_running)
                interval = DateTime.Now - _startTime;
            else
                interval = _stopTime - _startTime;
            return interval.TotalSeconds;
        }

        //// sample usage
        //public static void Main(String[] args)
        //{
        //    StopWatch s = new StopWatch();
        //    s.Start();
        //    // code you want to time goes here
        //    s.Stop();
        //    Console.WriteLine("elapsed time in milliseconds: " + s.GetElapsedTime());
        //}
    }
}