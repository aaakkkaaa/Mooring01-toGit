using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepTest : MonoBehaviour
{
    private Vector3 _oldPos;
    private float _oldTime;

    void Start()
    {
        _oldPos = transform.position;
        _oldTime = Time.time;
    }


    void onStep()
    {
        Vector3 newPos = transform.position;
        float newTime = Time.time;

        float l = (newPos - _oldPos).magnitude;
        float t = (newTime - _oldTime);

        print("длинна шага = " + l + "    Время на шаг = " + t + "    Скорость = " + (l / t));
        _oldPos = newPos;
        _oldTime = newTime;
    }

}
