using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using RootMotion;
using RootMotion.FinalIK;
using Valve.VR;

public class sCalibrator : MonoBehaviour
{
    // Начальные значения параметров калибровки головы и рук
    Vector3 _CadetHeadTargetPos0;
    Vector3 _LeftArmTargetPos0;
    Vector3 _LeftArmTargetEu0;
    Vector3 _RightArmTargetPos0;
    Vector3 _RightArmTargetEu0;

    // Камера и трекеры рук
    [SerializeField]
    Transform _Camera;
    [SerializeField]
    Transform _TrackerLeft;
    [SerializeField]
    Transform _TrackerRight;

    // Начальный рост модели (до уровеня глаз). Male - 1.686, Female - 1.6125
    [SerializeField]
    float _ModelEyesHeight = 1.686f;

    // Начальный масштаб модели
    [SerializeField]
    float _ModelIniScale = 0.95f;

    // Время удержания рук выше головы для срабатывания команды на калибровку
    [SerializeField]
    float _WaitTime = 3;

    // Высота превышния рук над головой
    [SerializeField]
    float _OverTop = 0.15f;

    // Время истечения проверки поднятых рук 
    float _EndTime = 0.0f;

    // Класс для вывода плавающих сообщений
    [SerializeField]
    UI_TextMessage _TextMessage;

    // Класс для вывода диалогов Да/Нет
    [SerializeField]
    UI_Dialog _Dialog;

    // Класс для диалога калибровки
    [SerializeField]
    UI_CalibrationDialog _CalibrationDialog;

    // Модель-шаблон для калибровки рук
    Transform _PatternModel;

    // Полупрозрачный материал модели-шаблона
    [SerializeField]
    Material _PatternModelSkin;

    // Консоль штурвала
    [SerializeField]
    Transform _Console;

    // Точки-цели на ограждении для калибровки рук
    [SerializeField]
    Transform _GuardrailHandPlaceLeft;
    [SerializeField]
    Transform _GuardrailHandPlaceRight;

    // Класс для записи в файл
    [SerializeField]
    sRecord _Record;

    // Дочерний объект камеры - таргет головы модели курсанта
    Transform _CadetHeadTarget;

    // Класс инвесной кинематики VRIK (RootMotion.FinalIK)
    VRIK _VRIK;

    // Класс инвесной кинематики для модели-шаблона
    VRIK _PatternModelVRIK;

    // Флаг состояния калибровки
    bool _CalibrationMode = false;

    // Текущий шаг калибровки
    //int _CalibrationStep = 0;


    // Start is called before the first frame update
    void Start()
    {
        // Получить доступ к классу VRIK
        _VRIK = GetComponent<VRIK>();

        // Дочерний объект камеры - таргет головы модели курсанта
        for (int i = 0; i < _Camera.childCount; i++)
        {
            Transform objTr = _Camera.GetChild(i);
            if (objTr.name == "CadetHeadTarget")
            {
                _CadetHeadTarget = objTr;
                break;
            }
        }

        // Начальный масштаб модели курсанта
        transform.localScale = Vector3.one * _ModelIniScale;


        // Начальные значения параметров калибровки рук
        _CadetHeadTargetPos0 = _VRIK.solver.spine.headTarget.localPosition;
        _LeftArmTargetPos0 = _VRIK.solver.leftArm.target.localPosition;
        _LeftArmTargetEu0 = _VRIK.solver.leftArm.target.localEulerAngles;
        _RightArmTargetPos0 = _VRIK.solver.rightArm.target.localPosition;
        _RightArmTargetEu0 = _VRIK.solver.rightArm.target.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        // Всегда ожидаем начала калибровки. 
        // Сигнал: поднять руки над головой на время >2 секунд
        if (!_CalibrationMode)
        {
            if (HandsUpTime())
            {
                // Вызвать диалог для подтверждения начала калибровки
                _Dialog.ShowDialog("Начать калибровку?", "CadetCalibration");
            }
        }

        //if (Input.GetKeyDown("space"))
        //{
        //    Transform CadetLHTrackerTarget = GameObject.Find("Cadet LH Tracker Target").transform;
        //    print("CadetLHTrackerTarget = " + CadetLHTrackerTarget);
        //    Transform CadetLHJoint = GameObject.Find("LeftHandCollider").transform.parent;
        //    print("CadetLHJoint = " + CadetLHJoint);

        //    _Record.MyLog("");
        //    _Record.MyLog("Левая рука. Цель: pos =\t" + tab(_VRIK.solver.leftArm.target.position, "F3"));
        //    _Record.MyLog("Левая рука. Цель: pos =\t" + tab(CadetLHTrackerTarget.position, "F3"));
        //    _Record.MyLog("Левая рука. Сустав: pos =\t" + tab(_VRIK.references.leftHand.position, "F3"));
        //    _Record.MyLog("Левая рука. Сустав: pos =\t" + tab(CadetLHJoint.position, "F3"));
        //    _Record.MyLog("");

        //    Vector3 myVect = _VRIK.solver.leftArm.target.position - _VRIK.references.leftHand.position;
        //    float myDist = myVect.magnitude;
        //    _Record.MyLog("Вектор =\t" + tab(myVect, "F3") + "\tДлина =\t" + myDist);

        //}
    }

