using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class sGlobalSettings : MonoBehaviour
{

    // ********************** Переменные для записи данных в файлы ********************************************

    [Header("Запись логов:")]

    // Папка для записи файлов
    public string RecDir = "Record";

    // Список файлов для записи логов
    [SerializeField]
    private string[] _RecFileNames = new string[] { "Main" };

    // Main - файл для записи по умолчанию
    // Примеры других файлов:
    // Track - файл для записи трека
    // Thread - файл для записи в фоновом потоке

    // Словарь - массив файлов для записи данных. Ключ - имя файла, значение - объект StreamWriter
    public Dictionary<String, StreamWriter> RecFile = new Dictionary<String, StreamWriter>();

    // Отладочный параметр - запиcывать ли логи
    public bool WriteLog = false;

    // ********************** Другое ********************************************

    //[Header("Другое")]



    private void Awake()
    {
        // Инициализируем генератор случайных чисел UnityEngine.Random
        // используется в BirdsAttractor.cs

        TimeSpan CurTime = DateTime.Now.TimeOfDay; // текущее время
        int Seed = (int)CurTime.TotalSeconds - (int)CurTime.TotalHours * 3600; // секунды с начала последнего часа
        UnityEngine.Random.InitState(Seed);
    }

    // Start is called before the first frame update
    void Start()
    {
        // ********************** Подготовится к записи данных в файлы ********************************************

        // Создать папку
        Directory.CreateDirectory(RecDir);
        RecDir = Path.Combine(Directory.GetCurrentDirectory(), RecDir);

        if (WriteLog)
        {
            // Создать папку
            Directory.CreateDirectory(RecDir);
            RecDir = Path.Combine(Directory.GetCurrentDirectory(), RecDir);

            // Заполнить словарь RecFile из массива RecFileNames (и сразу создать/пересоздать файлы для записи)
            for (int i=0; i<_RecFileNames.Length; i++)
            {
                AddToDic(_RecFileNames[i]);
            }
        }
    }


    // Добавить в словарь имя файла и созданный объект StreamWriter
    void AddToDic(String myRecFileName)
    {
        /*
        // TODO надо проверять, и если файл уже открыт то .....
        if (RecFile.ContainsKey(myRecFileName))
        {
            StreamWriter sw = RecFile[myRecFileName];
            sw.Close();
            sw.Dispose();
            RecFile.Remove(myRecFileName);
        }
        */
        RecFile.Add(myRecFileName, new StreamWriter(Path.Combine(RecDir, myRecFileName + ".txt")));
    }


}
