using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class sTime : MonoBehaviour
{

    // Точное время
    Stopwatch _StopWatch;
    public long StartTime;
    //public long UnixStartTime; // стартовое время Unix

    void Awake()
    {
        // Параметры времени
        _StopWatch = new Stopwatch();
        _StopWatch.Start();
        StartTime = _StopWatch.ElapsedMilliseconds;
        //UnixStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    // Возвращает текщее время работы программы в секундах
    public float CurrentTimeSec()
    {
        return (_StopWatch.ElapsedMilliseconds - StartTime) / 1000.0f;
    }

    // Возвращает текщее время работы программы в миллисекундах
    public int CurrentTimeMilliSec()
    {
        return (int)(_StopWatch.ElapsedMilliseconds - StartTime);
    }

}
