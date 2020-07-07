using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditorInternal;

public class PathWalker : MonoBehaviour
{
    // Допустимый путь для этого персонажа
    public List<Transform> Points;

    // Где мы сейчас
    public GameObject CurPos;
    // куда идти
    public GameObject WalkTarget;

    // маршрутные точки, последняя - цель, там надо будет развернуться
    private List<Transform> _path;

    private Animator _animator;

    private string _state = "";

    private float _normTimeStart;
    private float _normTimeCur;
    // индекс точки, к которой идем
    private int _idx;

    // направление движения на данном шаге
    private Transform _target;
    // начальный угол поворота вокруг Y, с которого начинается поворот
    private float _angleYStart;
    // целевой угол поворота вокруг Y
    private float _angleYFinish;
    // изменение начального угла, приводящее его к желаемому
    private float _deltaY;
    // начальное положение при моделировании последнего шага
    private Vector3 _posStart;

    // расстояние, проходимое за полный шаг (левой + правой)
    private float _stepL = 1.17f;
    // скорость движения при ходьбе
    private float _stepV = 0.88f;
    // максимальный угол поворота за одну итерацию коррекции угла
    private float _dAngleY = 3.0f;
    // Для остановки корутины в конце движения
    private IEnumerator _angleCorrector;

    void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
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
        _path = DetectPath(CurPos.name, pointName);
        if(_path.Count == 0)
        {
            print("Не найден путь в " + pointName);
            return;
        }
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
        _deltaY = Misc.NormalizeAngle(curRot.y - angle.y);
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
        if (_state == "WaitRotate")   // нужно дождаться, когда заработает анимация, и установить начальные значения
        {
            bool isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
            bool isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
            if (isRotLeft || isRotRight)
            {
                // началось вращение, установим необходимые параметры для следующего входа в OnAnimatorMove
                print("началось вращение");
                _state = "Rotate1";         // вращение перед перемещением
                _normTimeStart = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                print("_normTimeStart = " + _normTimeStart);
            }
        }
        else if (_state == "Rotate1")
        {
            // поворот на месте к первой точке маршрута
            bool isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
            bool isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
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
            else
            {
                // поворот закончился
                float toLast = (_path[_path.Count - 1].localPosition - _path[0].localPosition).magnitude;
                // идем к первой точке
                _idx = 1;
                _animator.SetTrigger("GoLong");
                _normTimeStart = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                _state = "Walk1";

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
            //print("l0 = " + l0 + "   l1 = " + l1);
            if (l0 > l1)
            {
                if (_idx < _path.Count - 1)
                {
                    // надо теперь идти к следующей точке
                    _idx++;
                    _target = _path[_idx];
                    print(_target.name);

                    // запускаем коррекцию угла
                    _angleCorrector = AngleCorrector();
                    StartCoroutine(_angleCorrector);
                }
                else
                {
                    // дошли до последней точки, останавливаемся 
                    _animator.SetTrigger("StayHere");
                    _state = "StayHere";
                }

            }

        }
        else if (_state == "StayHere")
        {
            if (_angleCorrector != null)
            {
                StopCoroutine(_angleCorrector);
            }
            // идет запуск анимации iddle, ждем когда она запустится
            if(_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                // определим, в какую сторону надо делать поворот
                print("_target = " + _target);
                _angleYFinish = _target.transform.localEulerAngles.y;
                _angleYStart = transform.localEulerAngles.y;
                _deltaY = Misc.NormalizeAngle(_angleYFinish - _angleYStart);
                if (_deltaY < 0)
                {
                    _animator.SetTrigger("RotLeft");
                    print("RotLeft");
                }
                else
                {
                    _animator.SetTrigger("RotRight");
                    print("RotRight");
                }
                _state = "WaitRotate2";
            }
        }
        else if (_state == "WaitRotate2")
        {
            bool isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
            bool isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
            if (isRotLeft || isRotRight)
            {
                // началось вращение, установим необходимые параметры для следующего входа в OnAnimatorMove
                _state = "Rotate2";         // вращение после перемещения
                _normTimeStart = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                print("Rotate2: _normTimeStart = " + _normTimeStart);
            }
        }
        else if (_state == "Rotate2")
        {
            bool isRotLeft = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateLeft");
            bool isRotRight = _animator.GetCurrentAnimatorStateInfo(0).IsName("RotateRight");
            if (isRotLeft || isRotRight)
            {
                _normTimeCur = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                //print(_normTimeCur);
                float dt = (_normTimeCur - _normTimeStart) / (1.0f - _normTimeStart);
                //print(dt);
                float dY = _deltaY * dt;
                //print("dY = " + dY);
                Vector3 lE = transform.localEulerAngles;
                lE.y = _angleYStart + dY;
                transform.localEulerAngles = lE;
            }
            else
            {
                _state = "";
                CurPos = _target.gameObject;
            }

        }


    }

