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
    [NonSerialized] public List<string> path;
    // Использовать для данного судна специальный путь
    [SerializeField]
    private bool _useSpecificPath;
    // Специальный путь для данного судна
    [SerializeField]
    private List<string> _specificPath;
    // точки пути, смещенные вправо
    private List<Vector3> _realPath;
    // велечина смещения вправо
    private float lOrt = 3.0f;

    // индекс текущей точки
    private int _curIdx;

    private string _state = "BEGIN";

    // класс со служебными функциями
    private sAssist _assist;

    // для контроля за параметрами и демпфирования
    private Rigidbody _rBody;

    // все самодвижущиеся лодки
    private BoatMoving[] _boats;
    // индекс данной лодки
    private int _myBoatidx;
    // дистанция опасного сближения
    private float _dangerDist = 50.0f;
    // Для выключения корутины (пока не используется)
    private IEnumerator _detectDanger;

    // для отладки построения вспомогательного пути
    public GameObject GreenObj;
    private List<GameObject> _allGreen;

    void Start()
    {
        _pathMngr = FindObjectsOfType<BoatPathManager>()[0];
        //print(_pathMngr.name);
        _points = _pathMngr.gameObject;
        transform.position = startPoint.transform.position;

        _assist = FindObjectsOfType<sAssist>()[0];

        _rBody = GetComponent<Rigidbody>();

        // для определения опасного сближения подготовка данных
        _boats = FindObjectsOfType<BoatMoving>();
        _myBoatidx = Array.IndexOf(_boats, this);
        print("Индекс данной лодки = " + _myBoatidx);

        // запуск процесса определения опасного сближения
        _detectDanger = DetectDanger();
        StartCoroutine(_detectDanger);
    }

    private void FixedUpdate()
    {
        // Начинаем плыть
        if (startPoint != null && (_state == "IDLE" || _state == "BEGIN") )
        {
            print(gameObject.name + " -> Начинаем плыть");

            // Построить путь судна
            if (_useSpecificPath)
            {
                // Особый путь
                path = _specificPath;
            }
            else
            {
                // Путь по общим отрезкам маршрутов (_branches в BoatPathManager) с элементами случайности
                path = _pathMngr.CreatePath(startPoint.name);
            }

            // создать массив из точек, смещенных вправо относительно точек заданного пути
            CreateRealPath( );
            DebugShowRealPath();
            
            if(_state == "BEGIN") // первый заход, мы стоим в несмещенной точке - надо сместить
            {
                transform.position = _realPath[0];
            }
            else
            {
                // мы стоим в предыдущей точке, куда только что приплыли, надо ее добавить в путь
                _realPath.Insert(0, transform.position);
            }
            _curIdx = 0;

            /*

            _curP = startPoint;
            _nextP = _points.transform.Find(path[_curIdx + 1]).gameObject;
            print(gameObject.name + "  START  _nextP = " + _nextP.name);
            */

            _Vz = 0;

            _state = "START";
        }

        if (_state == "START")
        {
            // перед началом движения поворачиваем нос к следующей точке
            Vector3 zGlobal = transform.TransformDirection(Vector3.forward);
            //_dirToNext = _nextP.transform.position - transform.position;
            _dirToNext = _realPath[1] - transform.position;
            //_dirToNext.x = _dirToNext.z = 0;
            Quaternion rot = Quaternion.FromToRotation(zGlobal, _dirToNext);
            float ang = rot.eulerAngles.y;
            ang = _assist.NormalizeAngle(ang);

            //print("ang = " + ang + "     ang1 = " + ang1);
            float dAng;
            if (Mathf.Abs(ang) > rotV * Time.fixedDeltaTime)
            {
                dAng = Mathf.Sign(ang) * rotV * Time.fixedDeltaTime;
            }
            else
            {
                dAng = ang;
                _state = "ACCELERATION";
                //print(gameObject.name + " Заканчиваем начальный поворот  dAng = " + dAng);
            }
            Vector3 curRot = transform.eulerAngles;
            curRot.y += dAng;
            transform.eulerAngles = curRot;

        }
        if (_state == "ACCELERATION")
        {
            if (_Vz < maxVz)
            {
                _Vz += Az * Time.fixedDeltaTime;
            }
            if (_Vz > maxVz)
            {
                _Vz = maxVz;
                _state = "STRAIGHT";
                //print(gameObject.name + "  Заканчиваем ускорение");
            }
            CorrectDirection( _realPath[_curIdx+1] );
            MovingStep();
        }
        if (_state == "STRAIGHT")
        {
            CorrectDirection(_realPath[_curIdx + 1]);
            MovingStep();

            // расстояние до ближайшей точки
            float len = (transform.position - _realPath[_curIdx + 1]).magnitude;
            if (_curIdx == _realPath.Count - 2)
            {
                // предпоследняя точка, проверяем, не пора ли замедляться
                if (len < LenSlow)
                {
                    _state = "SLOW";
                    Aslow = _Vz * _Vz / 2 / len;
                    //print(gameObject.name + "  Останавливаемся   Aslow = " + Aslow);
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
                    /*
                    GameObject next2P = _points.transform.Find(path[_curIdx + 2]).gameObject;
                    Vector3 nextDir = next2P.transform.position - _nextP.transform.position;
                    */
                    Vector3 nextDir = _realPath[_curIdx + 2] - _realPath[_curIdx + 1];
                    nextDir.y = transform.position.y;
                    nextDir = nextDir.normalized;
                    // отступаем от следующей точки в найденном направлении на расстояние len
                    //_P2 = _nextP.transform.position + nextDir * len;
                    _P2 = _realPath[_curIdx + 1] + nextDir * len;
                    _P2.y = transform.position.y;
                    _P1 = _realPath[_curIdx + 1];
                    _P1.y = transform.position.y;
                    _p0q0 = 0;
                    _p0p1 = (_P1 - _P0).magnitude;
                    //print(gameObject.name + " ROTATION   _P0 = " + _P0 + "   _P1 = " + _P1 + "    _P2 = " + _P2);

                }
            }
            return;
        }
        if (_state == "SLOW")
        {
            //CorrectDirection();
            _Vz -= Aslow*Time.fixedDeltaTime;
            if (_Vz <= 0)
            {
                _Vz = 0;
                _state = "IDLE";
                //print(gameObject.name + " Остановились");
                // установить новую текущую точку
                startPoint = _points.transform.Find(path[path.Count - 1]).gameObject;

            }
            else
            {
                MovingStep();
            }
        }
        if (_state == "ROTATION")
        {
            float dVz = _Vz * Time.fixedDeltaTime;
            float newL = _p0q0 + dVz;
            // не пора ли выходить из разворота
            if (newL >= _p0p1)
            {
                _state = "STRAIGHT";
                //_curP = _nextP;
                _curIdx++;
                //_nextP = _points.transform.Find(path[_curIdx + 1]).gameObject;
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
                _p0q0 += _Vz * Time.fixedDeltaTime / correction;
            }
        }
    }

    // смещение на каждом шаге
    private void MovingStep()
    {
        Vector3 locPos = new Vector3(0, 0, _Vz*Time.fixedDeltaTime);
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

    // определение угрозы столкновения
    private IEnumerator DetectDanger()
    {
        do
        {
            yield return new WaitForSeconds(2);
            for (int i = _myBoatidx+1; i < _boats.Length; i++)
            {
                float dist = (transform.position - _boats[i].transform.position).magnitude;
                if ( dist < _dangerDist)
                {
                    print("Опасное сближение лодок " + _myBoatidx + " и " + i);
                }
            }
        } while (true);
       
    }

    // на основе path создать 
    private void CreateRealPath( )
    {
        _realPath = new List<Vector3>();
        // первая точка
        Vector3 p0 = _points.transform.Find(path[0]).position;
        Vector3 p1 = _points.transform.Find(path[1]).position;
        Vector3 direct0 = (p1 - p0).normalized;
        Vector3 ort0 = new Vector3(direct0.z, direct0.y, -direct0.x);
        Vector3 real = p0 + ort0 * lOrt;
        _realPath.Add(real);

        // промежуточные точки
        for(int i=1; i<path.Count-1; i++)
        {
            Vector3 direct1 = (p1 - p0).normalized;
            Vector3 ort1 = new Vector3(direct1.z, direct1.y, -direct1.x);

            p0 = p1;
            p1 = _points.transform.Find(path[i+1]).position;
            Vector3 direct2 = (p1 - p0).normalized;
            Vector3 ort2 = new Vector3(direct2.z, direct2.y, -direct2.x);

            print("ort1 = " + ort1 + "  ort2 = " + ort2);
            Vector3 realOrt = ((ort1 + ort2) / 2).normalized;

            float angel = Vector3.SignedAngle(direct1, direct2, Vector3.up)/2.0f;
            float cosAng = Mathf.Cos(angel / 180 * Mathf.PI);
            if ( Mathf.Abs(cosAng) > 0.01f )
            {
                real = p0 + realOrt / cosAng * lOrt;
            }
            else
            {
                print("Слишком острый угол " + _points.transform.Find(path[i + 1]).gameObject.name);
                real = p0;
            }

            _realPath.Add(real);

        }

        // последняя точка
        direct0 = (p1 - p0).normalized;
        ort0 = new Vector3(direct0.z, direct0.y, -direct0.x);
        real = p1 + ort0 * lOrt;
        _realPath.Add(real);

    }

    private void DebugShowRealPath()
    {
        if (GreenObj == null)
        {
            print("Нет зеленого цилиндра");
            return;
        }
        if(_allGreen != null)
        {
            for (int i = 0; i < _allGreen.Count; i++ )
            {
                Destroy(_allGreen[i]);
            }
        }
        _allGreen = new List<GameObject>();
        for(int i=0; i<_realPath.Count; i++)
        {
            GameObject green = Instantiate<GameObject>(GreenObj);
            green.transform.position = _realPath[i];
            _allGreen.Add(green);
        }

        
    }

}