    // Приготовиться к калибровке
    public void MakeReady()
    {
        _CalibrationMode = true;

        // Восстановить масштаб модели курсанта
        transform.localScale = Vector3.one * _ModelIniScale;
        // Восстановить начальные значения параметров калибровки головы и рук
        _VRIK.solver.spine.headTarget.localPosition = _CadetHeadTargetPos0;
        _VRIK.solver.leftArm.target.localPosition = _LeftArmTargetPos0;
        _VRIK.solver.leftArm.target.localEulerAngles = _LeftArmTargetEu0;
        _VRIK.solver.rightArm.target.localPosition = _RightArmTargetPos0;
        _VRIK.solver.rightArm.target.localEulerAngles = _RightArmTargetEu0;

        // Создать модель-шаблон для калибровки
        _PatternModel = Instantiate(transform);
        // Удалить с модели-шаблона скрипт калибровки
        Destroy(_PatternModel.GetComponent<sCalibrator>());
        _PatternModel.parent = transform.parent;
        _PatternModel.name = "Cadet Clone";
        _PatternModel.localPosition = transform.localPosition;
        _PatternModel.localRotation = transform.localRotation;
        _PatternModel.localScale = transform.localScale;
        // Спрятать все лишнее (часы, браслет, тело), оставить только перчатки
        //_PatternModel.Find("m019_hipoly_no-opacity").gameObject.SetActive(false);
        _PatternModel.Find("Wristwatch.006").gameObject.SetActive(false);
        _PatternModel.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 Neck/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Wristwatch").gameObject.SetActive(false);
        _PatternModel.Find("m019_hipoly_no-opacity").gameObject.SetActive(false);
        // Заменить материал на перчатках
        _PatternModel.Find("m019_hipoly_no-opacity.001").GetComponent<Renderer>().material = _PatternModelSkin;
        //_PatternModel.Find("m019_hipoly_no-opacity").GetComponent<Renderer>().material = _PatternModelSkin;

        // Получить доступ к классу VRIK модели-шаблона
        _PatternModelVRIK = _PatternModel.GetComponent<VRIK>();

        // Установить модели-шаблону цели для ИК.
        // Голове и ногам - соответствующие точки основной модели
        _PatternModelVRIK.solver.spine.headTarget = _VRIK.references.head; // Голова
        _PatternModelVRIK.solver.spine.positionWeight = 1.0f;
        _PatternModelVRIK.solver.spine.rotationWeight = 1.0f;
        _PatternModelVRIK.solver.spine.pelvisTarget = _VRIK.references.pelvis; // Крестец
        _PatternModelVRIK.solver.spine.pelvisPositionWeight = 1.0f;
        _PatternModelVRIK.solver.spine.pelvisRotationWeight = 1.0f;
        _PatternModelVRIK.solver.leftLeg.target = _VRIK.references.leftToes;
        _PatternModelVRIK.solver.leftLeg.positionWeight = 1.0f;
        _PatternModelVRIK.solver.leftLeg.rotationWeight = 1.0f;
        _PatternModelVRIK.solver.rightLeg.target = _VRIK.references.rightToes;
        _PatternModelVRIK.solver.rightLeg.positionWeight = 1.0f;
        _PatternModelVRIK.solver.rightLeg.rotationWeight = 1.0f;
        // Рукам - точки-цели на ограждении
        _PatternModelVRIK.solver.leftArm.target = _GuardrailHandPlaceLeft;
        _PatternModelVRIK.solver.rightArm.target = _GuardrailHandPlaceRight;
        _PatternModelVRIK.solver.leftArm.positionWeight = 1.0f;
        _PatternModelVRIK.solver.leftArm.rotationWeight = 1.0f;
        _PatternModelVRIK.solver.rightArm.positionWeight = 1.0f;
        _PatternModelVRIK.solver.rightArm.rotationWeight = 1.0f;


        _CalibrationDialog.ShowDialog();
    }