    // вызывается из анимации поворота в конце
    void Rotate1End()
    {
        /*
        print("_state = " + _state);
        if (_state == "Rotate1")
        {
            print(gameObject.name + " -> Rotate1End");
            float toLast = (_path[_path.Count - 1].localPosition - _path[0].localPosition).magnitude;
            // идем к первой точке
            _idx = 1;
            _animator.SetTrigger("GoLong");
            _normTimeStart = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            _state = "Walk1";

            print("_normTimeStart = " + _normTimeStart);
        }
        */

    }

    // вызывается из анимации в конце каждого шага
    void StepEnd()
    {
        //print("StepEnd()");
    }

    // корректировка вращения при ходьбе
    private IEnumerator AngleCorrector()
    {
        float dAngleY;
        
        Quaternion correctRot = Quaternion.LookRotation(_target.localPosition - transform.localPosition);
        Vector3 corrAngle = correctRot.eulerAngles;
        corrAngle.x = 0;
        corrAngle.z = 0;

        while ( Mathf.Abs(transform.localEulerAngles.y - corrAngle.y) > _dAngleY)
        {
            print("transform.localEulerAngles.y = " + transform.localEulerAngles.y + "    angle.y = " + corrAngle.y);

            if (Misc.NormalizeAngle(transform.localEulerAngles.y - corrAngle.y) > 0)
            {
                dAngleY = -_dAngleY;
            }
            else
            {
                dAngleY = _dAngleY;
            }
            Vector3 curLocAngle = transform.localEulerAngles;
            curLocAngle.y += dAngleY;
            curLocAngle.x = 0;
            curLocAngle.z = 0;
            transform.localEulerAngles = curLocAngle;
            print("transform.localEulerAngles = " + transform.localEulerAngles);
            yield return new WaitForFixedUpdate(); ;
            correctRot = Quaternion.LookRotation(_target.localPosition - transform.localPosition);
            corrAngle = correctRot.eulerAngles;
        }
        // встаем в правильном направлении и заканчиваем вращение

        transform.localEulerAngles = corrAngle;
        
        yield return null;

    }

    public List<Transform> DetectPath(string start, string finish)
    {
        List<Transform> result = new List<Transform>();

        // если начало и конец совпадают, вернем пустой список
        if (start == finish)
        {
            return result;
        }

        // определим индексы в списке Points начальной и конечной точек маршрута
        int iStart = -1;
        int iFinish = -1;
        for (int i = 0; i < Points.Count; i++)
        {
            if (Points[i].name == start)
            {
                iStart = i;
            }
            if (Points[i].name == finish)
            {
                iFinish = i;
            }
        }

        // если не нашелся какой-то конец маршрута, вернем пустой список
        if (iStart == -1 || iFinish == -1)
        {
            return result;
        }

        // сформируем список из маршрутных точек
        int step = (iStart < iFinish) ? 1 : -1;
        for (int i = iStart; i != iFinish; i += step)
        {
            result.Add(Points[i]);
        }
        result.Add(Points[iFinish]);

        return result;
    }



}
