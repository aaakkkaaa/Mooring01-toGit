using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;


/*
 * Временный скрипт для отладки швартовых операций с канатами
 * 
 */
public class sBerthingDraft : MonoBehaviour
{
    [SerializeField]
    AnimatorController _Throw;
    [SerializeField]
    AnimatorController _Catch;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
        {
            if (Input.GetKeyDown("t")) // Поднять и бросить конец
            {
                GetComponent<Animator>().runtimeAnimatorController = _Throw;
            }
            else if (Input.GetKeyDown("c")) // Поймать конец и начать его сматывать
            {
                GetComponent<Animator>().runtimeAnimatorController = _Catch;
            }
        }

    }

    public void myMessage()
    {
        print("My Message !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }

}
