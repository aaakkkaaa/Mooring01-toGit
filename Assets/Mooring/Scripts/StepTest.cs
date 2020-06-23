using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepTest : MonoBehaviour
{
    private Vector3 _curPos;
    
    void Start()
    {
        _curPos = transform.position;
    }


    void onStep()
    {
        Vector3 newPos = transform.position;
        print("длинна шага = "+(_curPos-newPos).magnitude) ;
        _curPos = newPos;
    }

}
