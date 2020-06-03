using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Obi;

public class Cleat : MonoBehaviour
{
    // список канатов
    public List<Rope> Ropes;

    // рисовать ли канат с помощью гизмо
    public bool drawRope = false;

    // сумма сил на этой утке
    public Vector3 summF = Vector3.zero;

    private void Start()
    {
        if (Ropes == null)
        {
            Ropes = new List<Rope>();
        }
    }

    // рассчет силы от всех канатов приделанных к утке
    public Vector3 getForce()
    {
        summF = Vector3.zero;

        //print("getForce() -> Утка " + name);

        // по всем канатам этой утки:
        for (int i = 0; i < Ropes.Count; i++)
        {
            if ( (Ropes[i].obiRope == null || !Ropes[i].obiRope.isActiveAndEnabled)
                && ( Ropes[i].ropeTrick == null || !Ropes[i].ropeTrick.isActiveAndEnabled) )
            {
                continue;
            }

            // от утки на яхте до точки закрепления на берегу
            float curDist = Vector3.Distance(transform.position, Ropes[i].Bollard.position);
            // направление силы на одном канате
            Vector3 direct = Vector3.zero;
            // величина силы на одном канате
            float valueF = 0;                       

            // проверка, не лопнул ли канат
            if (curDist / Ropes[i].Len > Ropes[i].Stretch)
            {
                print("curDist = " + curDist + "  Ropes[i].Len = " + Ropes[i].Len + "   Ropes[i].Stretch = " + Ropes[i].Stretch);
                print("Канат лопнул! Утка " + gameObject.name + "   канат " + i);
                HideRope(i);
            }
            else
            {
                // если не лопнул, считаем силу на этом канате
                if (curDist > Ropes[i].Len) // имеется натяжение?
                {
                    valueF = (curDist / Ropes[i].Len - 1) / (Ropes[i].Stretch - 1) * Ropes[i].MaxForce;
                    direct = (Ropes[i].Bollard.position - transform.position).normalized;

                   
                }
            }
            Ropes[i].curForce = direct * valueF;
            summF += Ropes[i].curForce;
        }

        return summF;
    }

    private void HideRope( int i )
    {
        if(Ropes[i].obiRope != null)
        {
            Ropes[i].obiRope.gameObject.SetActive(false);
        }
        if(Ropes[i].ropeTrick != null)
        {
            Ropes[i].ropeTrick.gameObject.SetActive(false);
        }
        
    }



    // отладочное рисование каната в виде линии
    private void OnDrawGizmos()
    {
        if (Ropes == null) return;
        
        if (drawRope)
        {
            for (int i = 0; i < Ropes.Count; i++)
            {
                Gizmos.color = detectRopeColor(Ropes[i]);
                Gizmos.DrawLine(transform.position, Ropes[i].Bollard.position);
            }
        }
    }

    // определение цвета для рисования гизмо
    private Color detectRopeColor(Rope r)
    {
        float curDist = Vector3.Distance(transform.position, r.Bollard.position);
        Color col = Color.black;
        if (curDist < r.Len)
        {
            // канат не натянут
            col = Color.black;
        }
        else if (curDist < r.Len * r.Stretch)
        {
            // растяжение в допустимых пределах 
            float blueAdd = 0;
            
            blueAdd = r.curForce.magnitude/r.MaxForce * 0.5f;
            col = new Color(0.5f, 0.5f + blueAdd, 0.5f);
        }
        else
        {
            // превышено максимальное растяжение
            col = new Color(1.0f, 0.5f, 0.5f);
        }
        return col;
    }
}

public class Rope
{
    public Transform Bollard;
    //public Vector3 Pos;
    public float Len;
    public float Stretch;
    public float MaxForce;
    // Канат привязанный к утке (непонятно, как учесть, что это могут быть два конца одного каната)
    public ObiRope obiRope;
    public RopeTrick ropeTrick;
    // для рисования гизмо
    public Vector3 curForce=Vector3.zero;
}

