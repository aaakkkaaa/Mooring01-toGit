using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Wind : MonoBehaviour
{ 
    public WindWirections[] WindDir;

}


[Serializable]
public struct WindWirections
{
    public float posX;
    public float posZ;
    public float angle;
    public float value;
}
