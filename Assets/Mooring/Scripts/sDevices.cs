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

    // Класс Wind
    [SerializeField]
    private Wind _WindScript;

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

    // Start is called before the first frame update
    void Start()
    {
        // Компас -----------------------------

        // Фиксированная ориентация компаса
        _CompasFixEu = new Vector3(0.0f, _CompasNorthCorrection, 0.0f);

        // Флюгер --------------------

        // Спидометр --------------------

        // Класс YachtSolver
        _YachtSolver = transform.GetComponent<YachtSolver>();

        //_RudderAngleText = GameObject.Find("RudderAngleText").GetComponent<Text>();  // Дислей - положение руля
        //_TrackAngleText = GameObject.Find("TrackAngleText").GetComponent<Text>();    // Дисплей - курсовой угол

    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Компас -----------------------------

        // Поддержание фиксированной ориентации компасов
        _Compass1.eulerAngles = _CompasFixEu;
        _Compass2.eulerAngles = _CompasFixEu;

        // Флюгер --------------------

        // Держать флюгер по ветру
        if (_WindScript.WindDir[0].value > 0)
        {
            Vector3 myEu = _Weathercock.eulerAngles;
            myEu.y = _WindScript.WindDir[0].angle;
            _Weathercock.eulerAngles = myEu;
        }

        // Спидометр --------------------
        // Вывести данные на дисплей
        _SpeedText.text = (_YachtSolver.Vz * _MeterSecToKnot).ToString("F2", CultureInfo.InvariantCulture);
        //_RudderAngleText.text = RuderValue.ToString("F2", CultureInfo.InvariantCulture);
        //_TrackAngleText.text = NormalizeAngle(transform.localEulerAngles.y).ToString("F0", CultureInfo.InvariantCulture);

    }
}
