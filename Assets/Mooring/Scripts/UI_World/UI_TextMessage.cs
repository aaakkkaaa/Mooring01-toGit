using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UI_TextMessage : MonoBehaviour
{

    // Время, за которое сообщение возвращается в центр поля зрения
    [SerializeField]
    float myCenterTime = 0.3f;
    // Время, за которое сообщение растворяется после окончания времени жизни
    [SerializeField]
    float myDissolveTime = 0.5f;
    // Расстояние от камеры, на котором висит сообщение
    [SerializeField]
    float myDistance = 0.75f;

    // Дочерний объект - канвас "Message_Canvas"
    Transform myMessageCanvasTr;

    // Текстовый компонент для вывода сообщений
    Text myMessageText;

    // Скорости поворота UI канваса за камерой. Требуются для работы функции Mathf.SmoothDampAngle
    float myVelocityX = 0.0F;
    float myVelocityY = 0.0F;
    float myVelocityZ = 0.0F;

    // Флаг отображения сообщения
    bool myMessageIsKeeping = false;

    // Время начала выключания сообщения
    float myBeginDissolveTime = 0.0f;

    // Настройки и начальные данные проекта
    //sIniSet myIni;

    // Use this for initialization
    void Start()
    {

        // Дочерний объект - канвас "Message_Canvas"
        for (int i=0; i<transform.childCount; i++)
        {
            Transform myObjTr = transform.GetChild(i);
            if (myObjTr.name == "Canvas_Message")
            {
                myMessageCanvasTr = myObjTr;
                break;
            }
        }

        // Текстовый компонент для вывода сообщений
        for (int i = 0; i < myMessageCanvasTr.childCount; i++)
        {
            Transform myObjTr = myMessageCanvasTr.GetChild(i);
            if (myObjTr.name == "Text_Message")
            {
                myMessageText = myObjTr.GetComponent<Text>();
                break;
            }
        }

        // Выключить себя в начале
        gameObject.SetActive(false);

    }

    /*
    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            print("1 pressed");
            MyFuncShowMessage("Нажато один. Привет, мир!", 3.0f);
        }
        else if (Input.GetKeyDown("2"))
        {
            print("2 pressed");
            MyFuncShowMessage("Нажато два. Пока, мир!", 3.0f);
        }
    }
    */

    // Подготовить и показать текстовое сообщение. Отображается текст (myMessage) переданный из вызывающей функции
    public void ShowMessage(string myMessage, float myLifeTime)
    {
        // Перевести себя в дочерние объекты камеры
        transform.parent = Camera.main.transform;
        // Совместить себя с камерой
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        // Передвинуть себя на myDistance вперед
        transform.Translate(Vector3.forward * myDistance);
        // Откорректировать, чтобы угол крена был 0
        Vector3 myEu = transform.eulerAngles;
        myEu.z = 0.0f;
        transform.eulerAngles = myEu;
        // Вернуть себя обратно в корень сцены
        transform.parent = null;

        // Вставить текст сообщения
        myMessageText.text = myMessage;
        // Включить себя
        gameObject.SetActive(true);
        if (myLifeTime < 0.0f) // если myLifeTime меньше 0, то время жизни устанавливается 1 час (то есть практически не ограничено)
        {
            myLifeTime = 3600.0f;
        }
        // Установить время начала убирания сообщения
        myBeginDissolveTime = Time.time + myLifeTime;
        // Если сообщение в данный момент не отображается
        if (!myMessageIsKeeping)
        {
            // Запустить процедуру жизни сообщения
            StartCoroutine(KeepMessage());
        }
        else // если старое сообщение еще отображается
        {
            // Восстановить непрозрачность текста нового сообщения (на случай, если старое уже начало растворяться)
            Vector4 myTextColor = myMessageText.color;
            myTextColor.w = 1.0f;
            myMessageText.color = myTextColor;
        }
    }


    // Держать сообщение в поле зрения, затем убрать после истечения заданного времени
    IEnumerator KeepMessage()
    {
        myMessageIsKeeping = true;
        // Цвет текста
        Vector4 myTextColor = myMessageText.color;

        yield return null; // подождать до следующего кадра

        while (myTextColor.w > 0.0f)
        {
            // Текущее время
            float myTime = Time.time;

            // Держать сообщение в поле зрения

            // Запомнить родителя
            Transform myParent = transform.parent;
            // Перевести себя в дочерние объекты камеры
            transform.SetParent(Camera.main.transform);
            // Совместить себя с камерой
            transform.localPosition = Vector3.zero;
            // Текущие углы себя относительно камеры
            Vector3 myEu = transform.localEulerAngles;
            // Новые значения углов
            myEu.x = Mathf.SmoothDampAngle(myEu.x, 0.0f, ref myVelocityX, myCenterTime);
            myEu.y = Mathf.SmoothDampAngle(myEu.y, 0.0f, ref myVelocityY, myCenterTime);
            myEu.z = Mathf.SmoothDampAngle(myEu.z, 0.0f, ref myVelocityZ, myCenterTime);
            transform.localEulerAngles = myEu;
            // Передвинуть себя на myDistance вперед
            transform.Translate(Vector3.forward * myDistance);
            // Откорректировать, чтобы угол крена был 0
            myEu = transform.eulerAngles;
            myEu.z = 0.0f;
            transform.eulerAngles = myEu;
            // Вернуть себя обратно родителю
            transform.SetParent(myParent);

            // Если пришло время убирать сообщение
            if (myTime > myBeginDissolveTime) 
            // Постепенный переход текста в прозрачность
            {
                myTextColor.w = Mathf.SmoothStep(1.0f, 0.0f, (myTime - myBeginDissolveTime) / myDissolveTime);
                myMessageText.color = myTextColor;
            }
            yield return null; // подождать до следующего кадра
        }

        // Убрать сообщение
        // Выключить себя
        gameObject.SetActive(false);
        // Текст сообщения по умолчанию
        myMessageText.text = "Message";
        // Восстановить непрозрачность текста
        myTextColor.w = 1.0f;
        myMessageText.color = myTextColor;
        // Флаг отображения сообщения
        myMessageIsKeeping = false;
        // Время начала выключания сообщения
        myBeginDissolveTime = 0.0f;

    }

}