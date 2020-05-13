using Obi;
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

    // Точка, где должен находиться этот трекер в виртуальном пространстве
    [SerializeField]
    Transform _ConsoleTrackerTrackerPose;

    // Вспомогательный объект для коррекции положения [Camera Rig]
    [SerializeField]
    Transform _TrackerTempPose;

    // канаты для выключения моделирования
    private ObiRope[] _ropes;
    private ropeTrick[] _ropeTricks;

    // Start is called before the first frame update
    void Start()
    {
        // Зафиксировать положение яхты чтобы избавиться от конфликта с канатом
        Rigidbody ship = MainShip.GetComponent<Rigidbody>();
        ship.constraints = RigidbodyConstraints.FreezeAll;

        // Запустить корутину Регулировки громкости
        StartCoroutine(ChangeVolume());

        // Выключить обработку каната
        StartCoroutine(FreezeRope());

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
        {
            if (Input.GetKeyDown("m")) // Проверка меню
            {
                _MessageScr.ShowMessage("Проверка меню", 2f);
            }
        }
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
    IEnumerator FreezeRope()
    {
        // Переждать время 0.1 секунды
        yield return new WaitForSeconds(0.1f);

        // Find - ликвидировать при возможности
        _ropes = FindObjectsOfType<ObiRope>();
        for (int i = 0; i < _ropes.Length; i++)
        {
            _ropes[i].transform.SetParent(GameObject.Find("BakedRope").transform);
        }
        _ropeTricks = FindObjectsOfType<ropeTrick>();
        for (int i = 0; i < _ropes.Length; i++)
        {
            _ropeTricks[i].gameObject.SetActive(false);
        }
        Rigidbody ship = MainShip.GetComponent<Rigidbody>();
        ship.constraints = RigidbodyConstraints.None;

        // Провести калибровку положения базы камеры ([CameraRig]) относительно трекера HTC Vive, закрепленного на консоли штурвала в реальном пространстве

        // Высота консоли в модели яхты
        if (_ConsoleTracker.localPosition.y != 0.0f) // если VR работает
        {
            Transform Console = _ConsoleTrackerTrackerPose.parent;
            Vector3 ConcolePos = Console.localPosition;
            print("Высота установки консоли была: " + ConcolePos.y);
            ConcolePos.y += _ConsoleTracker.position.y - _ConsoleTrackerTrackerPose.position.y;
            print("Высота установки консоли стала: " + ConcolePos.y);
            Console.localPosition = ConcolePos;

            // Положение трекера в пространстве
            Transform CameraRig = _ConsoleTracker.parent;
            Transform MainShip = CameraRig.parent;
            _TrackerTempPose.SetParent(MainShip);
            CameraRig.SetParent(_TrackerTempPose);
            _TrackerTempPose.position = _ConsoleTrackerTrackerPose.position;
            CameraRig.SetParent(MainShip);
            _TrackerTempPose.SetParent(_ConsoleTracker);
            _TrackerTempPose.position = Vector3.zero;

        }

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
