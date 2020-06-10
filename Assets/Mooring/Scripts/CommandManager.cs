using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public string[] Commands = { "ПОДАТЬ ШВАРТОВЫ", "ВЫВЕСИТЬ КРАНЦЫ" };
    public string CurCommand;

    public GameObject Sailor1;
    public GameObject Sailor2;

    public RopeController LeftAft;
    public RopeController RightAft;

    private void Update()
    {
        
    }
}