    // Выполнить калибровку 1) модели курсанта и 2) положения рук.
    public void Calibrate(float CameraHeight)
    {
        // Выполнить масштабирование модели курсанта и ее красной копии (шаблона) по росту пользователя

        // Коэфициент масштабирования
        float Proportions = CameraHeight / _ModelEyesHeight;
        print("Коэфициент масштабирования модели курсанта: " + Proportions);
        // Масштабируем модель
        transform.localScale = Vector3.one * Proportions;
        // Масштабируем смещение таргетов головы и рук пользователя относительно их родителей - камеры и трекеров
        _CadetHeadTarget.localPosition *= Proportions;
        _VRIK.solver.leftArm.target.localPosition *= Proportions;
        _VRIK.solver.rightArm.target.localPosition *= Proportions;

        // Выполнить калибровку таргетов суставов рук

        CorrectOneTarget(_VRIK.solver.leftArm.target, _GuardrailHandPlaceLeft, "Левая рука");
        CorrectOneTarget(_VRIK.solver.rightArm.target, _GuardrailHandPlaceRight, "Правая рука");

        // Удалить модель-шаблон
        Destroy(_PatternModel.gameObject);

        _TextMessage.ShowMessage("Калибровка выполнена. Масштаб = " + Proportions.ToString("F3"), 3);

        _CalibrationMode = false;

    }


    // Удерживаются ли руки на головой заданное время (_WaitTime)
    private bool HandsUpTime()
    {
        if (_EndTime == 0.0f) // Отсчет времени еще не начался
        {
            // Если руки наверху, начинаем отсчет
            if (HandsUp()) _EndTime = Time.time + _WaitTime;
        }
        else // Отсчет времени уже идет
        {
            if (HandsUp()) // Если руки наверху, проверяем время
            {
                if (Time.time >= _EndTime) // Время ожидания до исполнения команды истекло
                {
                    _EndTime = 0.0f;
                    return true;
                }
            }
            else  // Если руки внизу, отсчет прерывается
            {
                _EndTime = 0.0f;
            }
        }
        return false;
    }

    // Подняты ли руки
    private bool HandsUp()
    {
        if ((_TrackerLeft.localPosition.y - _Camera.localPosition.y) > _OverTop && (_TrackerRight.localPosition.y - _Camera.localPosition.y) > _OverTop)
        {
            return true;
        }
        return false;
    }

