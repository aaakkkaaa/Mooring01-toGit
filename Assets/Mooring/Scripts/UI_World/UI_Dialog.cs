using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class UI_Dialog : MonoBehaviour
{

    // Расстояние от камеры, на котором выводится диалог
    [SerializeField]
    float _Distance = 0.5f;

    // Время жизни диалогa
    [SerializeField]
    float _LifeTime = 10.0f;

    // Начальный класс калибровки модели курсанта
    [SerializeField]
    sCalibrator _Calibrator;

    // Дочерний объект - канвас "Dialog_Canvas"
    Transform _DialogCanvas;

    // Текстовый компонент для вывода сообщений
    Text _DialogMessage;

    // Текстовые компоненты - "кнопки" Ди и Нет
    Text _ButtonYes;
    Text _ButtonNo;

    // Флаг отображения диалога
    bool _DialogIsOn = false;

    // Флаг "Коллизия руки с кнопкой еще продолжается" (смысл: "Выбор практически сделан, но не закончен")
    bool _CollisionIsOn = false;

    // Цвета подсветки кнопок Да и Нет
    Color _ButtonsColorDefault; // 130,77,24,90; 824D18
    Color _ButtonsColorHightLighted = new Color(255f,0f,0f,50f); //FF0000

    // Ключ - имя для обраного вызова методов при выборе "Да"
    string _MethodKey;

    // Use this for initialization
    void Start()
    {

        // Дочерний объект - канвас "Dialog_Canvas"
        for (int i=0; i<transform.childCount; i++)
        {
            Transform objTr = transform.GetChild(i);
            if (objTr.name == "Dialog_Canvas")
            {
                _DialogCanvas = objTr;
                break;
            }
        }

        // Текстовый компонент для вывода сообщений
        for (int i = 0; i < _DialogCanvas.childCount; i++)
        {
            Transform objTr = _DialogCanvas.GetChild(i);
            if (objTr.name == "Dialog_Message")
            {
                _DialogMessage = objTr.GetComponent<Text>();
            }
            else if (objTr.name == "Butt_Y")
            {
                _ButtonYes = objTr.GetComponent<Text>();
            }
            else if (objTr.name == "Butt_X")
            {
                _ButtonNo = objTr.GetComponent<Text>();
            }
        }

        // Цвета подсветки кнопок Да и Нет
        _ButtonsColorDefault = _ButtonNo.transform.GetChild(0).GetComponent<Renderer>().material.color;

        // Выключить себя в начале
        gameObject.SetActive(false);

    }


    void Update()
    {
        // Закрытие диалога по нажатию клавиши Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialog();
        }
    }


    // Подготовить и показать диалог Да/Нет. Отображается текст (Message) переданный из вызывающей функции
    public void ShowDialog(string Message, string MethodKey)
    {
        // Если диалог в данный момент отображается, ничего не делать
        if (_DialogIsOn) return;

        // Запомнить родителя
        Transform Parent = transform.parent;
        // Перевести себя в дочерние объекты камеры
        transform.SetParent(Camera.main.transform);
        // Совместить себя с камерой
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        // Откорректировать, чтобы углы тангажа и крена были 0
        Vector3 myEu = transform.eulerAngles;
        myEu.x = 0.0f;
        myEu.z = 0.0f;
        transform.eulerAngles = myEu;
        // Передвинуть себя на _Distance вперед
        transform.Translate(Vector3.forward * _Distance);
        // Вернуть себя обратно родителю
        transform.SetParent(Parent);

        // Запомнить ключ метода для последующего вызова
        _MethodKey = MethodKey;
        // Вставить текст сообщения
        _DialogMessage.text = Message;

        // Включить себя
        gameObject.SetActive(true);
        _DialogIsOn = true;

        // Выключить себя через заданное время жизни
        StartCoroutine(TimeIsOver(_LifeTime));
    }

    // Подсвечивать опцию выбора.
    // Вызывается скриптом внешнего объекта (руки) - Activator. Получает MyCollider - какой из своих коллайдеров был активирован.
    public void myTriggerEnter(Collider MyCollider, Transform Activator)
    {
        print("myTriggerEnter: Коллайдер " + MyCollider.name + " - вход");

        _CollisionIsOn = true;

        // Имя объекта внешнего коллайдера
        string ExtName = Activator.name;
        // Трансформ собственного коллайдера
        Transform myTr = MyCollider.transform;

        //myTr.GetChild(0).gameObject.SetActive(true);
        myTr.GetChild(0).GetComponent<Renderer>().material.color = _ButtonsColorHightLighted;
        print("myTr = " + myTr + " GetChild(0) = " + myTr.GetChild(0) + " material = " + myTr.GetChild(0).GetComponent<Renderer>().material + " Цвет подсветки: " + _ButtonsColorHightLighted);
    }

    // Выбор сделан - выход коллайдеров из контакта
    public void myTriggerExit(Collider MyCollider, Transform Activator)
    {
        // Имя объекта внешнего коллайдера
        string ExtName = Activator.name;
        // Трансформ собственного коллайдера
        Transform myTr = MyCollider.transform;

        //myTr.GetChild(0).gameObject.SetActive(false);
        _ButtonYes.transform.GetChild(0).GetComponent<Renderer>().material.color = _ButtonsColorDefault;
        _ButtonNo.transform.GetChild(0).GetComponent<Renderer>().material.color = _ButtonsColorDefault;

        // Действия по выходу
        if (myTr.name == "Butt_X")
        {
            print("Выбрано НЕТ");
        }
        else if(myTr.name == "Butt_Y")
        {
            print("Выбрано ДА");
            if(_MethodKey == "CadetCalibration")
            {
                // Приготовиться к калибровке
                _Calibrator.MakeReady();
            }
        }

        // Выключить себя
        CloseDialog();
    }

    // Закрытие диалога
    private void CloseDialog()
    {
        if (_DialogIsOn)
        {
            // Выключить себя
            _DialogIsOn = false;
            _CollisionIsOn = false;
            gameObject.SetActive(false);
        }
    }

    // Выключить себя через заданное время жизни
    IEnumerator TimeIsOver(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        // Если коллизия руки с кнопкой продолжается, то не выключать. Все равно выключится после окончания коллизии  
        if (!_CollisionIsOn)
        {
            CloseDialog();
        }
    }

}