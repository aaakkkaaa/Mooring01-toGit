using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

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
    // индекс точки, к которой идем
    private int _idx;

    // направление движения на данном шаге
    private Transform _target;
    // стартовая позиция, с которой начинается движение
    private Vector3 _posStart;
    // начальный угол поворота вокруг Y, с которого начинается поворот
    private float _angleYStart;
    // изменение начального угла, приводящее его к желаемому
    private float _deltaY;

    // расстояние, проходимое за полный шаг (левой + правой)
    private float _stepL = 1.17f;
    // скорость движения при ходьбе
    private float _stepV = 0.88f;

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
                WalkTo(WalkTarget.name);
            }
        }
    }

    // определить путь и начать поворот, потом движение 
    public void WalkTo(string pointName)
    {
        _path = _pathMan.getPath(CurPos.name, pointName);
        // поворот в сторону следующей точки маршрута
        _state = "WaitRotate";
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
        //_deltaY = Misc.NormalizeAngle(curRot.y) - Misc.NormalizeAngle(angle.y);
        _deltaY = Misc.NormalizeAngle(curRot.y - angle.y);
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

    // движение и вращение перса
    private void OnAnimatorMove()
    {
        if (_state == "WaitRotate")   // нужно дождаться, когда заработают анимации, и установить начальные значения
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
        }
        else if (_state == "Rotate1")
        {
            // поворот на месте к первой точке маршрута
            bool isRotLeft;
            bool isRotRight;
            isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
            isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
            if (isRotLeft || isRotRight)
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
        }
        else if (_state == "Walk1")
        {
            // идем к очередной точке маршрута _idx
            _normTimeCur = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            Vector3 direct = (_path[_idx].localPosition - transform.localPosition).normalized;
            Vector3 dPos = direct * _stepV * Time.deltaTime;
            Vector3 curPos = transform.localPosition;
            transform.localPosition = curPos + dPos;




            float l0 = (_path[_idx - 1].localPosition - transform.localPosition).magnitude;     // пройдено
            float l1 = (_path[_idx - 1].localPosition - _path[_idx].localPosition).magnitude;   // весь отрезок
            print("l0 = " + l0 + "   l1 = " + l1);
            if (l0 > l1)
            {
                // надо теперь идти к следующей точке
                if (_idx < _path.Count - 1)
                {
                    _idx++;
                    _target = _path[_idx];
                    print(_target.name);
                }


                // надо включить коррекцию направления, временно - скачком
                Quaternion correctRot = Quaternion.LookRotation(_target.localPosition - transform.localPosition);
                Vector3 angle = correctRot.eulerAngles;
                //print(angle);
                angle.x = 0;
                angle.z = 0;
                transform.localEulerAngles = angle;

            }


        }


    }

    // вызывается из анимаций поворота в конце
    void Rotate1End()
    {
        print("Rotate1End");
        // надо смотреть расстояние не до ближайшей точки, а до последней!
        float toLast = (_path[_path.Count - 1].localPosition - _path[0].localPosition).magnitude;
        if (toLast > _stepL) // идти больше шага
        {
            _idx = 1;   // идем к первой точке
            _normTimeStart = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            _posStart = gameObject.transform.localPosition;
            _animator.SetTrigger("GoLong");
            _state = "Walk1";

            print("_normTimeStart = " + _normTimeStart);
        }

    }




}