    private void CorrectOneTarget(Transform Target, Transform PatternJoint, string ConsoleText)
    {
        // Начальное определение координат для положения рук на ограждении. Служебная операция, выполяется один раз для установки постоянных значений в инспекторе
        //_GuardrailHandPlaceLeft.SetPositionAndRotation(Target.position, Target.rotation);
        //_Record.MyLog("");
        //_Record.MyLog(ConsoleText + ": Guardrail Hand Place: localPos =\t" + tab(_GuardrailHandPlaceLeft.localPosition, "F3") + "\tLocalEu =\t" + tab(_GuardrailHandPlaceLeft.localEulerAngles, "F1"));
        //_Record.MyLog("");

        _Record.MyLog(ConsoleText + ": " + Target.name + ".  Было: pos =\t" + tab(Target.localPosition,"F3") + "\teu =\t" + tab(Target.localEulerAngles,"F1"));
        Target.SetPositionAndRotation(PatternJoint.position, PatternJoint.rotation);
        _Record.MyLog(ConsoleText + ": " + Target.name + ". Стало: pos =\t" + tab(Target.localPosition,"F3") + "\teu =\t" + tab(Target.localEulerAngles, "F1"));
        _Record.MyLog("Положение трекеров:");
        
        _Record.MyLog("Камера. Локально: pos =\t" + tab(_Camera.localPosition,"F3") + "\teu\t" + tab(_Camera.localEulerAngles,"F1"));
        _Record.MyLog("Камера. Глобально: pos =\t" + tab(_Camera.position,"F3") + "\teu =\t" + tab(_Camera.eulerAngles,"F1"));
        _Record.MyLog("Голова. Сдвиг цели: pos =\t" + tab(_VRIK.solver.spine.headTarget.localPosition,"F3") + "\teu =\t" + tab(_VRIK.solver.spine.headTarget.localEulerAngles,"F1"));
        _Record.MyLog("Голова. Цель: pos =\t" + tab(_VRIK.solver.spine.headTarget.position,"F3") + "\teu =\t" + tab(_VRIK.solver.spine.headTarget.eulerAngles,"F1"));
        _Record.MyLog("Голова. Сустав: pos =\t" + tab(_VRIK.references.head.position,"F3") + "\teu =\t" + tab(_VRIK.references.head.eulerAngles,"F1"));
        _Record.MyLog("Левый трекер. Локально: pos =\t" + tab(_TrackerLeft.localPosition,"F3") + "\teu =\t" + tab(_TrackerLeft.localEulerAngles,"F1"));
        _Record.MyLog("Левый трекер. Глобально: pos =\t" + tab(_TrackerLeft.position,"F3") + "\teu =\t" + tab(_TrackerLeft.eulerAngles,"F1"));
        _Record.MyLog("Левая рука. Сдвиг цели: pos =\t" + tab(_VRIK.solver.leftArm.target.localPosition,"F3") + "\teu =\t" + tab(_VRIK.solver.leftArm.target.localEulerAngles,"F1"));
        _Record.MyLog("Левая рука. Цель: pos =\t" + tab(_VRIK.solver.leftArm.target.position,"F3") + "\teu =\t" + tab(_VRIK.solver.leftArm.target.eulerAngles,"F1"));
        _Record.MyLog("Левая рука. Сустав: pos =\t" + tab(_VRIK.references.leftHand.position,"F3") + "\teu =\t" + tab(_VRIK.references.leftHand.eulerAngles,"F1"));


        _Record.MyLog("");
        _Record.MyLog("Левая рука. Цель: pos =\t" + tab(_VRIK.solver.leftArm.target.position, "F3"));
        _Record.MyLog("Левая рука. Сустав: pos =\t" + tab(_VRIK.references.leftHand.position, "F3"));
        _Record.MyLog("");

        Vector3 myVect = _VRIK.solver.leftArm.target.position - _VRIK.references.leftHand.position;
        float myDist = myVect.magnitude;
        _Record.MyLog("Вектор =\t" + tab(myVect, "F3") + "\tДлина =\t" + myDist);
        _Record.MyLog("");

        _Record.MyLog("Правый трекер. Локально: pos =\t" + tab(_TrackerRight.localPosition,"F3") + "\teu =\t" + tab(_TrackerRight.localEulerAngles,"F1"));
        _Record.MyLog("Правый трекер. Глобально: pos =\t" + tab(_TrackerRight.position,"F3") + "\teu =\t" + tab(_TrackerRight.eulerAngles,"F1"));
        _Record.MyLog("Правая рука. Сдвиг цели: pos =\t" + tab(_VRIK.solver.rightArm.target.localPosition,"F3") + "\teu =\t" + tab(_VRIK.solver.rightArm.target.localEulerAngles,"F1"));
        _Record.MyLog("Правая рука. Цель: pos =\t" + tab(_VRIK.solver.rightArm.target.position,"F3") + "\teu =\t" + tab(_VRIK.solver.rightArm.target.eulerAngles,"F1"));
        _Record.MyLog("Правая рука. Сустав: pos =\t" + tab(_VRIK.references.rightHand.position,"F3") + "\teu =\t" + tab(_VRIK.references.rightHand.eulerAngles,"F1"));
        _Record.MyLog("Консоль: " + _Console.localPosition.y);

    }

    // Преобразование координат и углов для вывода в файл
    private string tab(Vector3 myVector, string format)
    {
        return (myVector.x.ToString(format) + "\t" + myVector.y.ToString(format) + "\t" + myVector.z.ToString(format)).Replace(".", ",");
    }

}
