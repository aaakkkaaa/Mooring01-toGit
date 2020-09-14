using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    [NonSerialized] public BirdsAttractor attractor;

    [Header("Set Dynamically")]
    public Rigidbody rigid;

    private Neighborhood neighborhood;

    public void Init()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();
        Vector3 pos = UnityEngine.Random.insideUnitSphere * attractor.spawnRadius;
        Vector3 vel = UnityEngine.Random.onUnitSphere * attractor.velocity;
        rigid.velocity = vel;
        LookAhead();
    }

    void LookAhead()
    {
        // Ориентировать птицу клювом в направлении полета
        transform.LookAt(pos + rigid.velocity);
    }

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    void FixedUpdate()
    {
        Vector3 vel = rigid.velocity;

        // ПРЕДОТВРАЩЕНИЕ СТОЛКНОВЕНИЙ - избегать близких соседей
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = neighborhood.avgClosePos;
        // Если получен вектор Vector3.zero, ничего предпринимать не надо
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= attractor.velocity;
        }

        // СОГЛАСОВАНИЕ СКОРОСТИ - попробовать согласовать скорость с соседями
        Vector3 velAlign = neighborhood.avgVel;
        // Согласование требуется, только если velAlign не равно Vector3.zero
        if (velAlign != Vector3.zero)
        {
            // Нас интересует только направление, поэтому нормализуем скорость
            velAlign.Normalize();
            // и затем преобразуем в выбранную скорость
            velAlign *= attractor.velocity;
        }

        // КОНЦЕНТРАЦИЯ СОСЕДЕЙ - движение в сторону центра группы соседей
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= attractor.velocity;
        }

        // ПРИТЯЖЕНИЕ - организовать движение в сторону объекта Attractor
        Vector3 delta = attractor.POS - pos;
        // Проверить, куда двигаться, в сторону Attractor или от него
        bool attracted = (delta.magnitude > attractor.attractPushDist);
        Vector3 velAttract = delta.normalized * attractor.velocity;
        // Применить все скорости
        float fdt = Time.fixedDeltaTime;
        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, attractor.collAvoid * fdt);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, attractor.velMatching * fdt);
            }
            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, attractor.flockCentering * fdt);
            }
            if (velAttract != Vector3.zero)
            {
                if (attracted)
                {
                    vel = Vector3.Lerp(vel, velAttract, attractor.attractPull * fdt);
                }
                else
                {
                    vel = Vector3.Lerp(vel, -velAttract, attractor.attractPush * fdt);
                }
            }
        }

        // Установить vel в соответствии c velocity в объекте-одиночке Spawner
        vel = vel.normalized * attractor.velocity;
        // присвоить скорость компоненту Rigidbody
        rigid.velocity = vel;
        // Повернуть птицу клювом в сторону нового направления движения
        LookAhead();
    }



}
