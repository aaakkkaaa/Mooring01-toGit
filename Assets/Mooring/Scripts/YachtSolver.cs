using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;
using System;

// Движение МПО в гор плоск (1)
// Лукомский Чугунов Системы управления МПО (2)
// Гофман Маневрирование судна (3)

public class YachtSolver : MonoBehaviour
{
    [Header("Управление:")]
    [Range(-1.0f, 1.0f)]
    public float engineValue = 0;           // -1..+1, для получения текущей мощности умножается на enginePower
    [Range(-540.0f, 540.0f)]
    public float steeringWheel = 0;         // угол поворота штурвала

    public bool GameWheel = false;        // управление от игрового руля

    [SerializeField]
    sRecord _Record;                      // Класс для вывода в файл
    [SerializeField]
    sTime _Time;                          // Класс точного времени
    long _nextLogTime = 0;                // Время следующей записи в файл

    [Header("Разные исходные данные:")]
    private float K_EnvalToFeng = 3600f;    // Коэф. для перевода engineValue в силу тяги Feng (еще учесть КПД винта)
    private float enginePower = 30000f;      // 40 л.с примерно 30 КВт
//    private float engBack = 0.75f;            // назад эффективность винта ниже
    private float engBack = 0.6f;            // назад эффективность винта ниже
    private float maxV = 4.1f;               // 4.1 м/с = 8 узлов
//    private float Ca = 0.97f;                // адмиралтейский коэффициент после перевода рассчетов в Си
    private float Ca = 16f;                // адмиралтейский коэффициент после перевода рассчетов в Си
    private float Lbody = 11.99f;            // длинна корпуса
//    private float M0 = 8680f;                // водоизмещение = вес яхты в кг
    private float M0 = 12000f;                // водоизмещение = вес яхты в кг
    private float Jy = 35000f;               // момент инерции относительно вертикальной оси
    private float K11 = 0.3f;                // коеф. для расчета массы с учетом присоединенной Mzz = (1+K11)*M0
    private float K66 = 0.5f;                // коеф. для расчета массы и момента с учетом прис.  Jyy = (1+K66)*Jy; Mxx = (1+K66)*M0
    private float KFrudX = 1.1f;             // Подгонка, для реализма эффективности руля
    private float KBetaForv = 0.7f;          // Чтобы уменьшить влияние Beta, так как руль обдувается водой винта (3 стр 55)
    private float KBetaBack = 1.0f;          // Чтобы уменьшить влияние Beta, так как руль обдувается водой винта (3 стр 55)
    private float KrudVzxContraEnx = 0.7f;   // Соотношение влияния руля в потоке воды и руля в потоке винта
    private float KwindF2 = 1.0f;             // Для подстройки влияния ветра на силу - множитель при V*V
    private float KwindF1 = 10.0f;            // Для подстройки влияния ветра на силу - множитель при V
    private float KwindM = 1.0f;             // Для подстройки влияния ветра на момент

    // занос кормы
    public int DirectV = 1;                 // направление вращения, +1 - правый винт, -1 - левый;
    public float Kzanos = 4;                // подбором, влияет на силу заноса кормы
    public float Tzanos = 1.3f;             // подбором, влияет на время (обратно) действия силы заноса после увеличения мощности 

    // рассчитывается один раз
    private float _KresZ2;                  // коэффициент перед V*V при рассчете силы сопротивления по Z
    private float _KresZ1;                  // коэффициент перед V при рассчете силы сопротивления по Z
    private float _KresX2;                  // коэффициент перед V*V при рассчете силы сопротивления по X
    private float _KresX1;                  // коэффициент перед V при рассчете силы сопротивления по X
    private float _KresOmega1;              // коэффициент перед силой сопротивления воды вращению линейный по омега
    private float _KresOmega2;              // коэффициент перед силой сопротивления воды вращению квадратичный по омега
    private float _Mzz;                     // Масса с учетом присоединенной
    private float _Mxx;                     // Масса с учетом присоединенной
    private float _Jyy;                     // Момент с учетом присоединенной массы

