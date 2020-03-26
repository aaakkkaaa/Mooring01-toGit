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

    // коллайдеры объектов, принадлежащих яхте
    [NonSerialized]
    public Collider[] yachtCols;

    private void Start()
    {
        for (int i = 0; i < Ropes.Count; i++)
        {
            Ropes[i].actCol = new List<ActiveCollider>();
        }

    }

    public void SolveAllRopes()
    {
        summF = Vector3.zero;

        //print("Утка " + name);

        // по всем канатам этой утки:
        for (int i = 0; i < Ropes.Count; i++)
        {
            if (!Ropes[i].obiRope.isActiveAndEnabled) continue;

            Vector3 direct = Vector3.zero;          // направление силы на одном канате
            float valueF = 0;                       // величина силы на одном канате
            float curDist = 0;                      // от утки до точки закрепления с учетом изгибов

            // сортируем точки коллайдинга
            var sortedCol = from c in Ropes[i].actCol orderby c.idxInAct select c;
            List<ActiveCollider> sortCol = new List<ActiveCollider>(sortedCol);

            // вычислим текущую длинну каната - она состоит из длинн отрезков 
            if(sortCol.Count == 0)
            {
                curDist = Vector3.Distance(transform.position, Ropes[i].Pos);
            }
            else
            {
                curDist = Vector3.Distance(transform.position, sortCol[0].pos);
                for(int j=0; j < sortCol.Count-1; j++)
                {
                    curDist += Vector3.Distance(sortCol[j].pos, sortCol[j+1].pos);
                }
                curDist += Vector3.Distance(sortCol[sortCol.Count - 1].pos, Ropes[i].Pos);
            }

            foreach (ActiveCollider ac in sortCol)
            {
                print(ac.actor.name + "  " + ac.idxInAct + "  " + ac.pos + "  " + ac.col.name + "  curDist = " + curDist);
            }

            direct = Vector3.zero;
            // проверка, не лопнул ли канат
            if (curDist / Ropes[i].Len > Ropes[i].Stretch)
            {
                print("Канат лопнул! Утка " + gameObject.name + "   канат " + i);
                Ropes[i].obiRope.gameObject.SetActive(false);
            }
            else
            {
                // если не лопнул, считаем силу на этом канате
                valueF=0;
                if (curDist > Ropes[i].Len) // имеется натяжение?
                {
                    valueF = (curDist / Ropes[i].Len - 1) / (Ropes[i].Stretch - 1) * Ropes[i].MaxForce;


                    // определяем направление силы
                    if (sortCol.Count == 0)
                    {
                        // если нет коллайдеров на канате, направление - от утки к точке закрепления
                        direct = (Ropes[i].Pos - transform.position).normalized;
                    }
                    else
                    {
                        // если еть коллайдеры на канате, надо найти первый коллайдер не принадлежащий яхте (временно!)
                        ActiveCollider ac = null;
                        for (int j = 0; j < sortCol.Count; j++)
                        {
                            if (FindExternalCol(sortCol[j]))
                            {
                                ac = sortCol[j];
                                break;
                            }
                        }
                        if (ac == null)
                        {
                            // если коллайдеры только с корпусом яхты, направление - от утки к точке закрепления (временно!)
                            direct = (Ropes[i].Pos - transform.position).normalized;
                        }
                        else
                        {
                            direct = (ac.pos - transform.position).normalized;
                        }
                    }
                }
            }
            Ropes[i].curForce = direct * valueF;
            summF += Ropes[i].curForce;
        }
       
    }

    // проверить, что ac контактирует с внешним коллайдером
    private bool FindExternalCol(ActiveCollider ac)
    {
        for(int i=0; i < yachtCols.Length; i++)
        {
            if(ac.col == yachtCols[i])
            {
                return false;
            }
        }
        return true;
    }

    public Vector3 getForce()
    {
        return summF;
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
                if (Ropes[i].actCol == null) return;
                if(Ropes[i].actCol.Count == 0)
                {
                    Gizmos.DrawLine(transform.position, Ropes[i].Pos);
                }
                else
                {
                    Gizmos.DrawLine(transform.position, Ropes[i].actCol[0].pos);
                    for (int j = 1; j < Ropes[i].actCol.Count; j++)
                    {
                        Gizmos.DrawLine(Ropes[i].actCol[j-1].pos, Ropes[i].actCol[j].pos);
                    }
                    Gizmos.DrawLine(Ropes[i].actCol[Ropes[i].actCol.Count-1].pos, Ropes[i].Pos);
                }

            }
        }
    }

    // определение цвета для рисования гизмо
    private Color detectRopeColor(Rope r)
    {
        float curDist = Vector3.Distance(transform.position, r.Pos);
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



[Serializable]
public class Rope
{
    public Vector3 Pos;
    // столкновения, зафиксированные на этом канате, передается из DetectCol
    [NonSerialized]
    public List<ActiveCollider> actCol;
    public float Len;
    public float Stretch;
    public float MaxForce;
    // Канат привязанный к утке (непонятно, как учесть, что это могут быть два конца одного каната)
    public ObiRope obiRope;

    [NonSerialized]
    public Vector3 curForce;
}

