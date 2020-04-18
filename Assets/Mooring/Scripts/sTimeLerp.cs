using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class sTimeLerp : MonoBehaviour
{
    // Интерполированная величина
    public float lerpValue;

    // Таймер отсчета времени от начала работы программы
    private Stopwatch _Timer;

    // Start is called before the first frame update
    void Start()
    {
        // Проверка
        //MyLerp(0f, 1f, 5f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
    }

    // линейно изменяем публичную величину lerpValue с течением времени
    // Min - начальное значение
    // Max - начальное значение
    // Duration - продолжительность периода интерполяции
    // Period - периодичность обновления значений (0 - каждый Update, -1 - каждый FixedUpdate, другие значения - время в сек.)

    public void MyLerp(float Min, float Max, float Duration, float Period)
    {
        _Timer = new Stopwatch();
        _Timer.Start();
        long StartTime = _Timer.ElapsedMilliseconds;
        float DurationMS = Duration * 1000f;
        long EndTime = StartTime + (long)DurationMS;
        StartCoroutine(ChangeValue(StartTime, DurationMS, EndTime, Min, Max, Period));
    }


    private IEnumerator ChangeValue(long StartTime, float DurationMS, long EndTime, float Min, float Max, float Period)
    {
        while (true)
        {

            long CurrentTime = _Timer.ElapsedMilliseconds;

            if (CurrentTime > EndTime)
            {
                lerpValue = Max;
                yield break;
            }

            float NormalizedTime = (CurrentTime - StartTime) / DurationMS;
            lerpValue = Mathf.Lerp(Min, Max, NormalizedTime);
            switch (Period)
            {
                case 0f:
                    yield return new WaitForEndOfFrame();
                    break;
                case -1f:
                    yield return new WaitForFixedUpdate();
                    break;
                default:
                    yield return new WaitForSeconds(Period);
                    break;
            }
        }
    }
}
