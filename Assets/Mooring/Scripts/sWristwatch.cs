using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sWristwatch : MonoBehaviour
{
    private Transform _hourHand;
    private Transform _minuteHand;
    private Transform _secondHand;
    private int _hourShift = 90;
    private int _minuteShift = 90;
    private int _secondShift = 0;
    private Vector3 _hourEu;
    private Vector3 _minuteEu;
    private Vector3 _secondEu;

    // Start is called before the first frame update
    void Start()
    {
        _hourHand = transform.Find("Wristwatch.003");
        _minuteHand = transform.Find("Wristwatch.002");
        _secondHand = transform.Find("Wristwatch.005");

        _hourEu = _hourHand.localEulerAngles;
        _minuteEu = _minuteHand.localEulerAngles;
        _secondEu = _secondHand.localEulerAngles;

        StartCoroutine(updateWatches());
    }

    // 
    IEnumerator updateWatches()
    {
        while (true)
        {
            TimeSpan CurTime = DateTime.Now.TimeOfDay; // текущее время
            
            int Hours = CurTime.Hours;
            int Minutes = CurTime.Minutes;

            float tHours = (float)CurTime.TotalHours;
            if(tHours >= 12.0f)
            {
                tHours -= 12.0f;
            }
            float tMinutes = (float)CurTime.TotalMinutes - Hours * 60;
            float tSeconds = (float)CurTime.TotalSeconds - Hours * 3600 - Minutes * 60;

            //print("tHours = " + tHours + " tMinutes = " + tMinutes + " tSeconds = " + tSeconds);

            _hourEu.z = tHours * 30 + _hourShift;
            _minuteEu.z = tMinutes * 6 + _minuteShift;
            _secondEu.z = tSeconds * 6 + _secondShift;

            _hourHand.localEulerAngles = _hourEu;
            _minuteHand.localEulerAngles = _minuteEu;
            _secondHand.localEulerAngles = _secondEu;

            yield return new WaitForSeconds(0.2f);
        }
    }
}
