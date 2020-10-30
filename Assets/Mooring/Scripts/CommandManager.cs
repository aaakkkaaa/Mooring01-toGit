using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public string[] Commands = { "ПОДАТЬ ШВАРТОВЫ", "ВЫВЕСИТЬ КРАНЦЫ", "НА НОС" };
    public string CurCommand;

    public GameObject Sailor1;
    public GameObject Sailor2;

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
                    Sailor s1 = Sailor1.GetComponent<Sailor>();
                    if (s1.CurCommand == "")
                    {
                        s1.CurCommand = "ПОДАТЬ ШВАРТОВЫ";
                    }
                }
            }

            if (Input.GetKeyDown("g")) // подать команду "НА НОС"
            {
                if (Sailor1 != null)
                {
                    Sailor s1 = Sailor1.GetComponent<Sailor>();
                    if (s1.CurCommand == "")
                    {
                        s1.CurCommand = "НА НОС";
                    }
                }
            }
        }


    }
}
