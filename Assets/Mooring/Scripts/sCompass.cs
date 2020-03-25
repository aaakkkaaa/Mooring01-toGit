using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sCompass : MonoBehaviour
{

    // Коррекция ориентации модели
    [SerializeField]
    private float _ModelNorthCorrection = 180.0f;

    // Фиксированная ориентация модели
    private Vector3 _FixEu;

    // Start is called before the first frame update
    void Start()
    {
        // Фиксированная ориентация модели
        _FixEu = new Vector3(0.0f, _ModelNorthCorrection, 0.0f);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Поддержание фиксированной ориентация модели
        transform.eulerAngles = _FixEu;
    }
}
