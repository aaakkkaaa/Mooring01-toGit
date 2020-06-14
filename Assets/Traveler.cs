using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traveler : MonoBehaviour
{
    // текущая цель движения
    public Transform Target;

    private Animator _animator;
    private string _state = "";

    void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
        {
            if (Input.GetKeyDown("g")) // повернуться к нужной точке и идти туда 
            {
                if (_state == "")
                {
                    _state = "GO";
                    RotateAndGo();
                }
            }
        }
    }

    public void RotateAndGo()
    {
        if (_animator.isMatchingTarget) return;

        Vector3 CorrectPos = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        float rotAng = Vector3.Angle( transform.TransformDirection(Vector3.forward), (CorrectPos - transform.position) );
        print("rotAng = " + rotAng);


        _state = "";
    }
}
