using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GoAway : MonoBehaviour
{
    [SerializeField]
    Transform _Camera;

    [SerializeField]
    Text _TimerBoxes;

    [SerializeField]
    sCalibrator _Calibrator;

    [SerializeField]
    Transform _MainShip;

    float _CameraHeight;

    Transform _Parent;

    // Start is called before the first frame update
    void Start()
    {
        // Выключить себя в начале
        gameObject.SetActive(false);
    }

    public void FlyAway()
    {

        // Запомнить родителя
        _Parent = transform.parent;
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
        // Перевести себя в дочерние объекты яхты
        transform.SetParent(_MainShip);
        // Откорректировать, чтобы угол рыскания был 0
        myEu = transform.localEulerAngles;
        myEu.y = 0.0f;
        transform.localEulerAngles = myEu;
        // Передвинуть себя вперед на 1 метр
        transform.Translate(Vector3.forward);
        // Вернуть себя обратно родителю
        transform.SetParent(_Parent);

        _TimerBoxes.text = "□□□";
        // Включить себя
        gameObject.SetActive(true);
        StartCoroutine(Wait3Sec());
        
    }

    IEnumerator Wait3Sec()
    {

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1f);
            if (i == 0) _TimerBoxes.text = "■□□";
            else if (i == 1) _TimerBoxes.text = "■■□";
            else if (i == 2) _TimerBoxes.text = "■■■";
        }

        _CameraHeight = _Camera.localPosition.y;
        print("_CameraHeight предварительно = " + _CameraHeight);
        transform.SetParent(null);
        StartCoroutine(Flight());
    }

    IEnumerator Flight()
    { 
        for (int i = 0; i < 40; i++)
        {
            transform.Translate(Vector3.forward * 1);
            _CameraHeight = Mathf.Max(_CameraHeight, _Camera.localPosition.y);
            Vector3 pos = transform.position;
            pos.y = _Camera.position.y;
            transform.position = pos;

            //yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.05f);

        }
        print("_CameraHeight окончательно = " + _CameraHeight);
        transform.SetParent(_Parent);
        gameObject.SetActive(false);
        _Calibrator.ScaleModel(_CameraHeight);
    }

}