    // рассчитывается на каждом шаге
    private float _Kprop1 = -0.0603f;                  // коэффициент перед V при рассчете КПД винта. Линейная зависимость от engineValue
    private float _Kprop2 = 0.0438f;                  // коэффициент перед V*V при рассчете КПД винта. Квадратичная зависимость от engineValue
    private float _Kprop3 = -0.01f;
    //private float _Kprop4 = -0.0007f; // 2200
    private float _Kprop4 = -0.001f; // 2200

    [Header("Вывод для контроля:")]
    public float Feng;                      // сила тяги, будет получатся из engineValue умножением на коэфициент K_EnvalToFeng и КПД винта (_Kprop1, _Kprop2)
    public float RuderValue;                // угол поворота пера руля
    public float FresZ;                     // сила сопротивления корпуса продольная
    public float FresX;                     // сила сопротивления корпуса поперечная

    public float FrudVzZ;                   // сила сопротивления руля от движения яхты относительно воды
    public float FrudVzX;                   // боковая сила на руле от движения яхты относительно воды
    public float MrudVzX;                   // Момент возникающий от силы FrudVzX на руле
    public float FrudEnZ;                   // доп. сила (от винта) из-за поворота руля 
    public float FrudEnX;                   // боковая сила (от движения яхты) из-за поворота руля 
    public float MrudEnX;                   // Момент возникающий от силы FrudEnX на руле
    public float MrudResZ;                  // Момент на руле от сопротивления воды, когда есть Beta

    public float MresBody;                  // Момент сил сопротивления воды
    public float FengX;                     // боковая сила от винта, возникает при изменениях мощности
    public float MengX;                     // Момент от винта

    public float FwindX;                    // сила ветра по X
    public float FwindZ;                    // сила ветра по Z
    public float Mwind;                     // момент от ветра

    public float FropeX;                    // сила натяжения по X
    public float FropeZ;                    // сила натяжения по Z
    public float Mrope;                     // момент от натяжения 


    public float Vz = 0;                    // текущая продольная скорость
    public float Vx = 0;                    // боковая скорость
    public float OmegaY = 0;                // скорость поворота вокруг вертикальной оси
    public float Beta = 0;                  // угол между локальной осью OZ и скоростью

    // Ветер
    private Wind _wind;

    // канаты
    private Cleat[] _cleats;

    // Группа величин для обработки сигнала от ручки газа
    [HideInInspector]
    public float ThrottleSignal;
    private float _middleThrottle = 0.46f;
    private float _ZeroThrottle = 0.05f;
    private float _PositiveMultiplier;
    private float _NegativeMultiplier;
    private float _ForwardGear = 0.67f; // Сигнал на ручке при включении переднего хода
    private float _BackwardGear = -0.67f; // Сигнал на ручке при включении заднего хода
    private float _ForwardGearEV = 0.2f; // Мощность двигателя при включении переднего хода
    private float _BackwardGearEV = -0.2f; // Мощность двигателя при включении переднего хода
    private bool _ThrottleCalibrationMode = false;
    private int _ThrottleCalibrationStep = 0;

    private void Awake()
    {
        GameObject windField = GameObject.Find("WindField");
        _wind = windField.GetComponent<Wind>();

        _cleats = GameObject.FindObjectsOfType<Cleat>();

    }

