using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Traveler : MonoBehaviour
{
    // текущая цель движения
    public Transform Target;

    private Animator _animator;

    private string _state = "";
    private float _normTimeStart;
    private float _normTimeCur;
    private Vector3 _posStart;


    void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
        {
            if (Input.GetKeyDown("r")) // повернуться к нужной точке
            {
                bool isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
                bool isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
                if (!(isRotLeft || isRotRight) )
                {
                    StartCoroutine(RotateAndGo("Rotate"));
                }
            }
            if (Input.GetKeyDown("g")) // сделать шаг к нужной точке
            {
                bool isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
                bool isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
                if (!(isRotLeft || isRotRight))
                {
                    _state = "Go";
                    StartCoroutine(RotateAndGo("Go"));
                }
            }
        }
    }

    public IEnumerator RotateAndGo( string task)
    {
        if (!_animator.isMatchingTarget)
        {
            if (task == "Rotate")
            {
                Quaternion correctRot = Quaternion.LookRotation(Target.position - transform.position);
                Vector3 angle = correctRot.eulerAngles;
                angle.x = 0;
                angle.z = 0;
                Vector3 curRot = transform.eulerAngles;
                float dY = Misc.NormalizeAngle(curRot.y) - Misc.NormalizeAngle(angle.y);
                if (dY > 0)
                {
                    _animator.SetTrigger("RotLeft");
                }
                else
                {
                    _animator.SetTrigger("RotRight");
                }
                print("RotateAndGo() - запуск -> " + _animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

                bool isRotLeft;
                bool isRotRight;
                do
                {
                    //print("проверка в карутине Rotate");
                    isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
                    isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");

                    yield return null;
                }
                while (!(isRotLeft || isRotRight));

                print("RotateLeft = " + isRotLeft + "  RotateRight = " + isRotRight);

                correctRot.eulerAngles = angle;
                //print(angle);
                print("RotateAndGo() - MatchTarget -> " + _animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                _animator.MatchTarget(Vector3.zero,
                                       correctRot,
                                       AvatarTarget.Root,
                                       new MatchTargetWeightMask(new Vector3(0, 0, 0), 1),
                                       _animator.GetCurrentAnimatorStateInfo(0).normalizedTime,
                                       0.6f);
            }
            else if(task == "Go")
            {
                _animator.SetTrigger("Go2Steps");
                /*
                print("RotateAndGo() - запуск -> " + _animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                _animator.SetTrigger("Go2Steps");
                do
                {
                    print("проверка в карутине Walk");
                    yield return null;
                }
                while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"));

            
                print("RotateAndGo() - MatchTarget -> " + _animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

                Vector3 newPos = Target.position;
                _animator.MatchTarget( newPos,
                                        Quaternion.identity,
                                        AvatarTarget.Root,
                                        new MatchTargetWeightMask(new Vector3(1, 1, 1), 0),
                                        _animator.GetCurrentAnimatorStateInfo(0).normalizedTime,
                                        0.8f);
                */
            }

        }
    }

    private void OnAnimatorMove()
    {

        if (_state == "Go")
        {
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                _state = "";
            }
            else 
            { 
                _normTimeCur = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                float dt = (_normTimeCur - _normTimeStart) / (1.0f - _normTimeStart);
                Vector3 dL = (Target.localPosition - _posStart) * dt;
                gameObject.transform.localPosition = _posStart + dL;
            }
        }
        else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            _state = "Go";
            _normTimeStart = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            _posStart = gameObject.transform.localPosition;
        }
    }
}
