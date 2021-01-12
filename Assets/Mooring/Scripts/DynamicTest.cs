using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class DynamicTest : MonoBehaviour
{

    // Яхта
    [SerializeField]
    Transform _MainShip;

    // Датчик GPS на корме
    [SerializeField]
    Transform _AftGPS;

    // Файл для чтения задания и чтения трека
    [SerializeField]
    string _NumFile = "00869";

    // объект для рисования пути
    [SerializeField]
    GraphDraw _GraphXY;
    // объект для рисования скорости
    [SerializeField]
    GraphDraw _GraphVz;

    // Глобальные параметры
    sGlobalSettings _GlobalSettings;

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

    // Шаг моделирования
    private float _dT = 0.02f;

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
    //CommandPars[] _Command;
    List<CommandPars> _Command;

    // Данные трекинга
    List<Vector2> _treckPos;
    List<Vector2> _treckV;

    // Start is called before the first frame update
    void Start()
    {
        // Глобальные параметры
        _GlobalSettings = GameObject.Find("GlobalS").GetComponent<sGlobalSettings>();

        // Класс управления яхтой
        _YachtSolver = _MainShip.GetComponent<YachtSolver>();

        // Класс для записи в файл
        _Record = transform.GetComponent<sRecord>();

        // Класс точного времени
        _Time = transform.GetComponent<sTime>();

        // Проверим существование файла с треком и считаем его если он есть
        string gpsFile = "GPS" + _NumFile + ".txt";
        if (File.Exists(Path.Combine(_GlobalSettings.RecDir, gpsFile)))
        {
            string[] gpsData = File.ReadAllLines(Path.Combine(_GlobalSettings.RecDir, gpsFile));
            // разбор и запись трека в списки _treckPos и _treckV
            int gpsT;
            int shiftT=0;
            float gpsX, gpsY;
            _treckPos = new List<Vector2>();
            _treckV = new List<Vector2>();
            for (int i = 1; i < gpsData.Length; i++)    // Первая строка - заголовки. Начинаем работать со 2-й
            {
                print("GPS: " + gpsData[i]);
                string[] oneData = gpsData[i].Split('\t');
                int.TryParse(oneData[0], out gpsT);
                float.TryParse(oneData[1], out gpsX);
                float.TryParse(oneData[2], out gpsY);
                Vector2 xy = new Vector2(gpsX, gpsY);
                _treckPos.Add(xy);
                if(i==1)    // в первой строке начальное время, нужно будет его вычитать из всех времен
                {
                    shiftT = gpsT;
                }
                else
                {
                    // для всех точек кроме первой вычисляем скорость
                    float curT = gpsT - shiftT;
                    float dT;
                    if ( i == 2 )
                    {
                        dT = curT;
                    }
                    else
                    {
                        dT = curT - _treckV[_treckV.Count-1].x;
                    }
                    float curV = (_treckPos[_treckPos.Count-1] - _treckPos[_treckPos.Count - 2]).magnitude / dT;
                    Vector2 tv = new Vector2(curT, curV);
                    _treckV.Add(tv);
                }
            }
            _GraphXY.AddData(_treckPos, Color.blue);
            _GraphVz.AddData(_treckV, Color.blue);
        }

        // Открыть файл с заданием и считать все строки в массив
        string taskFile = "MAH" + _NumFile + ".txt";
        string[] TaskData = File.ReadAllLines(Path.Combine(_GlobalSettings.RecDir, taskFile ));

        // Подготовить массив команд-структур
        //_Command = new CommandPars[TaskData.Length - 3];
        _Command = new List<CommandPars>();

        // Преобразовать строки в массив команд-структур
        for (int i = 1; i < TaskData.Length; i++) // Первая строка - заголовки. Начинаем работать со 2-й
        {
            // Разберем строку на параметры
            //print(TaskData[i]);
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
                //_Command[i - 3] = commPars;
                _Command.Add(commPars);
            }
        }

        // Проверим, что получилось
        print("Нач. время = " + _IniPars.t + " X = " + _IniPars.x + " Z = " + _IniPars.z + " V = " + _IniPars.v + " А = " + _IniPars.a);
        //for (int i = 0; i < _Command.Length; i++)
        //{
        //    print("Время = " + _Command[i].time + " Обороты = " + _Command[i].revs + " Руль = " + _Command[i].rudder + " Мощность = " + _Command[i].EnVal + " Рулевое колесо = " + _Command[i].StWh);
        //}

        // Отключим управление яхтой от штурвала и ручки газ-реверс (на случай, если включено)
        _YachtSolver.GameWheel = false;


        StartCoroutine(Modeling());

    }

    // Корутина выполнения моделирования после задержки
    IEnumerator Modeling()
    {
        yield return new WaitForSeconds(0.3f);

        _Record.MyLog("Track", "Время\tОбороты\tРуль\tСкорость\tКурс\tX\tY");

        //устанавливаем начальные положение и скорость
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

        float curTime = 0;
        int curCummand = 0;
        const float WritePause = 0.2f;
        float pauseToWrite = WritePause;

        // для запоминания положения и скорости
        List<Vector2> positions = new List<Vector2>();
        List<Vector2> velocity = new List<Vector2>();

        print("_Command.Length = " + _Command.Count);
        print("_Command[0].time = " + _Command[0].time);
        print("_Command[_Command.Length - 1].time = " + _Command[_Command.Count - 1].time);

        // моделирование:
        while (curTime < _Command[_Command.Count - 1].time + 20)
        {
            curTime += _dT;
            pauseToWrite -= _dT;
            if ( curCummand < _Command.Count && curTime > _Command[curCummand].time)
            {
                _YachtSolver.engineValue = _Command[curCummand].EnVal;
                _YachtSolver.steeringWheel = _Command[curCummand].StWh;
                //print("CurrentTime = " + curTime + " CommandTime = " + _Command[curCummand].time + ", " + _Command[curCummand].EnVal + ", " + _Command[curCummand].StWh);
                curCummand++;
            }

            _YachtSolver.Solve(_dT);

            if(curCummand < _Command.Count && pauseToWrite <= 0 )
            {
                _Record.MyLog("Track", curTime + "\t" + _Command[curCummand].revs + "\t" + _Command[curCummand].rudder + "\t" + _YachtSolver.Vz + "\t" + _AftGPS.eulerAngles.y + "\t" + _AftGPS.position.x + "\t" + _AftGPS.position.z);

                Vector2 pos = new Vector2(_AftGPS.position.x, _AftGPS.position.z);
                //print(_AftGPS.position.x + "   " + _AftGPS.position.z);
                positions.Add(pos);
                Vector2 vel = new Vector2(curTime, _YachtSolver.Vz);
                velocity.Add(vel);

                pauseToWrite = WritePause;
            }
        }
        print("Моделирование закончено");

        _GraphXY.AddData(positions, Color.red);
        _GraphVz.AddData(velocity, Color.red);

        yield return new WaitForSeconds(0.1f);
        _GraphXY.DrawAll();
        _GraphVz.DrawAll();
    }


}
