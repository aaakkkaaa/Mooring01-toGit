using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdsAttractor : MonoBehaviour
{
    // будет считываться птицами привязанными к этому аттрактору
    [NonSerialized] public Vector3 POS = Vector3.zero; 

    [Header("Attractor")]
    public float radiusX = 200;
    public float radiusY = 30;
    public float radiusZ = 200;
    public float xPhase = 0.5f;
    public float yPhase = 0.4f;
    public float zPhase = 0.1f;

    [Header("Birds")]
    public GameObject birdPrefab;
    public int numBoids = 3;
    // в этом радиусе происходит начальная расстановка
    public float spawnRadius = 30f;
    public float velocity = 10f;
    public float neighborDist = 30f;
    public float collDist = 4f;
    public float velMatching = 0.25f;
    public float flockCentering = 0.2f;
    public float collAvoid = 2f;
    public float attractPull = 2f;
    public float attractPush = 2f;
    public float attractPushDist = 5f;

    private Vector3 _basePose;

    // Крики птиц
    [Header("Bird's Calls")]

    // Минимальный и максимальный интералы между криками (сек.)
    [SerializeField]
    private int _minMute = 30;
    [SerializeField]
    private int _maxMute = 60;
    // Аудио источник - крик птиц
    private AudioSource _BirdCall;

    private void Start()
    {
        _basePose = transform.position;
        xPhase = UnityEngine.Random.Range(0.3f, 0.6f);
        yPhase = UnityEngine.Random.Range(0.3f, 0.6f);
        numBoids = Mathf.FloorToInt( UnityEngine.Random.Range(2.1f, 4.0f) );
        // генерируем птиц
        for (int i = 0; i < numBoids; i++)
        {
            GameObject go = Instantiate(birdPrefab);
            go.transform.SetParent(transform.parent);
            Bird bird = go.GetComponent<Bird>();
            bird.attractor = this;
            bird.Init();
        }

        // Крики птиц
        // Аудио источник - крик птиц (находится на текущем GameObject)
        _BirdCall = GetComponent<AudioSource>();
        // Запускаем корутину криков птиц
        StartCoroutine(BirdCall());
    }

    void FixedUpdate()
    {
        Vector3 tPos = Vector3.zero;
        tPos.x = Mathf.Sin(xPhase * Time.time/10) * radiusX + _basePose.x;
        tPos.y = Mathf.Sin(yPhase * Time.time/10) * radiusY + _basePose.y;
        tPos.z = Mathf.Sin(zPhase * Time.time/10) * radiusZ + _basePose.z;
        transform.position = tPos;
        POS = tPos;
    }

    // Крики птиц
    // Воспроизводим через случайные интервалы времени от _minMute до _maxMute в секундах
    IEnumerator BirdCall()
    {
        // Сначала пауза: от 5 секунд до _maxMute
        yield return new WaitForSeconds(UnityEngine.Random.Range(5, _maxMute));
        _BirdCall.Play();

        // Воспроизводим 
        // Осторожно: "бесконечный" цикл
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(_minMute, _maxMute));
            _BirdCall.Play();
        }
    }

}
