using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;



public class sRecord : MonoBehaviour
// ********************** Запись данных в файлы ********************************************
/*
                   Main     - Файл для записи по умолчанию
*/

{
    // Класс с параметрами для записи логов
    private sGlobalSettings _GlobalS;

    // Папка для записи файлов
    public string RecDir;

    // Класс точного времени
    sTime _Time;

    void OnEnable()
    {
        // Ссылка на класс с параметрами для записи логов
        _GlobalS = GameObject.Find("GlobalS").GetComponent<sGlobalSettings>();

        // Папка для записи файлов
        RecDir = _GlobalS.RecDir;

        // Класс точного времени
        _Time = transform.GetComponent<sTime>();
    }

    // ****************  Перегруженные функции для записи лог-файлов   ********************************
    // Запись в указанный файл
    public void MyLog(string myRecName, String myInfo)
    {
        if (_GlobalS.WriteLog)
        {
            if (_GlobalS.RecFile.ContainsKey(myRecName))
            {
                _GlobalS.RecFile[myRecName].WriteLine(_Time.CurrentTimeMilliSec() + "\t" + myInfo.Replace(".", ","));
            }
        }
    }

    // Запись в файл по умолчанию
    public void MyLog(String myInfo)
    {
        MyLog("Main", myInfo);
    }

    // Запись в файл по умолчанию без автоматической вставки времени
    public void MyLog(String myInfo, bool writeTime)
    {
        if (_GlobalS.WriteLog)
        {
            if (_GlobalS.RecFile.ContainsKey("Main"))
            {
                _GlobalS.RecFile["Main"].WriteLine(myInfo.Replace(".", ","));
            }
        }
    }

    // Запись в два файла
    public void MyLog(string myRecName1, string myRecName2, String myInfo)
    {
        if (_GlobalS.WriteLog)
        {
            int myCurrentTime = _Time.CurrentTimeMilliSec();
            if (_GlobalS.RecFile.ContainsKey(myRecName1))
            {
                _GlobalS.RecFile[myRecName1].WriteLine(myInfo.Replace(".", ",") + " CurrentTime = " + myCurrentTime);
            }
            if (_GlobalS.RecFile.ContainsKey(myRecName2))
            {
                _GlobalS.RecFile[myRecName2].WriteLine(myInfo.Replace(".", ",") + " CurrentTime = " + myCurrentTime);
            }
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
        if (_GlobalS.WriteLog)
        {
            _GlobalS.RecFile[myRecName].Close();
            _GlobalS.RecFile.Remove(myRecName);
        }
    }

    // Закрыть все открытые лог-файлы
    public void CloseAll()
    {
        // Закрыть все открытые лог-файлы
        List<String> myKeys = new List<String>(_GlobalS.RecFile.Keys);
        for (int i = 0; i < myKeys.Count; i++)
        {
            _GlobalS.RecFile[myKeys[i]].Close();
        }
    }

    private void OnApplicationQuit()
    {
        CloseAll();
    }
}