    void Start()
    {
        // рассчет коэффициента перед силой сопротивления в ур. динамики (1-8)
        _KresZ2 = Mathf.Pow(M0, 2.0f / 3.0f) / Ca;
//        _KresZ1 = _KresZ2 * 3.0f;             // подбором 
        _KresZ1 = _KresZ2 * 30.0f;             // подбором 

        //_KresZ1 = 248f;
        //_KresZ2 = 81f;
        _KresZ1 = 200f;
        _KresZ2 = 35f;
        print("_KresZ1 = " + _KresZ1 + " _KresZ2 = " + _KresZ2);

        _KresX2 = _KresZ2 * 20;               // подбором
        _KresX1 = _KresX2 * 3.0f;             // подбором 
        _KresOmega2 = _KresZ2 * 30 * 10;      // подбором
        _KresOmega1 = _KresZ2 * 16;           // подбором
        // массы и момент инерции с учетом присоединенных масс
        _Mzz = (1 + K11) * M0;
        _Mxx = (1 + K66) * M0;
        _Jyy = (1 + K66) * Jy;


        // Группа величин для обработки сигнала от ручки газа
        // При запуске программы должна стоять в среднем положении
        float middleThrottle = Input.GetAxis("VerticalJoy");
        print("Положение ручки в центре (middleThrottle) = " + middleThrottle);
        if (middleThrottle < 0.4f || middleThrottle > 0.6f)
        {
            print("Значение middleThrottle вне допустимого диапазона. Неправильная калибровка при включении рулевой системы или при запуске программы ручка управления газом не находится в центральном положении. Устаноалено значение по умолчанию, но работа системы не гарантируется");
        }
        else
        {
            _middleThrottle = middleThrottle;
        }
        print("Окончательно установлено положение ручки в центре _middleThrottle = " + _middleThrottle);
        _PositiveMultiplier = 1.0f / (1.0f - _middleThrottle);
        _NegativeMultiplier = 1.0f / _middleThrottle;

        _Record.MyLog("K_EnvalToFeng:\t" + K_EnvalToFeng + "\tengBack:\t" + engBack + "\tLbody:\t" + Lbody + "\tM0:\t" + M0 + "\tJy:\t" + Jy + 
            "\tK11:\t" + K11 + "\tK66:\t" + K66 + "\tKFrudX:\t" + KFrudX + "\tKBetaForv:\t" + KBetaForv + "\tKBetaBack:\t" + KBetaBack + "\tKrudVzxContraEnx:\t" + KrudVzxContraEnx);
        _Record.MyLog("DirectV:\t" + DirectV + "\tKzanos:\t" + Kzanos + "\tTzanos:\t" + Tzanos);
        _Record.MyLog("_KresZ1:\t" + _KresZ1 + "\t_KresZ2:\t" + _KresZ2 + "\t_KresX1:\t" + _KresX1 + "\t_KresX2:\t" + _KresX2 + "\t_KresOmega1:\t" + _KresOmega1 + 
            "\t_KresOmega2:\t" + _KresOmega2 + "\t_Mzz:\t" + _Mzz + "\t_Mxx:\t" + _Mxx + "\t_Jyy:\t" + _Jyy);
        _Record.MyLog("\t_Kprop1:\t" + _Kprop1 + "\t_Kprop2:\t" + _Kprop2 + "\t_Kprop3:\t" + _Kprop3 + "\t_Kprop4:\t" + _Kprop4 + "\n");

        _Record.MyLog("EngineV:\tKprop4:\tFeng\tRuderValue\tFresZ\tFresX\tFrudVzZ\tFrudVzX\tMrudVzX\tFrudEnZ\tFrudEnX\tMrudEnX\tMrudResZ\tMresBody\tFengX\tMengX\tFwindZ\tFwindX\tMwind\tdVz\tVz\tdVx\tVx\tdOmegaY\tOmegaY\tBeta\tdt");

    }

