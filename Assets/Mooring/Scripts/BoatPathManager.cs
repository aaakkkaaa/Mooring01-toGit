using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using System;

public class BoatPathManager : MonoBehaviour
{
    // пути
    private string[][] _branches;

    // номер текущего пути
    private int _curBranch;
    // последняя пройденая точка пути
    private int _curPoint;
    // направление движения по массиву точект
    private int _pathDir;
    // путь из имен точек
    List<string> _result;


    void Start()
    {
        _branches = new string[11][];
        _branches[0] = new string[] { "P+2+3", "P+1+3", "P-1+4", "P-0.5-1" };
        _branches[1] = new string[] { "P-0.5-1", "P-1-2.4" };
        _branches[2] = new string[] { "P-0.5-1", "P+0.5-1" };
        _branches[3] = new string[] { "P+0.5-1", "P+0.75-2.3" };
        _branches[4] = new string[] { "P+0.5-1", "P+1-1" };
        _branches[5] = new string[] { "P+1-1", "P+0.8+0.8", "P+0.5+1", "P+0.5+2", "P+1.5+2" };
        _branches[6] = new string[] { "P+1-1", "P+1.5-1" };
        _branches[7] = new string[] { "P+1.5-1", "P+1.9-2.1" };
        _branches[8] = new string[] { "P+1.5-1", "P+2.5-1", "P+2.75-2" };
        _branches[9] = new string[] { "P+1.5-1", "P+2.5-0.5", "P+2.5+0", "P+2.5+1.5", "P+2.8+2.2" };
        _branches[10] = new string[] { "P+1-1", "P+0.8+0.8", "P+1.5+1" };

        //CreatePath("P10");
    }

    public List<string> CreatePath(string cur)
    {
        // так можно задать произвольный конкретный путь
        //return new List<string>(new string[] { "P02", "P02_01", "P02_02", "P02_03" });

        _result = new List<string>();
        _result.Add(cur);
        _curBranch = -1;
        while ( CreateBranch(cur, _curBranch) )
        {
            cur = _result[_result.Count - 1];
        }

        PrintResult(_result);
        return _result;
    }

    // по стартовой точке строит часть пути до пересечения или до конца (в этом случае возвращает false)
    private bool CreateBranch(string cur, int exclude)
    {
        // определим скольким возможным путям соответствует точка cur
        List<int> start = new List<int>();
        for (int i = 0; i < _branches.Length; i++)
        {
            for (int j = 0; j < _branches[i].Length; j++)
            {
                if (cur == _branches[i][j] && exclude != i)
                {
                    start.Add(i);
                }
            }
        }
        print("CreatePath -> " + start.Count);

        // выбираем ветку из числа содержащих начальный узел cur
        _curBranch = -1;
        if (start.Count == 0)
        {
            print("Ошибка построения пути, start.Count == 0, заканчиваем поиск пути");
            return false;
        }
        if (start.Count == 1)
        {
            _curBranch = start[0];
        }
        else
        {
            int num = Mathf.FloorToInt(Random.Range(0, start.Count));
            _curBranch = start[num];
        }
        if (_curBranch == -1)
        {
            print("Ошибка построения пути, _curBranch == -1, заканчиваем поиск пути");
            return false;
        }
        string[] branch = _branches[_curBranch];        // будем двигаться по этой ветке

        // направление движения по ветке - от начала +1, от конца -1
        int idx = System.Array.IndexOf(branch, cur);    // где на этой ветке стартовая точка
        if (idx == 0)
        {
            _pathDir = 1;
        }
        else if (idx == branch.Length - 1)
        {
            _pathDir = -1;
        }
        else
        {
            print("Ошибка построения пути, начальная точка на середине ветки. idx = " + idx);
        }
        // стартовая точка, ветка и направление определено, надо сгенерировать путь до следующего пересечения
        do
        {
            idx += _pathDir;
            _result.Add(branch[idx]);
            for (int i = 0; i < _branches.Length; i++)
            {
                if (i != _curBranch)
                {
                    for (int j = 0; j < _branches[i].Length; j++)
                    {
                        if ( branch[idx] == _branches[i][j] )
                        {
                            return true;    // найдена точка ветвления
                        }
                    }
                }
            }

        } while ( !(idx == 0 || idx == branch.Length-1) );


        return false;   // дошли до конца ветки
    }

    private void PrintResult(List<string> res)
    {
        string str = "";
        for (int i = 0; i < res.Count; i++)
        {
            if (i == res.Count - 1)
            {
                str += res[i];
            }
            else
            {
                str += (res[i] + " - ");
            }
        }
        print(str);
    }


}
