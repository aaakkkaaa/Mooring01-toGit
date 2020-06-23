using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathWalker : MonoBehaviour
{
    // параметры базовой утки, левой если смотреть со стороны маринеро
    public Transform BaseTransf;        // не null только для маринеро
    // параметры текущей утки
    public Transform CurrTransf;        // не null только для маринеро

    // Где мы сейчас
    public GameObject CurPos;
    // куда идти
    public GameObject WalkTarget;

    // маршрутные точки, последняя - цель, там надо будет развернуться
    private List<Transform> _path;

    // хранитель путей
    private PathManager _pathMan;

    private Animator _animator;

    private string _state = "";

    private float _normTimeStart;
    private float _normTimeCur;

    // направление движения на данном шаге
    private Transform _target;
    // стартовая позиция, с которой начинается движение
    private Vector3 _posStart;
    // начальный угол поворота вокруг Y, с которого начинается поворот
    private float _angleYStart;
    // изменение начального угла, приводящее его к желаемому
    private float _deltaY;

    private float _stepL = 1.17f;

    void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
        _pathMan = FindObjectOfType<PathManager>();
        //print( name + ".PathWalker -> _pathMan = " + _pathMan.name);
    }

    void Update()
    {
        if (Input.GetKeyDown("g")) // сделать шаг к нужной точке
        {
            bool isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
            bool isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
            bool isWalk = _animator.GetCurrentAnimatorStateInfo(0).IsName("Walk");
            bool isGo2Steps = _animator.GetCurrentAnimatorStateInfo(0).IsName("Go2Steps");
            if (!(isRotLeft || isRotRight || isWalk || isGo2Steps))
            {
                //StartCoroutine(RotateAndGo("Go"));
                WalkTo(WalkTarget.name);
            }
        }
    }

    public void WalkTo(string pointName )
    {
        _path = _pathMan.getPath(CurPos.name, pointName);
        // поворот в сторону следующей точки маршрута
        _state = "";
        _target = _path[1];
        print(_target.name);
        Quaternion correctRot = Quaternion.LookRotation(_target.localPosition - transform.localPosition);
        Vector3 angle = correctRot.eulerAngles;
        //print(angle);
        angle.x = 0;
        angle.z = 0;
        Vector3 curRot = transform.localEulerAngles;
        _angleYStart = curRot.y;
        //print(curRot);
        _deltaY = Misc.NormalizeAngle(curRot.y) - Misc.NormalizeAngle(angle.y);
        //print(_deltaY);
        if (_deltaY > 0)
        {
            _animator.SetTrigger("RotLeft");
        }
        else
        {
            _animator.SetTrigger("RotRight");
        }

    }

    private void OnAnimatorMove()
    {
        if (_state == "")   // нужно дождаться, когда заработают анимации, и установить начальные значения
        {
            bool isRotLeft;
            bool isRotRight;
            isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
            isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
            if (isRotLeft || isRotRight)
            {
                // началось вращение, установим необходимые параметры для следующего входа в OnAnimatorMove
                //print("началось вращение");
                _state = "Rotate1";         // вращение перед перемещением
                _normTimeStart = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                print("_normTimeStart = " + _normTimeStart);
            }
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                // началась движение

            }

        }
        else if(_state == "Rotate1")
        {
            bool isRotLeft;
            bool isRotRight;
            isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
            isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
            if( isRotLeft || isRotRight )
            {
                _normTimeCur = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                //print(_normTimeCur);
                float dt = (_normTimeCur - _normTimeStart) / (1.0f - _normTimeStart);
                //print(dt);
                float dY = _deltaY * dt;
                //print("dY = " + dY);
                Vector3 lE = transform.localEulerAngles;
                lE.y = _angleYStart - dY;
                transform.localEulerAngles = lE;
                //print("dY = " + dY);
            }
            else
            {
                // закончилось начальное вращение

                _state = "";
            }


        }
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
                Vector3 dL = (_target.localPosition - _posStart) * dt;
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
