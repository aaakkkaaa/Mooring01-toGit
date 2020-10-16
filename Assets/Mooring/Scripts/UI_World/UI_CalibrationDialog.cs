using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_CalibrationDialog: MonoBehaviour
{

    [SerializeField]
    Transform _MainShip;

    [SerializeField]
    Transform _Aim;

    // Начальный класс калибровки модели курсанта
    [SerializeField]
    sCalibrator _Calibrator;

    Transform _Parent;

    // Время, которое нужно удерживать захваченную мишень, чтобы начать выполнение калибровки, сек.
    public float CaptureTime = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Выключить себя в начале
        gameObject.SetActive(false);
    }


    // Диалог калибровки модели кадета: Встаньте перед штурвалом
    public void ShowDialog()
    {
        // Запомнить родителя
        _Parent = transform.parent;

        // Перевести себя в дочерние объекты яхты
        transform.SetParent(_MainShip);
        transform.localPosition = new Vector3(0.77f, 2.21f, -4.25f);
        transform.localEulerAngles = Vector3.zero;

        // Включить себя
        gameObject.SetActive(true);

        // Перевести прицел в дочерние объекты камеры
        _Aim.SetParent(Camera.main.transform);
        _Aim.localPosition = Vector3.forward;
        _Aim.localEulerAngles = Vector3.zero;

    }

    public void StartCalibration(float CameraHeight)
    {

        // Вернуть прицел обратно себе
        _Aim.SetParent(transform);

        // Вернуть себя обратно родителю
        transform.SetParent(_Parent);
        // Выключить себя
        gameObject.SetActive(false);

        print("**********************  НАЧАТЬ КАЛИБРОВКУ!  ******************************");
        _Calibrator.Calibrate(CameraHeight);
    }

}
