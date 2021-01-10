using Obi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class sAssist : MonoBehaviour
{

    // Текстовое сообщение для курсанта
    [SerializeField]
    UI_TextMessage _MessageScr;

    // яхта
    public GameObject MainShip;

    // Трекер, закрепленный на консоли шутрвала в реальном пространстве
    [SerializeField]
    Transform _ConsoleTracker;

    // Точка, где должен находиться этот трекер в виртуальном пространстве (на модели консоли управления корабля)
    [SerializeField]
    Transform _ConsoleTrackerPose;

    // Вспомогательный объект для коррекции положения [Camera Rig]
    [SerializeField]
    Transform _TrackerTempPose;

    // канаты для выключения моделирования
    private ObiRope[] _ropes;
    private RopeTrick[] _ropeTricks;

    // Класс для записи в файл
    sRecord _Record;


    // Start is called before the first frame update
    void Start()
    {

        // Класс для записи в файл
        _Record = transform.GetComponent<sRecord>();

        // Зафиксировать положение яхты чтобы избавиться от конфликта с канатом
        Rigidbody ship = MainShip.GetComponent<Rigidbody>();
        ship.constraints = RigidbodyConstraints.FreezeAll;

        // Запустить корутину Регулировки громкости
        StartCoroutine(ChangeVolume());

        // Выключить обработку каната
        StartCoroutine(AfterStart());

    }

    // Update is called once per frame
    void Update()
    {
    }


    // Регулировка громкости
    IEnumerator ChangeVolume()
    // Проверять каждые 0.2 сек нажатие клавиш + и -
    // одновременно с клавишей Ctrl (и левой и правой)
    {

        // Обработка начинается
        float mySoundStep = 0.0f; // шаг регулировки громкости
        StringBuilder myMessage = new StringBuilder("□□□□□□□□□□"); // объект для индикатора текущей громкости
        int myVol; // целое число для индикатора громкости

        // Осторожно: "бесконечный" цикл
        while (true)
        {

            // Нажаты ли клавиши "+/-", одновременно с Ctrl
            if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
            {
                if (Input.GetKey("=")) // + (на самом деле =)
                {
                    mySoundStep = 0.1f;
                }
                else if (Input.GetKey("-")) // -
                {
                    mySoundStep = -0.1f;
                }
            }

            // Если какая-то кнопка управления громкостью была нажата, то mySoundStep != 0
            if (mySoundStep != 0.0f)
            {
                // Устанавливаем новую громкость (от 0.0 до 1.0)
                AudioListener.volume = Mathf.Clamp01(AudioListener.volume + mySoundStep);
                // Целочисленный параметр для индикации громкости (от 0 до 10)
                myVol = Mathf.RoundToInt(AudioListener.volume * 10);
                // Подготовим объект для индикатора текущей громкости
                for (int i = 0; i < 10; i++)
                {
                    if (i <= myVol - 1)
                    {
                        myMessage[i] = '■';
                    }
                    else
                    {
                        myMessage[i] = '□';
                    }
                }

                mySoundStep = 0.0f;

                // Выведем текущую громкость
                print("Audio Volume = " + AudioListener.volume + "; myVol = " + myVol + "; myMessage = " + myMessage);
                _MessageScr.ShowMessage(myMessage.ToString(), 2.0f);
            }

            // Переждать время 0.2 секунды
            yield return new WaitForSeconds(0.2f);
        }
    }

    // Выключить обработку каната
    IEnumerator AfterStart()
    {
        // Переждать время 0.1 секунды
        yield return new WaitForSeconds(0.1f);

        // Find - ликвидировать при возможности
        _ropes = FindObjectsOfType<ObiRope>();
        for (int i = 0; i < _ropes.Length; i++)
        {
            _ropes[i].transform.SetParent(GameObject.Find("BakedRope").transform);
        }
        _ropeTricks = FindObjectsOfType<RopeTrick>();
        for (int i = 0; i < _ropeTricks.Length; i++)
        {
            _ropeTricks[i].gameObject.SetActive(false);
        }
        Rigidbody ship = MainShip.GetComponent<Rigidbody>();
        ship.constraints = RigidbodyConstraints.None;

        // Провести калибровку положения базы камеры ([CameraRig]) относительно трекера HTC Vive, закрепленного на консоли штурвала в реальном пространстве

        print("_ConsoleTracker.localPosition.y = " + _ConsoleTracker.localPosition.y);
        if (_ConsoleTracker.localPosition.y != 0.0f) // если VR работает
        {
            // Положение трекера на консоли штурвала
            //_Record.MyLog("Положение трекера на консоли (локальное): localPos=\t" + tab(_ConsoleTracker.localPosition, "F3") + "\tlocalEu =\t" + tab(_ConsoleTracker.localEulerAngles, "F1"));
            //_Record.MyLog("Положение трекера на консоли (глобальное): globalPos=\t" + tab(_ConsoleTracker.position, "F3") + "\tglobalEu =\t" + tab(_ConsoleTracker.eulerAngles, "F1"));
            //_Record.MyLog("Место для трекера на консоли (локальное): localPos=\t" + tab(_ConsoleTrackerPose.localPosition, "F3") + "\tlocalEu =\t" + tab(_ConsoleTrackerPose.localEulerAngles, "F1"));
            //_Record.MyLog("Место для трекера на консоли (глобальное): globalPos=\t" + tab(_ConsoleTrackerPose.position, "F3") + "\tglobalEu =\t" + tab(_ConsoleTrackerPose.eulerAngles, "F1"));

            Transform Console = _ConsoleTrackerPose.parent;
            Vector3 ConcolePos = Console.localPosition;

            // Высота консоли в модели яхты

            //_Record.MyLog("Положение консоли было (локальное): localPos=\t" + tab(ConcolePos, "F3") + "\tlocalEu =\t" + tab(Console.localEulerAngles, "F1"));
            //_Record.MyLog("Положение консоли было (глобальное): globalPos=\t" + tab(Console.position, "F3") + "\tglobalEu =\t" + tab(Console.eulerAngles, "F1"));

            print("Высота установки консоли была: " + ConcolePos.y);
            ConcolePos.y += _ConsoleTracker.position.y - _ConsoleTrackerPose.position.y;
            print("Высота установки консоли стала: " + ConcolePos.y);
            Console.localPosition = ConcolePos;
            //_Record.MyLog("Положение консоли стало (локальное): localPos=\t" + tab(ConcolePos, "F3") + "\tlocalEu =\t" + tab(Console.localEulerAngles, "F1"));
            //_Record.MyLog("Положение консоли стало (глобальное): globalPos=\t" + tab(Console.position, "F3") + "\tglobalEu =\t" + tab(Console.eulerAngles, "F1"));

            // Выставить положение [Camera Rig] в виртуальном пространстве по положению трекера консоли в рельном пространстве (только положение, не углы)

            Transform CameraRig = _ConsoleTracker.parent; // [Camera Rig] - оснастка VR камеры от SteamVR
            Transform MainShip = CameraRig.parent; // Корабль
            _TrackerTempPose.SetParent(MainShip); // Вспомогательный объект для коррекции положения [Camera Rig] - перевести в дети корабля
            CameraRig.SetParent(_TrackerTempPose); // [Camera Rig] - перевести в дети вспомогательного объекта
            _TrackerTempPose.position = _ConsoleTrackerPose.position; // // Переместить вспомогательный объект с детьми ([Camera Rig]) в точку, где должен находиться _ConsoleTracker в виртуальном пространстве
            CameraRig.SetParent(MainShip); // Вернуть [Camera Rig] в дети корабля
            _TrackerTempPose.SetParent(_ConsoleTracker); // Вернуть вспомогательный объект в дети _ConsoleTracker
            _TrackerTempPose.position = Vector3.zero;

        }

    }

    // Преобразование координат и углов для вывода в файл
    private string tab(Vector3 myVector, string format)
    {
        return (myVector.x.ToString(format) + "\t" + myVector.y.ToString(format) + "\t" + myVector.z.ToString(format)).Replace(".", ",");
    }



    // ==========================================================================
    //                         Общедоступные методы
    // ==========================================================================

    // Нормализация угла: привести угол к (-180/+180)
    public float NormalizeAngle(float myAngle)
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
