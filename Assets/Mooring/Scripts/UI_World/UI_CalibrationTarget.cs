using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class UI_CalibrationTarget : MonoBehaviour
{
    // Трансформ объекта для визуализации мишени
    [SerializeField]
    Transform _TargetVisual;

    // Трансформ объекта для отображения течения времени захвата мишени 
    [SerializeField]
    Transform _TargetPlay;

    // Мвтериал для раскраски захваченной мишени
    [SerializeField]
    Material _TargetCapturedMat;

    // Материал для раскраски незахваченной мишени
    Material _TargetFreeMat;

    // Коллайдер прицела
    [SerializeField]
    Collider _AimCollider;

    // Размеры объекта для визуализации
    float _VisualScaleXZ;
    float _VisualScaleY;

    // Начальный размер объекта отображения времени захвата
    Vector3 _PlayScale0 = Vector3.zero;
    float _PlayScaleY;

    // Время начала захвата мишени
    float _CaptureStartTime = 0.0f;

    // Класс для диалога калибровки
    UI_CalibrationDialog _CalibrationDialog;

    // Start is called before the first frame update
    void Start()
    {
        // Запомним материал для раскраски незахваченной мишени
        _TargetFreeMat = _TargetVisual.GetComponent<MeshRenderer>().material;
        // Размеры объекта для визуализации
        _VisualScaleXZ = _TargetVisual.localScale.x;
        _VisualScaleY = _TargetVisual.localScale.y;

        // Начальный размер объекта отображения времени захвата
        _PlayScaleY = _TargetPlay.localScale.y;
        _PlayScale0.y = _PlayScaleY;

        _TargetPlay.localScale = _PlayScale0;

        // Класс для диалога калибровки
        _CalibrationDialog = transform.parent.GetComponent<UI_CalibrationDialog>();
    }

    // Захват мишени
    void OnTriggerEnter(Collider Coll)
    {
        //print("OnTriggerEnter: Коллайдер " + Coll.transform.name + " - вход");
        if (Coll == _AimCollider)
        {
            // Поменять материал раскраски при захвате
            _TargetVisual.GetComponent<MeshRenderer>().material = _TargetCapturedMat;
            // Начальный размер объекта отображения времени захвата
            _TargetPlay.localScale = _PlayScale0;

            // Время начала захвата и время, до которого нужно продержать захват
            _CaptureStartTime = Time.time;

            // Запустить корутину отображения течения времени захвата мишени
            StartCoroutine(RefreshCapture());
        }
    }

    // Потеряли захват мишени
    void OnTriggerExit(Collider Coll)
    {
        //print("Коллайдер " + Coll.transform.name + " - выход");
        if (Coll == _AimCollider)
        {
            // Остановить корутину отображения течения времени захвата мишени
            StopCoroutine(RefreshCapture());
            // Обнулить время начала захвата и время, до которого нужно продержать захват
            _CaptureStartTime = 0.0f;
            // Вернуть материал раскраски мишени при потере захвата
            _TargetVisual.GetComponent<MeshRenderer>().material = _TargetFreeMat;
            // Начальный размер объекта отображения времени захвата
            _TargetPlay.localScale = _PlayScale0;
        }
    }

    // Корутина отображения течения времени захвата мишени
    IEnumerator RefreshCapture()
    {
        WaitForSeconds pause = new WaitForSeconds(0.05f);

        yield return pause;


        while (_CaptureStartTime > 0.0f)
        {
            float t = (Time.time - _CaptureStartTime) / _CalibrationDialog.CaptureTime;
            float playScaleXZ = Mathf.Lerp(0, _VisualScaleXZ, t);
            _TargetPlay.localScale = new Vector3(playScaleXZ, _PlayScaleY, playScaleXZ);
            // Заданное время захвата мишени (CalibrationDialog.CaptureTime) достигнуто
            if (t >= 1.0f)
            {
                // Начальный размер объекта отображения времени захвата
                _TargetPlay.localScale = _PlayScale0;
                // Начать калибровку
                _CalibrationDialog.StartCalibration(Camera.main.transform.localPosition.y);
                break;
            }
            yield return pause;
        }
    }
}
