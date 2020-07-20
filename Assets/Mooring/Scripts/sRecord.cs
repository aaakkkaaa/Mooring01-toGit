using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;



public class sRecord : MonoBehaviour
// ********************** Запись данных в файлы ********************************************
{

    // Папка для записи файлов
    public String RecDir = "Record";
    // Словарь - массив файлов для записи данных. Ключ - имя файла, значение - объект StreamWriter
    Dictionary<String, StreamWriter> _RecFile = new Dictionary<String, StreamWriter>();
    /*
    Main     - Файл для записи по умолчанию
    */


    // Отладочный параметр - запиcывать ли логи
    [SerializeField]
    bool _WriteLog = true;

    // Класс точного времени
    sTime _Time;

    //// Параметры точного времени
    //Stopwatch _StopWatch; // таймер точного времени
    //private long StartTime; // время начала работы программы

    void Awake()
    {

        //// Запуск таймера точного времени
        //_StopWatch = new Stopwatch();
        //_StopWatch.Start();
        //// Время начала работы программы
        //StartTime = _StopWatch.ElapsedMilliseconds;


        // ********************** Запись данных в файлы ********************************************

        // Создать папку
        Directory.CreateDirectory(RecDir);
        RecDir = Path.Combine(Directory.GetCurrentDirectory(), RecDir);

        if (_WriteLog)
        {
            // Файл для записи по умолчанию
            AddToDic("Main");
            // Файл для записи трека
            AddToDic("Track");
            //// Файл для записи в фоновом потоке
            //AddToDic("Thread");
        }

        // Класс точного времени
        _Time = transform.GetComponent<sTime>();
    }

    // Добавить в словарь имя файла и созданный объект StreamWriter
    void AddToDic(String myRecFileName)
    {
        // TODO надо проверять, и если файл уже открыт то .....
        _RecFile.Add(myRecFileName, new StreamWriter(Path.Combine(RecDir, myRecFileName + ".txt")));
    }


    // ****************  Перегруженные функции для записи лог-файлов   ********************************
    // Запись в указанный файл
    public void MyLog(string myRecName, String myInfo)
    {
        if (_WriteLog)
        {
            _RecFile[myRecName].WriteLine(_Time.CurrentTimeMilliSec() + "\t" + myInfo.Replace(".", ","));
        }
    }

    // Запись в файл по умолчанию
    public void MyLog(String myInfo)
    {
        if (_WriteLog)
        {
            _RecFile["Main"].WriteLine(_Time.CurrentTimeMilliSec() + "\t" + myInfo.Replace(".", ","));
        }
    }

    // Запись в файл по умолчанию без автоматической вставки времени
    public void MyLog(String myInfo, bool writeTime)
    {
        if (_WriteLog)
        {
            _RecFile["Main"].WriteLine(myInfo.Replace(".", ","));
        }
    }

    // Запись в два файла
    public void MyLog(string myRecName1, string myRecName2, String myInfo)
    {
        if (_WriteLog)
        {
            int myCurrentTime = _Time.CurrentTimeMilliSec();
            _RecFile[myRecName1].WriteLine(myInfo.Replace(".", ",") + " CurrentTime = " + myCurrentTime);
            _RecFile[myRecName2].WriteLine(myInfo.Replace(".", ",") + " CurrentTime = " + myCurrentTime);
        }
    }


    // Преобразование координат и углов для вывода в файл
    public string tab(Vector3 myVector, string format)
    {
        return (myVector.x.ToString(format) + "\t" + myVector.y.ToString(format) + "\t" + myVector.z.ToString(format)).Replace(".", ",");
    }
    // То же для 2-мерных векторов
    public string tab(Vector2 myVector, string format)
    {
        return (myVector.x.ToString(format) + "\t" + myVector.y.ToString(format)).Replace(".", ",");
    }

    // ******************************************************************

    // Закрыть один лог-файл и удалить его запись из словаря лог-файлов
    public void Close(string myRecName)
    {
        if (_WriteLog)
        {
            _RecFile[myRecName].Close();
            _RecFile.Remove(myRecName);
        }
    }

    // Закрыть все открытые лог-файлы
    public void CloseAll()
    {
        // Закрыть все открытые лог-файлы
        List<String> myKeys = new List<String>(_RecFile.Keys);
        for (int i = 0; i < myKeys.Count; i++)
        {
            _RecFile[myKeys[i]].Close();
        }
    }

    private void OnApplicationQuit()
    {
        CloseAll();
    }
}
