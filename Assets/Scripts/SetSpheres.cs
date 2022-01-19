using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSpheres : MonoBehaviour
{
    public float interval = 10f;
    public int RowAmount = 100;
    public GameObject buyExample;

    void Start()
    {
        print("Создаю буйки");

        float startX = -RowAmount * interval / 2;
        float startZ = -RowAmount * interval / 2;
        for( int i=0; i<RowAmount; i++)
        {
            for(int j=0; j<RowAmount; j++)
            {
                GameObject oneBuy = Instantiate(buyExample, gameObject.transform);
                //Color с = new Color(0.5f + Random.value / 2, 0.5f + Random.value / 2, 0.5f + Random.value / 2);
                Color с = new Color( Random.value, Random.value, Random.value );
                oneBuy.GetComponent<Renderer>().material.color = с;
                oneBuy.name = "Buy_" + i + "_" + j;
                oneBuy.transform.localPosition = new Vector3(startX + i* interval, 0, startZ + j* interval);
            }
        }

    }

}
