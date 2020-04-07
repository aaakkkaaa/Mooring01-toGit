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
    public float angle;
    public float value;
}