    private void Update()
    {

        // Режим калибровки ручки газ-реверс

        if (_ThrottleCalibrationMode)
        {
            ThrottleSignal = ThrottleValue();
            if (Input.GetKeyDown("q"))
            {
                switch (_ThrottleCalibrationStep)
                {
                    case 0:
                        print("Полный назад: " + ThrottleSignal);
                        _ThrottleCalibrationStep = 1;
                        break;
                    case 1:
                        print("Малый назад: " + ThrottleSignal);
                        _BackwardGear = ThrottleSignal + _ZeroThrottle;
                        _ThrottleCalibrationStep = 2;
                        break;
                    case 2:
                        print("Нейтраль: " + ThrottleSignal);
                        _ThrottleCalibrationStep = 3;
                        break;
                    case 3:
                        print("Малый вперед: " + ThrottleSignal);
                        _ForwardGear = ThrottleSignal - _ZeroThrottle;
                        _ThrottleCalibrationStep = 4;
                        break;
                    case 4:
                        print("Полный вперед: " + ThrottleSignal);
                        _ThrottleCalibrationStep = 5;
                        break;
                    case 5:
                        print("Калибровка ручки газ-реверс завершена.");
                        _ThrottleCalibrationMode = false;
                        _ThrottleCalibrationStep = 0;
                        break;
                }
            }
            return;
        }

        // Control + z : включить режим калибровки ручки газ-реверс
        if (Input.GetKeyDown("q"))
        {
            if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
            {
                print("Включен режим калибровки ручки газ-реверс. Устанавливайте ручку в стационарные положения от ПОЛНЫЙ НАЗАД до ПОЛНЫЙ ВПЕРЕД и нажимайете клавишу q.");
                _ThrottleCalibrationMode = true;
                _ThrottleCalibrationStep = 0;
            }
        }

        // получить управление с клавиатуры
        if (Input.GetKeyDown("up"))
        engineValue += 0.1f;
        else if (Input.GetKeyDown("down"))
            engineValue -= 0.1f;
        if (Input.GetKey("right"))
            steeringWheel += 1.0f;
        else if (Input.GetKey("left"))
            steeringWheel -= 1.0f;

        if (GameWheel) // получить управление от руля
        {
            // Положение штурвала
            steeringWheel = Mathf.Lerp(-540.0f, 540.0f, (Input.GetAxis("HorizontalJoy") + 1.0f) / 2.0f);

            // Положение ручки газа. Приводится из диапазона 0/1 в диапазон -1/1
            ThrottleSignal = ThrottleValue();
            ThrottleSignal = Mathf.Clamp(ThrottleSignal, -1.0f, 1.0f);
            if (ThrottleSignal >= _ForwardGear)
                engineValue = _ForwardGearEV + (ThrottleSignal - _ForwardGear) / (1.0f - _ForwardGear) * (1.0f - _ForwardGearEV);
            else if (ThrottleSignal <= _BackwardGear)
                engineValue = _BackwardGearEV + (ThrottleSignal - _BackwardGear) / (-1.0f - _BackwardGear) * (-1.0f - _BackwardGearEV);
            else
                engineValue = 0;
        }

        engineValue = Mathf.Clamp(engineValue, -1.0f, 1.0f);
        steeringWheel = Mathf.Clamp(steeringWheel, -540.0f, 540.0f);

    }

    // Положение ручки газа. Приводится из диапазона 0/1 в диапазон -1/1
    float ThrottleValue()
    {
        float TS = Input.GetAxis("VerticalJoy") - _middleThrottle;

        // Малые значения обнуляем
        if (Mathf.Abs(TS) < _ZeroThrottle)
        {
            TS = 0.0f;
        }
        // Положительное значение
        if (TS >= 0.0f)
        {
            TS = TS * _PositiveMultiplier;
        }
        // Отрицательное значение
        else
        {
            TS = TS * _NegativeMultiplier;
        }

        return TS;
    }


