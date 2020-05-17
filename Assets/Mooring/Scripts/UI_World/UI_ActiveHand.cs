using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Сажается на объект с коллайдером, привязанный к руке
// При взаимодействиях с коллайдером Диалога вызывает его функции
public class UI_ActiveHand : MonoBehaviour
{
    [SerializeField]
    UI_Dialog _UI_Dialog;

    void OnTriggerEnter(Collider Coll)
    {
        //print("OnTriggerEnter: Коллайдер " + Coll.transform.name + " - вход");
        if (Coll.transform.name.Substring(0, 4) == "Butt")
        {
            _UI_Dialog.myTriggerEnter(Coll, transform);
        }
    }

    void OnTriggerExit(Collider Coll)
    {
        //print("Коллайдер " + Coll.transform.name + " - выход");
        if (Coll.transform.name.Substring(0, 4) == "Butt")
        {
            _UI_Dialog.myTriggerExit(Coll, transform);
        }
    }

}
