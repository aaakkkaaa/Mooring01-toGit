using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class Kranec : MonoBehaviour
{
    public GameObject Layer;

    private GameObject _ballon;
    private ObiRope _rope;
    private GameObject _zero;
    private ObiRopeBlueprint _bluePrint;

    public bool Set;

    private void Awake()
    {
        _ballon = transform.Find("Ballon").gameObject;
        _rope = transform.Find("Rope").GetComponent<ObiRope>();
        _bluePrint = _rope.blueprint as ObiRopeBlueprint;
        //_zero = transform.Find("ZeroPoint").gameObject;
    }

    private void Update()
    {
        if (Set)
        {
            SetAndKnot();
            Set = false;
        }
    }

    public void SetAndKnot()
    {
        if (Layer == null) return;

        // поднести кранец близко к точке привязки
        transform.position = Layer.transform.position;
        transform.rotation = Layer.transform.rotation;
        Vector3 pos = transform.localPosition;
        pos.x -= 0.3f;
        transform.localPosition = pos;

        StartCoroutine(MoveToLayer());
    }

    private IEnumerator MoveToLayer()
    {
        while ((Layer.transform.position - transform.position).magnitude > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, Layer.transform.position, 0.05f);
            yield return new WaitForFixedUpdate();
        }
    }

}
