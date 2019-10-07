using System;
using System.Collections;
using System.Collections.Generic;

public class TimeMeasurment
{
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    public double Start(Action _action, int _times)
    {
        double avarage = 0;
        for (int i = 0; i < _times; i++)
        {
            timer.Reset();
            timer.Start();
            _action();
            timer.Stop();
            avarage = (double)timer.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency;

        }
        return avarage /= (double)_times;
    }

    public long StartMS(Action _action, int _times)
    {
        long avarage = 0;
        for (int i = 0; i < _times; i++)
        {
            timer.Reset();
            timer.Start();
            _action();
            timer.Stop();
            avarage = timer.ElapsedMilliseconds;

        }
        return avarage /= _times;
    }

}