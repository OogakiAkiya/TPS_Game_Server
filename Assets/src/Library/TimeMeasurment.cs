using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TimeMeasurment
{
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    //fps
    int frameCount = 0;
    float prevTime = 0;
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

    //より正確な時間がわかる
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



    public void Fps(string _before = "")
    {
        ++frameCount;
        float time = Time.realtimeSinceStartup - prevTime;

        if (time >= 1f)
        {
            FileController.GetInstance().Write("fps", _before + frameCount / time);
            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;
        }
    }

}