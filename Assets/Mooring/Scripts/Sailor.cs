using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using System;

public class Sailor : MonoBehaviour
{
    public GameObject LHand;
    public GameObject RHand;
    private Animator _animator;
    private RopeController rContr;

    [NonSerialized]
    public string[] States = { "IDLE", "FIND_ROPE", "TAKE_HANK_R", "TAKE_HANK_L", "THROW_ROPE", "DRAG_ROPE_L", "DRAG_ROPE_R", "FIX_ROPE" };
    [NonSerialized]
    public string CurState = "IDLE";

    // канат с которым взаимодействует перс
    public ObiRope WorkRope;
    // направление броска каната
    public GameObject RopeTarget;

    private int _workIdx;

    private void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
    }

    private void Start()
    {
        CurState = "IDLE";
        _animator.Play("m_idle_neutral_01");
    }

    void Update()
    {

        // Определить, нажата ли левая кнопка мыши
        if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
        {
            if (Input.GetKeyDown("t")) // Поднять и бросить конец
            {
                if (CurState == "IDLE")
                {
                    // воспроизведение анимации
                    CurState = "FIND_ROPE";
                    _animator.Play("64_26_2");
                }
                else
                {
                    // остановить анимации
                    CurState = "IDLE";
                    _animator.Play("m_idle_neutral_01");
                }
            }
        }
    }



    // после вытягивания впереде левой руки притягивает к руке ближайший шарик рабочего каната
    public void FindRope()
    {
        print("FindRope");
        if (WorkRope == null)
        {
            print("Не назначен WorkRope");
            return;
        }
        float minMag = 100f;
        int minIdx = -1;    // индекс в солвере
        ObiSolver solver = WorkRope.solver;
        for (int i = 0; i < WorkRope.particleCount; i++)
        {
            int solverIndex = WorkRope.solverIndices[i];
            Vector3 posL = solver.positions[solverIndex];
            Vector3 pos = solver.transform.TransformPoint(posL);
            float curMag = (pos - LHand.transform.position).magnitude;
            if (curMag < minMag)
            {
                minMag = curMag;
                minIdx = i;
            }
        }
        print(minIdx);
        if (minIdx != -1)
        {
            _workIdx = minIdx;
            rContr = WorkRope.GetComponent<RopeController>();
            if(rContr.FixPoints.Count == 0)
            {
                rContr.FixPoints.Add(_workIdx);
            }
            else
            {
                rContr.FixPoints[0] = _workIdx; 
            }
            rContr.Fixator = RHand;
            CurState = "DRAG_ROPE_R";
        }
        else
        {
            CurState = "IDLE";
            _animator.Play("m_idle_neutral_01");
        }
    }



    // Взять бухту каната
    private void TakeRopeHank()
    {
        print("TakeRopeHank");
        GameObject.Find("Obi Rope").GetComponent<Obi.ObiPathSmoother>().enabled = true;

        if (WorkRope == null)
        {
            print("Не назначен WorkRope");
            return;
        }

        rContr = WorkRope.GetComponent<RopeController>();
        int[] points = { 100, 200, 300, 400 };
        rContr.FixPoints.Clear();
        rContr.FixPoints.AddRange(points);
        rContr.Fixator = RHand;
        CurState = "TAKE_HANK_R";
        rContr.CurState = "MANYPOINTS";

    }

    private void ThrowRope()
    {
        print("ThrowRope");

        if (WorkRope == null)
        {
            print("Не назначен WorkRope");
            return;
        }

        rContr = WorkRope.GetComponent<RopeController>();
        rContr.FixPoints.Clear();
        rContr.Fixator = null;
        // определим направление броска в глобальных координатах
        Vector3 startPoint;
        if(CurState == "TAKE_HANK_R")
        {
            startPoint = RHand.transform.position;
        }
        else
        {
            startPoint = LHand.transform.position;
        }
        rContr.ThrowTo(RopeTarget.transform.position - startPoint);
        rContr.CurState = "FREE";

        CurState = "IDLE";
    }

    private void CatchRope()
    {

    }

    private void DragRope()
    {

    }

    private void FixRope()
    {

    }

}
