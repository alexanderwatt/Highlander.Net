/*
    Copyright (c) 2006, Corey Goldberg

    StopWatch.cs is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.
*/


using System;


public class StopWatch
{

    private DateTime startTime;
    private DateTime stopTime;
    private bool running = false;

    public void Start()
    {
        this.startTime = DateTime.Now;
        this.running = true;
    }

    public void Stop()
    {
        this.stopTime = DateTime.Now;
        this.running = false;
    }

    // elaspsed time in milliseconds
    public double GetElapsedTimeMs()
    {
        TimeSpan interval;

        if (running)
            interval = DateTime.Now - startTime;
        else
            interval = stopTime - startTime;

        return interval.TotalMilliseconds;
    }

    public TimeSpan GetElapsedTime()
    {
        TimeSpan interval;
        if (running)
            interval = DateTime.Now - startTime;
        else
            interval = stopTime - startTime;
        return interval;
    }

    // elaspsed time in seconds
    public double GetElapsedTimeSecs()
    {
        TimeSpan interval;

        if (running)
            interval = DateTime.Now - startTime;
        else
            interval = stopTime - startTime;

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