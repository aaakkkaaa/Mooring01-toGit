using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDrone : MonoBehaviour

// Предназначен для обзора пространства в режиме "дрона"
// Перемещается вместе с камерой с помощью клавиатуры, мыши, джойстика
// Имеет также выделенные точки для быстрого позиционирования горячими клавишами:
//    h - перелет в начальное положение, установленное в редакторе
//    b - вид на залив
//    y - перелет в положение "за штурвал" и вход в дочерние объекты яхты / выход из дочернего положения

{

    [SerializeField]
    float _KeyboardHorSpeed = 1f;

    [SerializeField]
    float _KeyboardVertSpeed = 1f;

    [SerializeField]
    float _KeyboardYawSpeed = 1f;

    [SerializeField]
    float _MouseHorSpeed = 1f;

    [SerializeField]
    float _MouseVertSpeed = 1f;

    [SerializeField]
    float _MouseYawSpeed = 1f;

    [SerializeField]
    float _JoystickHorSpeed = 1f;

    [SerializeField]
    float _JoystickVertSpeed = 1f;

    [SerializeField]
    float _JoystickYawSpeed = 1f;

    [SerializeField]
    float _HMin = 0f;

    [SerializeField]
    float _HMax = 2000f;

    Camera _referenceCamera;

    Vector3 _oldMousePos;

    float _OldMouseX;

    // Основная яхта
    Transform _Yacht;

    // Параметры перелета

    // Положение в начале перелета
    Vector3 _StartPos;
    Vector3 _StartEu;
    // Положение в конце перелета
    Vector3 _EndPos;
    Vector3 _EndEu;
    // Флаг перелета, блокирует управление
    bool _Flight = false;
    // Время начала перелета, сек
    float _StartTime;
    // Продолжительность перелета, сек
    [SerializeField]
    float _FlightTime = 2.0f;
    // Положение в начале сеанса
    Vector3 _HomePos;
    Vector3 _HomeEu;
    // Положение "Вид на залив"
    [SerializeField]
    Vector3 _BayPos;
    [SerializeField]
    Vector3 _BayEu;
    // Положение "за штурвалом" - локальный сдвиг относительно центра яхты
    [SerializeField]
    Vector3 _SkipperPos = new Vector3(1, 2, -5);

    // Вспомогательный объект - точка проекции камеры на горизонтальную плоскость аватара
    Transform _CameraPlumb;

    // Start is called before the first frame update
    void Start()
    {
        // Камера
        _referenceCamera = Camera.main;

        // Вспомогательный объект - точка проекции камеры на горизонтальную плоскость аватара
        _CameraPlumb = transform.Find("CameraPlumb");

        _HomePos = transform.position;
        _HomeEu = transform.eulerAngles;

        // Основная яхта
        _Yacht = transform.Find("Archimedes");
    }

    // Update is called once per frame
    void Update()
    {

        if (_Flight) // Только перелетаем в заданное положение
        {
            float Interpolant = (Time.time - _StartTime) / _FlightTime;
            transform.localPosition = Vector3.Lerp(_StartPos, _EndPos, Interpolant);
            transform.localRotation = Quaternion.Lerp(Quaternion.Euler(_StartEu), Quaternion.Euler(_EndEu), Interpolant);
            if (Interpolant >= 1)
            {
                transform.localPosition = _EndPos;
                transform.localEulerAngles = _EndEu;
                _Flight = false;
            }
        }
        else // Все остальное управление
        {
            // Команда на перелет домой
            if (Input.GetKeyDown("h"))
            {
                transform.parent = null; // Выйти в корень иерархии сцены
                _StartTime = Time.time;
                _StartPos = transform.position;
                _StartEu = transform.eulerAngles;
                _EndPos = _HomePos;
                _EndEu = _HomeEu;

                _Flight = true;
            }
            // Команда на перелет к башне
            else if (Input.GetKeyDown("b"))
            {
                transform.parent = null; // Выйти в корень иерархии сцены
                _StartTime = Time.time;
                _StartPos = transform.position;
                _StartEu = transform.eulerAngles;
                _EndPos = _BayPos;
                _EndEu = _BayEu;

                _Flight = true;
            }
            // Команда "за штурвал"
            else if (Input.GetKeyDown("y"))
            {
                if (!transform.parent) // Если находимся в корне иерархии сцены
                {
                    // Перейти в дети яхты
                    transform.parent = _Yacht;
                    // Перелететь "на хвост" ближайшего самолета
                    _StartPos = transform.localPosition;
                    _StartEu = transform.localEulerAngles;
                    _StartEu.y = NormalizeAngle(_StartEu.y); // нормализовать курсовой угол в диапазоне +/- 180 градусов
                    _EndPos = _SkipperPos;
                    _EndEu = Vector3.zero;
                    _StartTime = Time.time;

                    _Flight = true;
                }
                else // Вернуться в корень (слезть с хвоста)
                {
                    transform.parent = null; // Выйти в корень иерархии сцены
                }
            }
            // Управление перемещением и поворотом
            else
            {
                float x = 0f;
                float y = 0f;
                float z = 0f;
                float w = 0f;
                float myCurMouseX = 0;

                // Сначала получим сигналы от джойстика
                // Сигналы от осей джойстика
                //x = Input.GetAxis("Horizontal");
                //z = Input.GetAxis("Vertical");
                //y = Input.GetAxis("Throttle");
                //w = Input.GetAxis("Twist");
                //float myCameraPitch = Input.GetAxis("Hat_Vert");

                // Наклонить камеру по тангажу - отработаем сразу. Работает только если VR не активен (проект запущен без маски VR)
                //Vector3 myCamEu = _referenceCamera.transform.localEulerAngles;
                //myCamEu.x = myCamEu.x + myCameraPitch;
                //_referenceCamera.transform.localEulerAngles = myCamEu;

                // Клавиатура и мышь - сигналы суммируются (и с джойстиком). Исключение - нажатая левая кнопка мыши отрабатывается сразу, независимо от других сигналов (то есть, по сути, тоже суммируется)

                // Сначала - перемещение при нажатой левой кнопке мыши
                // Возьмем начальную точку
                if (Input.GetMouseButtonDown(0))
                {
                    _oldMousePos = Input.mousePosition;
                }
                if (Input.GetMouseButton(0))
                {
                    Vector3 myCurMousePos = Input.mousePosition;
                    x = x + (myCurMousePos.x - _oldMousePos.x) * _MouseHorSpeed;
                    z = z + (myCurMousePos.y - _oldMousePos.y) * _MouseHorSpeed;
                }

                // Перемещение по колесику мыши
                y = y + Input.GetAxis("Mouse ScrollWheel") * _MouseVertSpeed;

                // Поворот при нажатой правой кнопке мыши. Установим параметр поворота w
                if (Input.GetMouseButtonDown(1))
                {
                    _OldMouseX = Input.mousePosition.x;
                }
                if (Input.GetMouseButton(1))
                {
                    myCurMouseX = Input.mousePosition.x;
                    if (myCurMouseX > _OldMouseX)
                    {
                        w = w + _MouseYawSpeed;
                    }
                    else if (myCurMouseX < _OldMouseX)
                    {
                        w = w - _MouseYawSpeed;
                    }
                }

                // Теперь получим сигналы от клавиатуры
                if (Input.GetKey("up") || Input.GetKey("w")) z = z + _KeyboardHorSpeed;
                if (Input.GetKey("left") || Input.GetKey("a")) x = x - _KeyboardHorSpeed;
                if (Input.GetKey("down") || Input.GetKey("s")) z = z - _KeyboardHorSpeed;
                if (Input.GetKey("right") || Input.GetKey("d")) x = x + _KeyboardHorSpeed;

                // Если нажат shift, трансформируем сигналы
                if (Input.GetKey("left shift") || Input.GetKey("right shift"))
                {
                    // Если нет сигнала от оси "Throttle", то возьмем высоту от оси z ("Vertical")
                    if (y == 0.0f)
                    {
                        y = z;
                        z = 0.0f;
                    }
                    else // Если есть - усилим сигнал
                    {
                        y = y * 10f;
                    }
                    // Если нет сигнала от оси "Twist" и мыши то возьмем поворот от оси x ("Horizontal")
                    if (w == 0.0f)
                    {
                        w = x;
                        x = 0.0f;
                    }
                }

                // Если был сигнал поворота, поворачиваем
                if (!(w == 0.0f))
                {
                   RotateAvatar(w);
                }
                // Если был сигнал перемещения, перемещаем
                if (x != 0.0f || y != 0.0f || z != 0.0f)
                {
                    //float myHorSpeed = Mathf.Clamp(_horSpeed * transform.localPosition.y / hMin, 10.0f, 1000.0f); // Умножим скорость перемещения на относительную высоту
                    //float myVertSpeed = Mathf.Clamp(_vertSpeed * transform.localPosition.y / hMin * transform.localPosition.y / hMin, _vertSpeed, _vertSpeed * 25.0f); // Умножим вертикальную скорость перемещения на относительную высоту
                    // Переместить по горизонтали
                    transform.Translate(x, 0f, z);
                    // Взять позицию
                    Vector3 myPos = transform.localPosition;
                    // Ограничить новую высоту
                    myPos.y = Mathf.Clamp((myPos.y + y), _HMin, _HMax);
                    //Применить
                    transform.localPosition = myPos;
                }

            }
        }
    }

    // Повернуть опорный объект (Mortar) вокруг камеры (так, чтобы камера вращалась, оставалась на месте)
    // Если просто поворачивать аватара вокруг своей оси, камера едет, как на карусели.
    void RotateAvatar(float w)
    {
        // Возьмем положение головы
        Vector3 myPos = _referenceCamera.transform.position;
        // Возьмем свои углы Эйлера
        Vector3 myEu = transform.eulerAngles;
        // Откорректируем высоту
        myPos.y = transform.position.y;
        // Поставим туда вспомогательный объект
        _CameraPlumb.position = myPos;
        _CameraPlumb.eulerAngles = myEu;
        //print("1) _CameraPlumb.position = " + myPos.ToString("F4") + " _CameraPlumb.eulerAngles = " + myEu.ToString("F4"));
        // Пойдем к нему в дети
        Transform myParent = transform.parent; // но запомним своего родителя
        _CameraPlumb.parent = null;
        transform.parent = _CameraPlumb;
        // Возьмем углы Эйлера
        myEu.y += w; // Повернуть по курсу
        _CameraPlumb.eulerAngles = myEu; // Применить
        // Вернем отцов и детей на место
        _CameraPlumb.parent = transform;
        transform.parent = myParent;
        //print("2) _CameraPlumb.position = " + myPos.ToString("F4") + " _CameraPlumb.eulerAngles = " + myEu.ToString("F4"));
    }

    // Приведем угол от (0/360) к (-180/+180)
    float NormalizeAngle(float myAngle)
    {
        while (myAngle > 180.0f)
        {
            myAngle -= 360.0f;
        }
        while (myAngle < -180.0f)
        {
            myAngle += 360.0f;
        }
        return myAngle;
    }

}