    void FixedUpdate()
    {

        float dt = Time.fixedDeltaTime;
        // поворот пера руля
        RuderValue = -steeringWheel * 35 / 540;
        // сила тяги
        float FengOld = Feng; // для анализа, нужен ли занос кормы от работы винта
        //Feng = enginePower * engineValue / maxV; 
        //Feng = enginePower * engineValue / maxV * (0.1f + 0.8f * (Mathf.Abs(Vz / maxV)) + 0.8f * (Mathf.Abs(Vz * Vz / maxV / maxV)));
        //Feng = enginePower * engineValue / maxV * ( 0.2f + 0.75f * (Mathf.Abs(Vz / maxV)) + 0.8f * (Mathf.Abs(Vz * Vz / maxV / maxV)) );
        // Feng = enginePower * engineValue / maxV * (0.27f + 1.43f * (Mathf.Abs(Vz / maxV)) );
        //Feng = enginePower * engineValue / maxV * (0.15f + 0.38f * (Mathf.Abs(Vz / maxV)) - 0.24f * (Mathf.Abs(Vz * Vz / maxV / maxV)));
        //Feng = enginePower * engineValue / maxV * 0.21f * (0.1045f + 0.0876f * (Mathf.Abs(Vz)) + 0.114f * Vz * Vz  - 0.02f * (Mathf.Abs(Vz * Vz * Vz )));

        // коэффициенты при рассчете КПД винта. _Kprop1, _Kprop2, _Kprop3 - константы, определены при инициализации, _Kprop4 вычисляется
        //y = -0,6223x4 + 1,9605x3 - 2,3006x2 + 1,2025x - 0,2408
        _Kprop4 = -0.2408f + 1.2025f * Mathf.Abs(engineValue) - 2.3006f * engineValue * engineValue + 1.9605f * Mathf.Abs(engineValue) * engineValue * engineValue - 0.6223f * engineValue * engineValue * engineValue * engineValue;


        // Сила тяги винта
        Feng = engineValue * K_EnvalToFeng * (1 + _Kprop1*Mathf.Abs(Vz) + _Kprop2 * Vz * Vz + _Kprop3 * Vz * Vz * Mathf.Abs(Vz) + _Kprop4 * Vz * Vz * Vz * Vz);
        if (Feng<0)
        {
            Feng *= engBack;
        }
        //_Record.MyLog("engineValue\t" + engineValue + "\tK_EnvalToFeng\t" + K_EnvalToFeng + "\tVz\t" + Vz + "\t_Kprop1\t" + _Kprop1 + "\t_Kprop2\t" + _Kprop2 + "\t_Kprop3\t" + _Kprop3 + "\t_Kprop4\t" + _Kprop4 + "\tFeng\t" + Feng);

        // сила сопротивления корпуса
        FresZ = -Mathf.Sign(Vz) * _KresZ2 * Vz * Vz - _KresZ1 * Vz;
        FresX = -Mathf.Sign(Vx) * _KresX2 * Vx * Vx - _KresX1 * Vx;
        //print("Квадрат: " + (-Mathf.Sign(Vz) * _KresZ2 * Vz * Vz) + "   Линейное: " + (-_KresZ1 * Vz) );

        //_Record.MyLog(_Time.CurrentTimeSec() + "\t" + engineValue + "\t" + Vz + "\t" + Feng + "\t" + FresZ, false);

        // силы и момент на руле от движения яхты
        FrudVzZ = -Mathf.Sign(Vz) * FruderZ(RuderValue - Beta, Vz);
        FrudVzX = Mathf.Sign(RuderValue - Beta) * Mathf.Sign(Vz) * FruderX(RuderValue - Beta, Vz);
        if(Feng >0)
        {
            FrudVzX *= KrudVzxContraEnx;    // доля влияния руля в потоке воды
        }
        MrudVzX = -FrudVzX * Lbody / 2;
        // print("RuderValue = " + RuderValue + "   Beta = " + Beta + "   Итого = " + (RuderValue - Beta));

        // силы и момент на руле от работы винта - возникают НЕ только при кручении винта вперед
        
        if (Feng > 0)
        {
            float VeffRud = Mathf.Sqrt(Feng / 440);
            FrudEnZ = -FruderZ(RuderValue, VeffRud);
            FrudEnX = Mathf.Sign(RuderValue) * FruderX(RuderValue, VeffRud);

            //FrudVzX *= (1 - KrudVzxContraEnx);    // доля влияния руля в потоке винта
            FrudEnX *= (1 - KrudVzxContraEnx);    // доля влияния руля в потоке винта

            MrudEnX = -FrudEnX * Lbody / 2;
        }
        else
        {
            //FrudEnZ = FrudEnX = MrudEnX = 0.0f;

            float VeffRud = Mathf.Sqrt(-Feng / 440);
            FrudEnX = -Mathf.Sign(RuderValue) * FruderX(RuderValue, VeffRud);
            FrudEnX *= (1 - KrudVzxContraEnx);
            MrudEnX = -FrudEnX * Lbody / 2;
            FrudEnZ = 0.0f;
        }

        // Момент на руле из-за сопротивления руля воде при наличии угла Beta
        if (Vz > 0)
        {
            // сделал только при движении вперед, чтобы не попадать в штопор на заднем ходу
            MrudResZ = -(FrudVzZ + FrudEnZ) * Mathf.Sin(Beta * Mathf.PI / 180) * Lbody / 2;
            //print(MrudResZ);
        }
        else
        {
            MrudResZ = 0;
        }

        // Момент - сопротивление вращательному движению. Не по (1), а из физических соображений
        //MresBody = -Mathf.Sign(OmegaY) * _KresOmega2 * (OmegaY * Lbody) * (OmegaY * Lbody) / 8;
        MresBody = -Mathf.Sign(OmegaY) * _KresOmega2 * (OmegaY * Lbody) * (OmegaY * Lbody) / 8 - _KresOmega1 * (OmegaY * Lbody) / 2;
        // сила и момент от увеличения мощности двигателя
        float impactX = detectEngineImpact(FengOld, Feng);
        float dFengX = dt * (Kzanos * impactX - Tzanos * FengX);    // спадающая экспонента

        // dFengX = dt * (Kzanos * Kzanos * (Feng - FengOld) - Tzanos * FengX)

        FengX += dFengX;
        MengX = -FengX * Lbody / 2;
        //_Record.MyLog("FengOld\t" + FengOld + "\tFeng\t" + Feng + "\timpactX\t" + impactX + "\tKzanos\t" + Kzanos + "\tTzanos\t" + Tzanos + "\tdFengX\t" + dFengX + "\tFengX\t" + FengX + "\tMengX\t" + MengX);

        // Влияние ветра:
        // в глобальной системе
        float wZ = _wind.WindDir[0].value * Mathf.Cos(_wind.WindDir[0].angle * Mathf.PI / 180);
        float wX = _wind.WindDir[0].value * Mathf.Sin(_wind.WindDir[0].angle * Mathf.PI / 180);
        //print("*************************");
        //print("wZ = " + wZ + "   wX = " + wX);
        Vector3 wGlobal = new Vector3(wX, 0, wZ);
        // в локальной системе
        Vector3 wLocal = transform.InverseTransformVector(wGlobal);
        wLocal.x -= Vx;
        wLocal.z -= Vz;
        float wAngle = Vector3.Angle(Vector3.forward, wLocal) * Mathf.Sign(wLocal.x);  // направление отсчитываем от носа
        wAngle = NormalizeAngle(wAngle);
        //print("Локально: " + wLocal + "   wAngle = " + wAngle);
        float wValue = wLocal.magnitude;
        //_Record.MyLog("wLocal.x\t" + wLocal.x + "\twLocal.z\t" + wLocal.z + "\twAngle\t" + wAngle + "\twValue\t" + wValue);
        // получаем величину силы и момент от ветра
        float fWind = WindForce(wAngle, wValue);           // возвращается абс. величина силы!

        //print("fWind = " + fWind);
        Vector3 FwindVec = Vector3.Normalize(wLocal) * fWind;
        FwindX = FwindVec.x;
        FwindZ = FwindVec.z;
        Mwind = WindMoment(wAngle, wValue);

        // Натяжение канатов
        FropeX = 0;
        FropeZ = 0;
        Mrope = 0;
        for (int i = 0; i < _cleats.Length; i++)
        {
            Vector3 localF = transform.InverseTransformDirection(_cleats[i].getForce());
            FropeX += localF.x;
            FropeZ += localF.z;
            Mrope += localF.x * (_cleats[i].transform.localPosition.z);
            Mrope += -localF.z * (_cleats[i].transform.localPosition.x);
        }

        // Численное интегрирование:
        // Интегрируем dVz/dt - продольная скорость по Z
        //float rotToVz = _Mxx * OmegaY * Vx;
        float rotToVz = _Mxx * OmegaY * Vx * 0.2f;
        float dVz = dt * (Feng + FresZ + FrudVzZ + FrudEnZ + FwindZ + FropeZ + rotToVz) / _Mzz;
        //_Record.MyLog("\t_Mxx\t" + _Mxx + "\tOmegaY\t" + OmegaY + "\tVx\t" + Vx + "\trotToVz\t" + rotToVz + "\tFeng\t" + Feng + "\tFresZ\t" + FresZ + "\tFrudVzZ\t" + FrudVzZ + "\tFrudEnZ\t" + FrudEnZ + "\trotToVz\t" + rotToVz);
/*
        if (_Time.CurrentTimeMilliSec() > _nextLogTime - 20)
        {
            _Record.MyLog(_Time.CurrentTimeSec() + "\t" + Vz + "\t" + Feng + "\t" + FresZ + "\t" + FrudVzZ + "\t" + FrudEnZ + "\t" + FwindZ + "\t" + FropeZ + "\t" + rotToVz + "\t" + _Mzz + "\t" + dt + "\t" + dVz, false);
            _nextLogTime += 1000;
        }
*/
        // Интегрируем dVx/dt - боковая скорость по X
        //        float rotToVx = -_Mzz * OmegaY * Vz;
        float rotToVx = -_Mzz * OmegaY * Vz * 0.2f;
        float dVx = dt * (FrudVzX + FrudEnX + FresX + FengX + FwindX + FropeX + rotToVx) / _Mxx;
        //_Record.MyLog("\tFrudVzX\t" + FrudVzX + "\tFrudEnX\t" + FrudEnX + "\tFresX\t" + FresX + "\tFengX\t" + FengX + "\tFwindX\t" + FwindX + 
        //    "\trotToVx\t" + rotToVx + "\t\tSUM\t" + (FrudVzX + FrudEnX + FresX + FengX + FwindX + FropeX + rotToVx) + "\tdt\t" + dt +
        //    "\t_Mxx\t" + _Mxx + "\tdVx\t" + dVx);
        //print("FrudX = " + FrudX + "   FresX = " + FresX + "   rotToVy = " + rotToVx + "   dVx = "+ dVx + "   Vx = "+Vx);

        // Интегрируем dOmegaY/dt - момент вокруг вертикальной оси Y
        float vToRot = (_Mzz - _Mxx) * Vx * Vz;

        float dOmegaY = dt * (MrudVzX + MrudEnX + MresBody + MengX + MrudResZ + Mwind + Mrope + vToRot) / _Jyy;
/*
        if (_Time.CurrentTimeMilliSec() > _nextLogTime - 20)
        {
//            _Record.MyLog(_Time.CurrentTimeSec() + "\t" + RuderValue + "\t" + Beta + "\t" + MrudVzX + "\t" + MrudEnX + "\t" + MresBody + "\t" + MengX + "\t" + MrudResZ + "\t" + vToRot + "\t" + dOmegaY + "\t" + OmegaY, false);
            _Record.MyLog(_Kprop4 + "\t" + Feng + "\t" + RuderValue + "\t" + FresZ + "\t" + FresX + "\t" + FrudVzZ + "\t" + FrudVzX + "\t" + MrudVzX + "\t" + FrudEnZ + "\t" + FrudEnX + "\t" + MrudEnX + "\t" + MrudResZ + "\t" + MresBody + "\t" + FengX + "\t" + MengX + "\t" + Vz + "\t" + Vx + "\t" + OmegaY + "\t" + Beta + "\t" + dt);
            _nextLogTime += 1000;
         }
*/
        _Record.MyLog(engineValue + "\t" + _Kprop4 + "\t" + Feng + "\t" + RuderValue + "\t" + FresZ + "\t" + FresX + "\t" + FrudVzZ + "\t" + FrudVzX + "\t" + MrudVzX + "\t" + FrudEnZ + "\t" + FrudEnX + "\t" + MrudEnX + "\t" + MrudResZ + "\t" + MresBody + "\t" + FengX + "\t" + MengX + "\t" + FwindZ + "\t" + FwindX + "\t" + Mwind + "\t" + dVz + "\t" + Vz + "\t" + dVx + "\t" + Vx + "\t" + dOmegaY + "\t" + OmegaY + "\t" + Beta + "\t" + dt);

        // Новые скорости
        Vz += dVz;
        Vx += dVx;
        OmegaY += dOmegaY;

        // Рассчет и изменение положения
        Vector3 localV = new Vector3(Vx, 0, Vz);
        Vector3 globalV = transform.TransformVector(localV);
        Vector3 curPos = gameObject.transform.position;
        curPos += globalV * dt;
        gameObject.transform.position = curPos;

        // Рассчет и изменение угла
        Vector3 rot = transform.eulerAngles;
        rot.y += OmegaY * 180 / Mathf.PI * dt;
        transform.eulerAngles = rot;


        // определение угла Beta между продольной осью и скоростью
        if (Vz == 0.0f && Vx == 0.0f)
        {
            Beta = 0;
        }
        else if (Vz == 0.0f && Vx != 0.0f)
        {
            Beta = 90 * Mathf.Sign(Vx);
        }
        else
        {
            Beta = Mathf.Atan(Vx / Vz) * 180 / Mathf.PI;
        }

        // коррекция для реалистичности влияния руля
        if (Vz > 0)
        {
            Beta *= KBetaForv;
        }
        else
        {
            Beta *= KBetaBack;
        }
    }

