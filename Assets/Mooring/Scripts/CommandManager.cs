using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public string[] Commands = { "ПОДАТЬ ШВАРТОВЫ", "ВЫВЕСИТЬ КРАНЦЫ", "НА НОС" };
    public string CurCommand;

    public Sailor Sailor1;
    public Sailor Sailor2;
    public Marinero Marinero;

    public RopeController LeftAft;
    public RopeController RightAft;

    private void Update()
    {
        if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
        {
            if (Input.GetKeyDown("t")) // подать команду "ПОДАТЬ ШВАРТОВЫ"
            {
                if (Sailor1 != null)
                {
                    if (Sailor1.CurCommand == "")
                    {
                        Sailor1.CurCommand = "ПОДАТЬ ШВАРТОВЫ";
                    }
                }
            }

            if (Input.GetKeyDown("g")) // подать команду "НА НОС"
            {
                if (Sailor1 != null)
                {
                    if (Sailor1.CurCommand == "")
                    {
                        Sailor1.CurCommand = "НА НОС";
                    }
                }
            }
        }


    }
}
