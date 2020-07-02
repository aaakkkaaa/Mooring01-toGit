using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// хранит пути, отдает путь по точкам начала и конца
public class PathManager : MonoBehaviour
{
    public List<Transform> Points;

    public List<Transform> getPath(string start, string finish)
    {
        List<Transform> result = new List<Transform>();

        // если начало и конец совпадают, вернем пустой список
        if (start == finish)
        {
            return result;
        }

        // определим индексы в списке Points начальной и конечной точек маршрута
        int iStart = -1;
        int iFinish = -1;
        for (int i = 0; i < Points.Count; i++)
        {
            if(Points[i].name == start)
            {
                iStart = i;
            }
            if (Points[i].name == finish)
            {
                iFinish = i;
            }
        }

        // если не нашелся какой-то конец маршрута, вернем пустой список
        if(iStart == -1 || iFinish == -1)
        {
            return result;
        }

        // сформируем список из маршрутных точек
        int step = (iStart < iFinish) ? 1 : -1;
        for(int i = iStart; i != iFinish; i += step )
        {
            result.Add(Points[i]);
        }
        result.Add(Points[iFinish]);

        return result;
    }


    /*
    public List<OnePath> AllPathes;


    
    public List<Transform> getPath(string start, string finish)
    {
        List<Transform> result = new List<Transform>();
        
        for(int i=0; i<AllPathes.Count; i++)
        {
            string s1 = AllPathes[i].Points[0].name;
            string s2 = AllPathes[i].Points[AllPathes[i].Points.Count - 1].name;
            if (s1 == start && s2 == finish)
            {
                result = AllPathes[i].Points;
            }
            else if (s1 == finish && s2 == start)
            {
                for(int j = AllPathes[i].Points.Count-1; j>=0; j--)
                {
                    result.Add(AllPathes[i].Points[j]);
                }
            }
        }
        return result;
    }
    */

}

[Serializable]
public class OnePath
{
    public List<Transform> Points;
}


