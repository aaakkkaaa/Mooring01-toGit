using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Wind : MonoBehaviour
{ 
    public WindDirections[] WindDir;

}


[Serializable]
public struct WindDirections
{
    public float posX;
    public float posZ;
    public float angle; // Азимут (угол от направлениея на север): КУДА дует ветер. Восточный ветер: 270 (или -90) градусов
    public float value; // Скорость ветра в узлах
}
