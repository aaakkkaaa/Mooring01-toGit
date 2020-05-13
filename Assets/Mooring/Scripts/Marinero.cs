using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

public class Marinero : MonoBehaviour
{
    public GameObject LHand;
    public GameObject RHand;
    private Animator _animator;
    private RopeController rContr;

    [NonSerialized]
    public string[] States = { "IDLE", "FIND_ROPE", "TAKE_HANK_R", "TAKE_HANK_L", "THROW_ROPE", "DRAG_ROPE_L", "DRAG_ROPE_R", "FIX_ROPE",  "PUSH_ROPE" };
    [NonSerialized]
    public string CurState = "IDLE";

    // канат с которым взаимодействует перс
    public ObiRope WorkRope;
    // направление броска каната
    public GameObject RopeTarget;
    // индекс рабочей частицы
    private int _ropeIdx;
    private int _ropeDragStep=5;

    // Утка с которой работает маринеро
    public GameObject WorkCleat;

    private ObiSolver _solver;
    private Collider _coll;

    private void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _coll = gameObject.GetComponent<Collider>();
    }

    private void Start()
    {
        CurState = "IDLE";
    }

    // Вызывается из Sailor, когда она бросает канат
    public void CatchRope()
    {
        print("Marinero.CatchRope()");
        _animator.SetTrigger("BeReadyToCatch");
        //_animator.Play("143_20_01");
        if(WorkRope != null)
        {
            // подписаться на обработку столкновений
            _solver = WorkRope.solver;
            _solver.OnCollision += SolverOnCollision;
            // включить притяжение каната к руке
            rContr = WorkRope.GetComponent<RopeController>();
            _ropeIdx = 120;
            int[] points = { _ropeIdx };
            rContr.FixPoints.Clear();
            rContr.FixPoints.AddRange(points);
            rContr.Fixator = RHand;
            CurState = "TAKE_HANK_R";
            rContr.CurState = "FLY_TO";
        }
    }

    void SolverOnCollision(object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
    {
        // нужно убедиться, что случился контакт именно WorkRope и Marinero
        bool isRopeContact = false;
        foreach (Oni.Contact contact in e.contacts)
        {
            if (contact.distance < 0.01)
            {
                ObiSolver.ParticleInActor pa = _solver.particleToActor[contact.particle];
               if (pa.actor == WorkRope)
                {
                    Component collider;
                    if (ObiCollider.idToCollider.TryGetValue(contact.other, out collider))
                    {
                        if(collider == _coll)
                        {
                            isRopeContact = true;
                        }
                    }
                }
            }
        }
        if(isRopeContact)
        {
            print("Канат долетел до коллайдера Маринеро");
            // отключаем отлов столкновений
            _solver.OnCollision -= SolverOnCollision;
            
            // запускаем анимацию ловли
            _animator.SetTrigger("CatchNow");

            // включить притяжение каната к руке
            rContr = WorkRope.GetComponent<RopeController>();
            int[] points = { _ropeIdx };
            rContr.FixPoints.Clear();
            rContr.FixPoints.AddRange(points);
            rContr.Fixator = RHand;
            CurState = "TAKE_HANK_R";
            rContr.CurState = "MANYPOINTS";
        }
    }

    // надо перехватить левой рукой за последнюю частицу
    void GetEndOfRopeToLeftHand()
    {
        // включить притяжение крайней точки каната правой руке
        rContr = WorkRope.GetComponent<RopeController>();
        _ropeIdx = WorkRope.activeParticleCount - 1;
        int[] points = { _ropeIdx };
        rContr.FixPoints.Clear();
        rContr.FixPoints.AddRange(points);
        rContr.Fixator = LHand;
        CurState = "DRAG_ROPE_L";
        rContr.CurState = "MANYPOINTS";
    }

    // вызывается из анимации когда надо просунуть канат в утку
    void PushThroughToTarget1()
    {
        print("PushThroughToLeftHand()");
        // включить притяжение крайней точки каната к Target1
        rContr = WorkRope.GetComponent<RopeController>();
        _ropeIdx = WorkRope.activeParticleCount - 1;
        int[] points = { _ropeIdx };
        rContr.FixPoints.Clear();
        rContr.FixPoints.AddRange(points);
        GameObject attract = WorkCleat.transform.Find("Target1").gameObject;
        rContr.Fixator = attract;
        CurState = "PUSH_ROPE";
        rContr.CurState = "MANYPOINTS";
    }

    void PushThroughToTarget2()
    {
        print("PushThroughToRightHand()");
        // включить притяжение крайней точки каната к Target2
        rContr = WorkRope.GetComponent<RopeController>();
        _ropeIdx = WorkRope.activeParticleCount - 1;
        int[] points = { _ropeIdx };
        rContr.FixPoints.Clear();
        rContr.FixPoints.AddRange(points);
        GameObject attract = WorkCleat.transform.Find("Target2").gameObject;
        rContr.Fixator = attract;
        CurState = "PUSH_ROPE";
        rContr.CurState = "MANYPOINTS";
    }

    void AfterPush()
    {
        // включить притяжение крайней точки каната правой руке
        rContr = WorkRope.GetComponent<RopeController>();
        _ropeIdx = WorkRope.activeParticleCount - 1;
        int[] points = { _ropeIdx };
        rContr.FixPoints.Clear();
        rContr.FixPoints.AddRange(points);
        rContr.Fixator = RHand;
        CurState = "DRAG_ROPE_R";
        rContr.CurState = "MANYPOINTS";

    }

    // при вытягивании каната вызываются по очереди
    void TakeRopeToLeftHand()
    {
        print("TakeRopeToLeftHand()");
        _ropeIdx -= _ropeDragStep;
        // нужно будет переделать потом условие в зависимости от расстояния до утки
        if (_ropeIdx > 30)
        {
            rContr.FixPoints[0] = _ropeIdx;
            rContr.Fixator = LHand;
            CurState = "DRAG_ROPE_L";
            rContr.CurState = "MANYPOINTS";
        }
        else
        {
            _animator.SetTrigger("EndDrag");
        }

    }
    void TakeRopeToRightHand()
    {
        print("TakeRopeToRightHand()");
        _ropeIdx -= _ropeDragStep;
        // нужно будет переделать потом условие в зависимости от расстояния до утки
        if (_ropeIdx > 30)
        {
            rContr.FixPoints[0] = _ropeIdx;
            rContr.Fixator = RHand;
            CurState = "DRAG_ROPE_R";
            rContr.CurState = "MANYPOINTS";
        }
        else
        {
            _animator.SetTrigger("EndDrag");
        }

    }

    // Запустить анимацию после зарержки
    IEnumerator FreezeRope( float wait, string trig )
    {
        // Переждать время 
        yield return new WaitForSeconds(wait);
        // Толкнуть анимацию триггером
        _animator.SetTrigger(trig);
    }


}
