using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class sDevices : MonoBehaviour
{

    // Компас -----------------------------

    // Компас на главной (правой) стойке
    [SerializeField]
    private Transform _Compass1;

    // Компас на вспомогательной (левой) стойке
    [SerializeField]
    private Transform _Compass2;

    // Коррекция ориентации модели компаса
    [SerializeField]
    private float _CompasNorthCorrection = 180.0f;

    // Фиксированная ориентация компаса
    private Vector3 _CompasFixEu;

    // Флюгер --------------------

    // Модель флюгера
    [SerializeField]
    private Transform _Weathercock;

    // Чашки анемометра
    [SerializeField]
    private Transform _Cups;

    // Класс Wind
    [SerializeField]
    private Wind _WindScript;

    // Коэфициент скорости вращения чашек анемометра
    // Угловая скорость чашек в градусах/сек.: (скорость ветра * коэф. передачи на чашки / радиус чашек) * 180 / ПИ
    // коэф. передачи на чашки = 0.5, радиус чашек = 0.165

    private float _RotationCoef;

    // Спидометр --------------------

    // Дисплей - скорость
    [SerializeField]
    Text _SpeedText;
    //Text _RudderAngleText;                  // Дислей - положение руля
    //Text _TrackAngleText;                   // Дисплей - курсовой угол
    //float _Mile = 1852.0f;                  // Морская миля = 1852 метра
    //float _Knot = 0.5144f;                  // Скорость 1 узел = 0.514... метр/сек.
    float _MeterSecToKnot = 1.944f;           // Скорость 1 метр/сек. = 1,943844492440605 узла

    // Класс YachtSolver
    private YachtSolver _YachtSolver;

    // Класс sAssist - вспомогательные функции и общие параметры
    [SerializeField]
    private sAssist _Assist;


    // Органы управления -----------------------------

    [SerializeField] Transform _HelmWheel;    // Штурвал
    [SerializeField] Transform _HelmWheel2;   // Второй штурвал
    Transform _ThrottleLever;                 // Ручка газ-реверс
    //float _Mile = 1852.0f;                  // Морская миля = 1852 метра
    //float _Knot = 0.5144f;                  // Скорость 1 узел = 0.514... метр/сек.

    // Источник пены
    GameObject _FoamGen;

    // Аудио источники
    AudioSource _EngineConst;
    AudioSource _EngineVar;
    AudioSource _Propeller;
    AudioSource _WindSound;


    // Start is called before the first frame update
    void Start()
    {

        // Органы управления -----------------------------

        //_HelmWheel = GameObject.Find("HelmWheel").transform;                         // Штурвал
        _ThrottleLever = GameObject.Find("ThrottleLever").transform;                 // Ручка газ-реверс
        // Источник пены
        _FoamGen = GameObject.Find("FoamGenAft");
        _FoamGen.SetActive(false);

        // Компас -----------------------------

        // Фиксированная ориентация компаса
        _CompasFixEu = new Vector3(0.0f, _CompasNorthCorrection, 0.0f);

        // Флюгер --------------------
        // Коэфициент скорости вращения чашек анемометра
        // Угловая скорость чашек в градусах/сек.: (скорость ветра * коэф. передачи на чашки / радиус чашек) * 180 / ПИ
        // Коэфициент вращения _RotationCoef = коэф. передачи на чашки / радиус чашек) * 180 / ПИ
        // коэф. передачи на чашки = 0.5, радиус чашек = 0.165
        _RotationCoef = 0.5f / 0.165f * 180f / Mathf.PI;

        // Спидометр --------------------

        // Класс YachtSolver
        _YachtSolver = transform.GetComponent<YachtSolver>();

        //_RudderAngleText = GameObject.Find("RudderAngleText").GetComponent<Text>();  // Дислей - положение руля
        //_TrackAngleText = GameObject.Find("TrackAngleText").GetComponent<Text>();    // Дисплей - курсовой угол

        // Аудио источники
        _EngineConst = GameObject.Find("EngineConstant").GetComponent<AudioSource>();
        _EngineVar = GameObject.Find("EngineVariable").GetComponent<AudioSource>();
        _Propeller = GameObject.Find("Propeller").GetComponent<AudioSource>();
        _WindSound = GameObject.Find("Wind").GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void LateUpdate()
    {

        // Органы управления -----------------------------

        // Ручка газ-реверс
        Vector3 myVect = _ThrottleLever.localEulerAngles;
        myVect.x = Mathf.Lerp(-50, 50, (_YachtSolver.engineValue + 1) / 2.0f);
        _ThrottleLever.localEulerAngles = myVect;

        // Штурвал
        myVect = _HelmWheel.localEulerAngles;
        myVect.z = _YachtSolver.steeringWheel + 30;
        //myVect.z = - Mathf.Lerp(-540, 540, (steeringWheel + 35) / 70.0f);
        _HelmWheel.localEulerAngles = _HelmWheel2.localEulerAngles = myVect;

        // Изменить высоту звука двигателя, выключить/включить шум воды и пену 

        float EV = Mathf.Abs(_YachtSolver.engineValue);
        _EngineVar.pitch = EV * 1.5f + 1; // pitch меняется от 1 до 2.5
        if (_Propeller.isPlaying) // Была включена передача
        {
            if (EV == 0.0f) // А теперь включен холостой ход
            {
                _FoamGen.SetActive(false); // Пена - выключить
                _Propeller.Stop(); // Шум воды - выключить
            }
            else // По-прежнему включена передача
            {
                _Propeller.pitch = EV * 2 + 1; // pitch меняется от 1 до 3
                _Propeller.volume = EV;
            }
        }
        else // Передача не была включена
        {
            if (EV != 0.0f) // А теперь передача включена
            {
                _FoamGen.SetActive(true);
                _Propeller.Play();
            }
        }

        // Компас -----------------------------

        // Поддержание фиксированной ориентации компасов
        _Compass1.eulerAngles = _CompasFixEu;
        _Compass2.eulerAngles = _CompasFixEu;

        // Флюгер и анемометр --------------------

        // Держать флюгер по ветру, крутить чашки анемометра
        float WindSpeed = _WindScript.WindDir[0].value;
        // Если ветер дует
        if (WindSpeed > 0)
        {
            // Флюгер
            Vector3 myEu = _Weathercock.eulerAngles;
            myEu.y = _WindScript.WindDir[0].angle + 180;
            _Weathercock.eulerAngles = myEu;
            // Анемометр
            myEu = _Cups.localEulerAngles;
            // Угловая скорость чашек в градусах/сек.: (скорость ветра * коэф. передачи на чашки / радиус чашек) * 180 / ПИ
            // коэф. передачи на чашки = 0.5, радиус чашек = 0.165
            // Поворот за один кадр:
            float Delta = WindSpeed * _RotationCoef * Time.deltaTime;
            myEu.y = _Assist.NormalizeAngle(myEu.y - Delta);
            _Cups.localEulerAngles = myEu;
            // Ветер
            if (WindSpeed >= 5) // Дует сильно
            {
                if (!_WindSound.isPlaying) // Звука нет
                {
                    _WindSound.Play(); // Включить звук
                }
                // Громкость (меняется от 0.25 до 1 на интервале скорости от 5 до 20 м/сек)
                _WindSound.volume = Mathf.Clamp(WindSpeed, 5f, 20f) / 20f;
                // Высота звука (меняется от 1 до 2 на интервале скорости от 10 до 20 м/сек)
                _WindSound.pitch = Mathf.Clamp(WindSpeed, 10f, 20f) / 10f;
            }
            else // Дует слабо
            {
                if (_WindSound.isPlaying) // Звук есть
                {
                    print("Звук ветра выключаем");
                    _WindSound.Stop(); // Выключить звук
                }
            }
        }
        else // Вообще не дует
        {
            if (_WindSound.isPlaying) // Звук есть
            {
                print("Звук ветра выключаем");
                _WindSound.Stop(); // Выключить звук
            }
        }

        // Спидометр --------------------
        // Вывести данные на дисплей
        _SpeedText.text = (_YachtSolver.Vz * _MeterSecToKnot).ToString("F2", CultureInfo.InvariantCulture);
        //_RudderAngleText.text = RuderValue.ToString("F2", CultureInfo.InvariantCulture);
        //_TrackAngleText.text = NormalizeAngle(transform.localEulerAngles.y).ToString("F0", CultureInfo.InvariantCulture);

    }
}