    // сила сопротивления руля по оси Z
    private float FruderZ(float ang, float V)
    {
        float VV = 2.37f;           // если скорость 1,54 то V*V = 2,37
        float Fv3;                  // сила при скорости 3 узла

        ang = Mathf.Abs(ang);
        Fv3 = 12.3f + ang * (-0.311f + ang * (0.103f + ang * 0.011f));
        return Fv3 / VV * V * V;
    }

    // сила сопротивления руля по оси X, порождает боковую скорость и момент вращения
    private float FruderX(float ang, float V)
    {
        float VV = 2.37f;           // если скорость 1,54 то V*V = 2,37
        float Fv3;                  // сила при скорости 3 узла

        ang = Mathf.Abs(ang);
        Fv3 = ang * (59.88f + ang * (-2.57f + ang * 0.029f));
        return Fv3 / VV * V * V * KFrudX; // KFrudX - подгонка, чтобы уменьшить эффективность руля
    }

    // боковая сила приложеная к корме из-за увеличения мощности двигателя
    // TODO! не все ситуации рассмотрены!
    private float detectEngineImpact(float FengOld, float Feng)
    {
        float impact = 0;
        if (FengOld >= 0 && FengOld < Feng)     // мощность увеличили
        {
            if (Vz >= 0)                        // стоим или плывем вперед
            {
                impact = Kzanos * (Feng - FengOld);
            }
        }
        if (FengOld <= 0 && FengOld > Feng)     // увеличили мощность назад
        {
            if (Vz <= 0)                        // стоим или движемся назад
            {
                impact = Kzanos * (Feng - FengOld);
            }
        }
        // учтем направление вращения винта
        return impact * DirectV;
    }

