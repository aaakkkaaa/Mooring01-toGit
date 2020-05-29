using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using RootMotion;
using RootMotion.FinalIK;
using Valve.VR;

public class sCalibrator : MonoBehaviour
{
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

    // Класс для вывода диалогов
    [SerializeField]
    UI_Dialog _Dialog;

    // Класс улетающего меню для масштабирования по росту
    [SerializeField]
    UI_GoAway _GoAway;

    // Модель-шаблон для калибровки рук
    Transform _PatternModel;

    // Полупрозрачный материал модели-шаблона
    [SerializeField]
    Material _PatternModelSkin;

    // Консоль штурвала
    [SerializeField]
    Transform _Console;

    // Точки-цели на штурвале для калибровки рук
    [SerializeField]
    Transform _HelmHandPlaceLeft;
    [SerializeField]
    Transform _HelmHandPlaceRight;

    // Класс для записи в файл
    [SerializeField]
    sRecord _Record;

    // Родительский объект точек-целей на штурвале для калибровки рук (куда вернуть их после калибровки рук)
    Transform _HelmHandPlacesParent;

    // Суставы модели-шаблона
    //Transform _PatternHead;
    //Transform _PatternRightHand;
    //Transform _PatternLeftHand;
    //Transform _PatternLeftFoot;
    //Transform _PatternRighFoot;

    // Дочерний объект камеры - таргета головы модели курсанта
    Transform _CadetHeadTarget;

    // Класс инвесной кинематики VRIK (RootMotion.FinalIK)
    VRIK _VRIK;

    // Класс инвесной кинематики для модели-шаблона
    VRIK _PatternModelVRIK;

    // Флаг состояния калибровки
    bool _CalibrationMode = false;


