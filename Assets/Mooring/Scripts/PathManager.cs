using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// хранит пути, отдает путь по точкам начала и конца
public class PathManager : MonoBehaviour
{
    /*
    private Dictionary<string, string[]> AllPathes = new Dictionary<string, string[]>()
    {
        ["111"] = new string[]{ "1", "2" },
    };
    */
    /*
    // пути заданы именами объектов
    private string[][] AllPathes =
    {
        new string[]{ "P_01","P_02","P_03","P_04","P_05","P_06","P_07","P_08","P_09",}, // левый, кормовая утка - носовая утка
        new string[]{ "P_01","P_10", "P_11" }   // левый, кормовая утка - кранец на корме
    };
    */

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

}

[Serializable]
public class OnePath
{
    public List<Transform> Points;
}


