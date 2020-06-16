using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Misc 
{
    // Нормализация угла: привести угол к (-180/+180)
    public static float NormalizeAngle(float myAngle)
    {
        while (myAngle > 180.0f)
        {
            myAngle -= 360.0f;
        }
        while (myAngle < -180.0f)
        {
            myAngle += 360.0f;
        }
        return myAngle;
    }
}
