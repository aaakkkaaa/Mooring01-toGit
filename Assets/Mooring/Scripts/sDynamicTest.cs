using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class sDynamicTest : MonoBehaviour
{

    // Яхта
    [SerializeField]
    Transform _MainShip;

    // Датчик GPS на корме
    [SerializeField]
    Transform _AftGPS;

    // Файл для чтения задания
    [SerializeField]
    string _TaskFile = "MAH00869";

    // Класс управления яхтой
    YachtSolver _YachtSolver;

    // Класс для записи в файл
    sRecord _Record;

    // Класс точного времени
    sTime _Time;

    // Счетчик команд
    int _TaskCounter = -1;

    // Счетчик секунд
    int _SecCounter = 1;

    // Счетчик команд
    bool _RecordSw = true;

    // Параметры начального положения
    struct InitialPars
    {
        public int t; // Начальное время
        public float x; // Начальная координата X
        public float z; // Начальная координата Z
        public float v; // Начальная скорость
        public int a; // Начальное угловое положение
        public float vx; // Начальная поперечная скорость 
    }

    InitialPars _IniPars;

    // Параметры команды управления
    struct CommandPars
    {
        public int time; // Время
        public int revs; // Обороты
        public int rudder; // Угол поворота руля
        public float EnVal; // Мощность двигателя (получаем из оборотов)
        public float StWh; // Угол поворота рулевого колеса, град. (получаем из поворота руля)
    }

    // Массив - все команды
    CommandPars[] _Command;

    // Start is called before the first frame update
    void Start()
    {
        // Класс управления яхтой
        _YachtSolver = _MainShip.GetComponent<YachtSolver>();

        // Класс для записи в файл
        _Record = transform.GetComponent<sRecord>();

        // Класс точного времени
        _Time = transform.GetComponent<sTime>();

        // Открыть файл с заданием и считать все строки в массив
        string[] TaskData = File.ReadAllLines(Path.Combine(_Record.RecDir, _TaskFile + ".txt"));

        // Подготовить массив команд-структур
        _Command = new CommandPars[TaskData.Length - 3];

        // Преобразовать строки в массив команд-структур
        for (int i=1; i < TaskData.Length; i++) // Первая строка - заголовки. Начинаем работать со 2-й
        {
            // Разберем строку на параметры
            string[] CommandData = TaskData[i].Split('\t');
            if (i == 1) // Вторая строка - начальные параметры
            {
                // Преобразуем параметры из текста в числа
                float.TryParse(CommandData[0], out _IniPars.x); // Начальная координата X
                float.TryParse(CommandData[1], out _IniPars.z); // Начальная координата Z
                float.TryParse(CommandData[2], out _IniPars.v); // Начальная скорость
                int.TryParse(CommandData[3], out _IniPars.a); // Начальный угол
                float.TryParse(CommandData[4], out _IniPars.vx);
            }
            else if (i == 2) // Третья строка - второй заголовок. Пропускаем
            {
            }
            else // Остальные строки - команды
            {
                // Подготовим структуру для параметров команды
                CommandPars commPars = new CommandPars();
                // Преобразуем параметры из текста в числа
                int.TryParse(CommandData[0], out commPars.time);
                int.TryParse(CommandData[1], out commPars.revs);
                int.TryParse(CommandData[2], out commPars.rudder);
                // Зафискируем начальное время
                if (i == 3)
                {
                    _IniPars.t = commPars.time;
                }
                // Приведем время относительно начального (+1 секунда)
                commPars.time = commPars.time - _IniPars.t + 1;
                // Преобразуем обороты двигателя (max 2200) в нормированную мощность (+/-1)
                commPars.EnVal = commPars.revs / 2200f;
                // Преобразуем угол поворота руля в угол поворота рулевого колеса)
                commPars.StWh = commPars.rudder * 540 / 35;
                // Запишем структуру в массив команд
                _Command[i - 3] = commPars;
            }
        }

        // Проверим, что получилось
        //print("Нач. время = " + _IniPars.t + " X = " + _IniPars.x + " Z = " + _IniPars.z + " V = " + _IniPars.v + " А = " + _IniPars.a);
        //for (int i = 0; i < _Command.Length; i++)
        //{
        //    print("Время = " + _Command[i].time + " Обороты = " + _Command[i].revs + " Руль = " + _Command[i].rudder + " Мощность = " + _Command[i].EnVal + " Рулевое колесо = " + _Command[i].StWh);
        //}

        // Отключим управление яхтой от штурвала и ручки газ-реверс (на случай, если включено)
        _YachtSolver.GameWheel = false;

        // Запускаем корутину выполнения команд
        StartCoroutine(ExecuteTask());

        // Запускаем корутину записи трека
        StartCoroutine(RecordTrack());
    }

    // Корутина выполнения команд
    IEnumerator ExecuteTask()
    {
        for (int i = 0; i < _Command.Length; i++)
        {
            // Номер текущей команды
            _TaskCounter = i - 1;

            // Ждем до начала выполнения команды (+/- 0.05 сек)
            //print("_Time.CurrentTimeSec() = " + _Time.CurrentTimeSec() + " _Command[i].time = " + _Command[i].time);
            while (_Time.CurrentTimeSec() < _Command[i].time - 0.05f)
            {
                yield return new WaitForSeconds(0.1f);
            }
            // Дождались. Выполняем команду.
            if (i == 0) // Первая команда - устанавливаем начальные положение и скорость
            {
                // Поменяем иерархию
                _AftGPS.parent = null;
                _MainShip.parent = _AftGPS;
                // Поставим яхту в начальное положение
                Vector3 v3 = _AftGPS.position;
                v3.x = _IniPars.x;
                v3.z = _IniPars.z;
                _AftGPS.position = v3;
                // Угол
                v3 = _AftGPS.eulerAngles;
                v3.y = _IniPars.a;
                _AftGPS.eulerAngles = v3;
                // Вернем иерархию обратно
                _MainShip.parent = null;
                _AftGPS.parent = _MainShip;
                // Скорость
                _YachtSolver.Vz = _IniPars.v;
                _YachtSolver.Vx = _IniPars.vx;
            }
            _YachtSolver.engineValue = _Command[i].EnVal;
            _YachtSolver.steeringWheel = _Command[i].StWh;
            print("CurrentTime = " + _Time.CurrentTimeSec() + " CommandTime = " + _Command[i].time + ", " + _Command[i].EnVal + ", " + _Command[i].StWh);

        }

        // Все команды выполнены. Остановка
        _YachtSolver.engineValue = 0;
        _YachtSolver.steeringWheel = 0;
        _RecordSw = false;
    }

    // Запись трека
    IEnumerator RecordTrack()
    {
        _Record.MyLog("Track", "Время\tОбороты\tРуль\tСкорость\tКурс\tX\tY");
        while (_RecordSw)
        {
            while (_Time.CurrentTimeSec() < _SecCounter - 0.01f)
            {
                yield return new WaitForEndOfFrame();
                //yield return new WaitForSeconds(0.1f);
            }
            _SecCounter++;
            if (_TaskCounter == -1)
            {
                _Record.MyLog("Track", "0\t0\t0\t" + _YachtSolver.Vz + "\t" + _AftGPS.eulerAngles.y + "\t" + _AftGPS.position.x + "\t" + _AftGPS.position.z);
            }
            else
            {
                _Record.MyLog("Track", (_SecCounter + _IniPars.t - 2) + "\t" + _Command[_TaskCounter].revs + "\t" + _Command[_TaskCounter].rudder + "\t" + _YachtSolver.Vz + "\t" + _AftGPS.eulerAngles.y + "\t" + _AftGPS.position.x + "\t" + _AftGPS.position.z);
            }
        }
    }

}