    // Start is called before the first frame update
    void Start()
    {
        // Получить доступ к классу VRIK
        _VRIK = GetComponent<VRIK>();

        // Родительский объект точек-целей на штурвале для калибровки рук (куда вернуть их после калибровки рук)
        _HelmHandPlacesParent = _HelmHandPlaceLeft.parent;

        // Дочерний объект камеры - таргета головы модели курсанта
        for (int i = 0; i < _Camera.childCount; i++)
        {
            Transform objTr = _Camera.GetChild(i);
            if (objTr.name == "CadetHeadTarget")
            {
                _CadetHeadTarget = objTr;
                break;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        // Всегда ожидаем начала калибровки. 
        // Встать ровно, смотреть перед собой.
        // Сигнал: поднять руки над головой на время >2 секунд
        if (HandsUpTime())
        {
            // Вызвать диалог для подтверждения начала калибровки
            _Dialog.ShowDialog("Начать калибровку?", "ScaleModel");
        }
        if (Input.GetKeyDown("space"))
        {
            Transform CadetLHTrackerTarget = GameObject.Find("Cadet LH Tracker Target").transform;
            print("CadetLHTrackerTarget = " + CadetLHTrackerTarget);
            Transform CadetLHJoint = GameObject.Find("LeftHandCollider").transform.parent;
            print("CadetLHJoint = " + CadetLHJoint);

            _Record.MyLog("");
            _Record.MyLog("Левая рука. Цель: pos =\t" + tab(_VRIK.solver.leftArm.target.position, "F3"));
            _Record.MyLog("Левая рука. Цель: pos =\t" + tab(CadetLHTrackerTarget.position, "F3"));
            _Record.MyLog("Левая рука. Сустав: pos =\t" + tab(_VRIK.references.leftHand.position, "F3"));
            _Record.MyLog("Левая рука. Сустав: pos =\t" + tab(CadetLHJoint.position, "F3"));
            _Record.MyLog("");

            Vector3 myVect = _VRIK.solver.leftArm.target.position - _VRIK.references.leftHand.position;
            float myDist = myVect.magnitude;
            _Record.MyLog("Вектор =\t" + tab(myVect, "F3") + "\tДлина =\t" + myDist);

        }
    }

    // Определить высоту камеры
    public void GetCameraHeight()
    {
        _CalibrationMode = true;
        _GoAway.FlyAway();
    }

    // Выполнить масштабирование модели по росту пользователя
    public void ScaleModel(float CameraHeight)
    {
        // Коэфициент масштабирования
        float Proportions = CameraHeight / _ModelEyesHeight;
        print("Коефициент масштабирования модели курсанта: " + Proportions);
        // Масштабируем модель
        transform.localScale = Vector3.one * Proportions;
        // Масштабируем смещение таргета головы и рук пользователя относительно камеры и трекеров
        _CadetHeadTarget.localPosition *= Proportions;
        _VRIK.solver.leftArm.target.localPosition *= Proportions;
        _VRIK.solver.rightArm.target.localPosition *= Proportions;
        // Масштабируем смещение таргета головы и рук пользователя относительно камеры и трекеров
        _CadetHeadTarget.localPosition *= Proportions;
        _VRIK.solver.leftArm.target.localPosition *= Proportions;
        _VRIK.solver.rightArm.target.localPosition *= Proportions;
        // Масштабируем смещение точек - целей на штурвале
        _HelmHandPlaceLeft.localPosition *= Proportions;
        _HelmHandPlaceRight.localPosition *= Proportions;

        _TextMessage.ShowMessage("Масштабирование (" + Proportions.ToString("F3", CultureInfo.InvariantCulture) + ") выполнено.\nЗаймите положение для калибровки рук", 3);

        // Приготовиться к калибровке рук

        // Создать модель-шаблон для калибровки рук и совместить ее с основной моделью
        _PatternModel = Instantiate(transform);
        Destroy(_PatternModel.GetComponent<sCalibrator>());
        _PatternModel.parent = transform.parent;
        _PatternModel.name = "Cadet Clone";
        _PatternModel.localPosition = transform.localPosition;
        _PatternModel.localRotation = transform.localRotation;
        _PatternModel.localScale = transform.localScale;
        // Заменить материал на теле модели-шаблона
        _PatternModel.Find("m019_hipoly_no-opacity").GetComponent<Renderer>().material = _PatternModelSkin;

        // Получить доступ к классу VRIK модели-шаблона
        _PatternModelVRIK = _PatternModel.GetComponent<VRIK>();

        //_PatternLeftHand = _PatternModelVRIK.references.leftHand;
        //_PatternRightHand = _PatternModelVRIK.references.rightHand;

        //_PatternHead = _PatternModel.Find("Bip01 Head");
        //_PatternLeftFoot = _PatternModel.Find("Bip01 L Toe0");
        //_PatternRighFoot = _PatternModel.Find("Bip01 R Toe0");


        // Установить модели-шаблону цели для ИК.
        // Голове и ногам - соответствующие точки основной модели
        _PatternModelVRIK.solver.spine.headTarget = _VRIK.references.head;
        _PatternModelVRIK.solver.leftLeg.target = _VRIK.references.leftToes;
        _PatternModelVRIK.solver.leftLeg.positionWeight = 1.0f;
        _PatternModelVRIK.solver.leftLeg.rotationWeight = 1.0f;
        _PatternModelVRIK.solver.rightLeg.target = _VRIK.references.rightToes;
        _PatternModelVRIK.solver.rightLeg.positionWeight = 1.0f;
        _PatternModelVRIK.solver.rightLeg.rotationWeight = 1.0f;
        // Рукам - точки-цели на штурвале (предварительно вынеся точки-цели из детей штурвала в модель яхты, чтобы не крутились)
        _HelmHandPlaceLeft.SetParent(_Console.parent);
        _HelmHandPlaceRight.SetParent(_Console.parent);
        _PatternModelVRIK.solver.leftArm.target = _HelmHandPlaceLeft;
        _PatternModelVRIK.solver.rightArm.target = _HelmHandPlaceRight;
        _PatternModelVRIK.solver.leftArm.positionWeight = 1.0f;
        _PatternModelVRIK.solver.leftArm.rotationWeight = 1.0f;
        _PatternModelVRIK.solver.rightArm.positionWeight = 1.0f;
        _PatternModelVRIK.solver.rightArm.rotationWeight = 1.0f;

        StartCoroutine(CalibrateHands());
    }

    IEnumerator CalibrateHands()
    {
        yield return new WaitForSeconds(2f);
        _TextMessage.ShowMessage("□□□□□", 2);
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1f);
            if (i == 0) _TextMessage.ShowMessage("■□□□□", 2);
            else if (i == 1) _TextMessage.ShowMessage("■■□□□", 2);
            else if (i == 2) _TextMessage.ShowMessage("■■■□□", 2);
            else if (i == 3) _TextMessage.ShowMessage("■■■■□", 2);
            else if (i == 4) _TextMessage.ShowMessage("■■■■■", 0.1f);
        }
        // Выполнить калибровку таргетов суставов
        {
            CorrectOneTarget(_VRIK.solver.leftArm.target, _HelmHandPlaceLeft, "Левая рука");
            CorrectOneTarget(_VRIK.solver.rightArm.target, _HelmHandPlaceRight, "Правая рука");

            // Удалить модель-шаблон
            Destroy(_PatternModel.gameObject);

            // Вернуть точки-цели на штурвале родителю
            _HelmHandPlaceLeft.SetParent(_HelmHandPlacesParent);
            _HelmHandPlaceRight.SetParent(_HelmHandPlacesParent);

            _TextMessage.ShowMessage("Калибровка рук выполнена", 3);

            _CalibrationMode = false;
        }
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
        //print(ConsoleText + ": " + Target.name + ".  Было: pos = " + Target.localPosition.ToString("F3") + ", eu = " + Target.localEulerAngles);
        //Target.SetPositionAndRotation(PatternJoint.position, PatternJoint.rotation);
        //print(ConsoleText + ": " + Target.name + ". Стало: pos = " + Target.localPosition.ToString("F3") + ", eu = " + Target.localEulerAngles);
        //print("Положение трекеров:");
        //print("Аватар: pos = " + transform.localPosition.ToString("F3") + ", eu = " + transform.localEulerAngles.ToString("F1"));
        //print("Камера. Локально: pos = " + _Camera.localPosition.ToString("F3") + ", eu = " + _Camera.localEulerAngles.ToString("F1"));
        //print("Камера. Глобально: pos = " + _Camera.position.ToString("F3") + ", eu = " + _Camera.eulerAngles.ToString("F1"));
        //print("Голова. Цель: pos = " + _VRIK.solver.spine.headTarget.position.ToString("F3") + ", eu = " + _VRIK.solver.spine.headTarget.eulerAngles.ToString("F1"));
        //print("Голова. Сустав: pos = " + _VRIK.references.head.position.ToString("F3") + ", eu = " + _VRIK.references.head.eulerAngles.ToString("F1"));
        //print("Левый трекер. Локально: pos = " + _TrackerLeft.localPosition.ToString("F3") + ", eu = " + _TrackerLeft.localEulerAngles.ToString("F1"));
        //print("Левый трекер. Глобально: pos = " + _TrackerLeft.position.ToString("F3") + ", eu = " + _TrackerLeft.eulerAngles.ToString("F1"));
        //print("Левая рука. Цель: pos = " + _VRIK.solver.leftArm.target.position.ToString("F3") + ", eu = " + _VRIK.solver.leftArm.target.eulerAngles.ToString("F1"));
        //print("Левая рука. Сустав: pos = " + _VRIK.references.leftHand.position.ToString("F3") + ", eu = " + _VRIK.references.leftHand.eulerAngles.ToString("F1"));
        //print("Правый трекер. Локально: pos = " + _TrackerRight.localPosition.ToString("F3") + ", eu = " + _TrackerRight.localEulerAngles.ToString("F1"));
        //print("Правый трекер. Глобально: pos = " + _TrackerRight.position.ToString("F3") + ", eu = " + _TrackerRight.eulerAngles.ToString("F1"));
        //print("Правая рука. Цель: pos = " + _VRIK.solver.rightArm.target.position.ToString("F3") + ", eu = " + _VRIK.solver.rightArm.target.eulerAngles.ToString("F1"));
        //print("Правая рука. Сустав: pos = " + _VRIK.references.rightHand.position.ToString("F3") + ", eu = " + _VRIK.references.rightHand.eulerAngles.ToString("F1"));
        //print("Консоль: " + _Console.localPosition.y);

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
