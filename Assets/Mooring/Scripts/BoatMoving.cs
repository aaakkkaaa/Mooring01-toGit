using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatMoving : MonoBehaviour
{
    // начало движения
    public GameObject startPoint;
    // угловая скорость (для вращения на месте в начале)
    public float rotV = 15.0f;
    // максимальная линейная скорость
    public float maxVz = 3.0f;
    // Ускорение 
    public float Az = 1.5f;
    // расстояние до точки, на котором начинается поворот
    public float LenRot = 10.0f;
    // расстояние до последней точки, на котором начинается тормоз
    public float LenSlow = 15.0f;

    // текущая линейная скорость
    private float _Vz=0;
    // ускорение при замедлении
    private float Aslow;

    // текущая точка
    private GameObject _curP;
    // следующая точка
    private GameObject _nextP;
    // направление на следующую точку
    private Vector3 _dirToNext;

    // для кривой Безье - текущий путь от P0 до Q0
    private float _p0q0;
    // точка, с которой начинаем поворот
    private Vector3 _P0;
    // точка, в которой должны закончить поворот
    private Vector3 _P2;
    // точка - вершина поворота
    private Vector3 _P1;
    // расстояние от начала поворота до вершины поворота
    private float _p0p1;


    // тут лежат все объекты-точки
    private GameObject _points;
    // возвращает путь по начальной точке
    private BoatPathManager _pathMngr;
    // имена всех точек пути
    private List<string> _path;
    // Использовать для данного судная специальный путь
    [SerializeField]
    private bool _useSpecificPath;
    // Специальный путь для данного судна
    [SerializeField]
    private List<string> _specificPath;
    // индекс текущей точки
    private int _curIdx;

    private string _state = "IDLE";

    // класс со служебными функциями
    private sAssist _assist;


    void Start()
    {
        _pathMngr = FindObjectsOfType<BoatPathManager>()[0];
        //print(_pathMngr.name);
        _points = _pathMngr.gameObject;
        /*
        Vector3 startPos = startPoint.transform.position;
        startPos.y = transform.position.y;
        transform.position = startPos;
        */
        transform.position = startPoint.transform.position;

        _assist = FindObjectsOfType<sAssist>()[0];

    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        // Начинаем плыть
        if (startPoint != null && _state == "IDLE")
        {
            print(gameObject.name + " -> Начинаем плыть");

            // Построить путь судна
            if (_useSpecificPath)
            {
                // Особый путь
                _path = _specificPath;
            }
            else
            {
                // Путь по общим отрезкам маршрутов (_branches в BoatPathManager) с элементами случайности
                _path = _pathMngr.CreatePath(startPoint.name);
            }
            
            _state = "START";
            _curIdx = 0;
            _curP = startPoint;
            _nextP = _points.transform.Find(_path[_curIdx + 1]).gameObject;
            print(gameObject.name + "  START  _nextP = " + _nextP.name);
            _Vz = 0;
        }
        //}

        if (_state == "START")
        {
            // перед началом движения поворачиваем нос к следующей точке
            Vector3 zGlobal = transform.TransformDirection(Vector3.forward);
            _dirToNext = _nextP.transform.position - transform.position;
            //_dirToNext.x = _dirToNext.z = 0;
            Quaternion rot = Quaternion.FromToRotation(zGlobal, _dirToNext);
            float ang = rot.eulerAngles.y;
            ang = _assist.NormalizeAngle(ang);

            float ang1 = Vector3.Angle(zGlobal, _dirToNext);

            //print("ang = " + ang + "     ang1 = " + ang1);
            float dAng;
            if (Mathf.Abs(ang) > rotV * Time.deltaTime)
            {
                dAng = Mathf.Sign(ang) * rotV * Time.deltaTime;
            }
            else
            {
                dAng = ang;
                _state = "ACCELERATION";
                print(gameObject.name + " Заканчиваем начальный поворот  dAng = " + dAng);
            }
            Vector3 curRot = transform.eulerAngles;
            curRot.y += dAng;
            transform.eulerAngles = curRot;

        }
        if (_state == "ACCELERATION")
        {
            if (_Vz < maxVz)
            {
                _Vz += Az * Time.deltaTime;
            }
            if (_Vz > maxVz)
            {
                _Vz = maxVz;
                _state = "STRAIGHT";
                print(gameObject.name + "  Заканчиваем ускорение");
            }
            CorrectDirection( _nextP.transform.position );
            MovingStep();
        }
        if (_state == "STRAIGHT")
        {
            CorrectDirection(_nextP.transform.position );
            MovingStep();

            // расстояние до ближайшей точки
            float len = (transform.position - _nextP.transform.position).magnitude;
            if (_curIdx == _path.Count - 2)
            {
                // предпоследняя точка, проверяем, не пора ли замедляться
                if (len < LenSlow)
                {
                    _state = "SLOW";
                    Aslow = _Vz * _Vz / 2 / len;
                    print(gameObject.name + "  Останавливаемся   Aslow = " + Aslow);
                }
            }
            else
            {
                // проверяем расстояние до след. точки чтобы начать разворот
                if (len < LenRot)
                {
                    _state = "ROTATION";
                    _P0 = transform.position;
                    // точка окончания поворота лежит на прямой, соединяющей следующие две маршрутные точки
                    //print("ИЩУ: " + _path[_curIdx + 2]);
                    GameObject next2P = _points.transform.Find(_path[_curIdx + 2]).gameObject;
                    Vector3 nextDir = next2P.transform.position - _nextP.transform.position;
                    nextDir.y = transform.position.y;
                    nextDir = nextDir.normalized;
                    // отступаем от следующей точки в найденном направлении на расстояние len
                    _P2 = _nextP.transform.position + nextDir * len;
                    _P2.y = transform.position.y;
                    _P1 = _nextP.transform.position;
                    _P1.y = transform.position.y;
                    _p0q0 = 0;
                    _p0p1 = (_P1 - _P0).magnitude;
                    print(gameObject.name + " ROTATION   _P0 = " + _P0 + "   _P1 = " + _P1 + "    _P2 = " + _P2);

                }
            }
            return;
        }
        if (_state == "SLOW")
        {
            //CorrectDirection();
            _Vz -= Aslow*Time.deltaTime;
            if (_Vz <= 0)
            {
                _Vz = 0;
                _state = "IDLE";
                print(gameObject.name + " Остановились");
                // установить новую текущую точку
                startPoint = _points.transform.Find(_path[_path.Count - 1]).gameObject;

            }
            else
            {
                MovingStep();
            }
        }
        if (_state == "ROTATION")
        {
            float dVz = _Vz * Time.deltaTime;
            float newL = _p0q0 + dVz;
            // не пора ли выходить из разворота
            if (newL >= _p0p1)
            {
                _state = "STRAIGHT";
                _curP = _nextP;
                _curIdx++;
                _nextP = _points.transform.Find(_path[_curIdx + 1]).gameObject;
            }
            else
            {
                // первая итерация, предварительная
                float testT = newL / _p0p1;
                Vector3 Q0test = _P0 + (_P1 - _P0) * testT;
                Vector3 Q1test = _P1 + (_P2 - _P1) * testT;
                Vector3 Btest = Q0test + (Q1test - Q0test) * testT;
                // полученное расстояние отличается от того, которое лодка проходит, вычисляем на сколько надо скорректировать
                float correction = (Btest - transform.position).magnitude / dVz;
                //print("correction = " + correction);
                // вторая итерация, находим точку с учетом коррекции
                float realL = _p0q0 + dVz / correction;
                float realT = realL / _p0p1;
                Vector3 Q0 = _P0 + (_P1 - _P0) * realT;
                Vector3 Q1 = _P1 + (_P2 - _P1) * realT;
                Vector3 Breal = Q0 + (Q1 - Q0) * realT;
                // перемещение и разворот
                transform.position = Breal;
                CorrectDirection(Q1test);
                _p0q0 += _Vz * Time.deltaTime / correction;
            }
        }
    }

    // смещение на каждом шаге
    private void MovingStep()
    {
        Vector3 locPos = new Vector3(0, 0, _Vz*Time.deltaTime);
        Vector3 pos = transform.TransformPoint(locPos);
        transform.position = pos;
    }

    // коррекция направления при  движении к следующей точке 
    private void CorrectDirection( Vector3 nextPosition )
    {
        Vector3 zGlobal = transform.TransformDirection(Vector3.forward);
        _dirToNext = nextPosition - transform.position;
        Quaternion rot = Quaternion.FromToRotation(zGlobal, _dirToNext);
        float ang = rot.eulerAngles.y;
        ang = _assist.NormalizeAngle(ang);
        Vector3 curRot = transform.eulerAngles;
        curRot.y += ang;
        transform.eulerAngles = curRot;
    }

}
