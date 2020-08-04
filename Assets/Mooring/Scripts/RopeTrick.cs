using System.Collections; 
using System.Collections.Generic;
using UnityEngine;

public class RopeTrick : MonoBehaviour
{
    // куда прикреплять на яхте
    public Transform YachtElement;
    // куда прикреплять на берегу
    public Transform MarinaElement;
    // длинна в момент, когда канат привязали (нужно для вычисления сил)
    public float PhisicLength;

    // начальная длинна меша
    private float _baseL = 1.8f;

    private void Start()
    {
        if (YachtElement == null || MarinaElement == null) return;
        PhisicLength = (YachtElement.position - MarinaElement.position).magnitude;
    }

    private void FixedUpdate()
    {
        if (YachtElement == null || MarinaElement == null) return;

        transform.position = YachtElement.position;
        float curL = (YachtElement.position - MarinaElement.position).magnitude;
        Vector3 curScale = transform.localScale;
        curScale.z = curL / _baseL;
        curScale.x = 1 / curScale.z;
        transform.localScale = curScale;
        transform.LookAt(MarinaElement.position, Vector3.up);

    }
}