    // Приведем любой угол от к (-180/+180)
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

    // Получение силы в зависимости от угла и скорости ветра
    private float WindForce(float alfa, float v)
    {
        float x = Mathf.Abs(alfa);

        float windf = Mathf.Sin(Mathf.PI * x / 180) * 4 + 2.0f + 4*(180-x)/180;
        float f = windf * ( v * KwindF1 + v * v * KwindF2 );

        //_Record.MyLog("\talfa\t" + alfa + "\tx\t" + x + "\tv\t" + v + "\twindf\t" + windf + "\tWindForce\t" + f);
        //print("Сила ветра = " + f);
        return f;
    }

    // Получение момента в зависимости от угла и скорости ветра
    private float WindMoment(float alfa, float v)
    {
        float x = Mathf.Abs(alfa);
        float windm = Mathf.Sin(Mathf.PI * x / 180) * 4;
        //_Record.MyLog("\talfa\t" + alfa + "\tx\t" + x + "\tv\t" + v + "\twindm\t" + windm + "\tsin(Pi)*4\t" + (Mathf.Sin(Mathf.PI) * 4) + "\tWindMoment\t" + (windm * v * v * KwindM * Mathf.Sign(alfa)));
        return windm * v * v * KwindM * Mathf.Sign(alfa);
    }


}
