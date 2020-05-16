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
    private int _ropeDragStep=20;

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
        
        rContr = WorkRope.GetComponent<RopeController>();
        // выстроить крайние точки каната по линии, чтобы потом продеть в утку
        Vector3 t1 = WorkCleat.transform.Find("Target1").position;
        Vector3 t2 = WorkCleat.transform.Find("Target2").position;
        rContr.MoveTo(t1, t2, 15);
        // включить притяжение крайней точки каната к Target1
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
        print(gameObject.name + " - AfterPush()");
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

    // Канат вытягивается правой рукой а бухта собирается в левой руке
    void DragRopeWorpPointToLeft()
    {
        print(gameObject.name + " - DragRopeWorpPointToLeft()");
        rContr = WorkRope.GetComponent<RopeController>();
        // если это первая итерация, то канат в правой руке, а в левой пусто
        if (rContr.CurState == "MANYPOINTS")
        {
            // передадим канат в левую руку 
            rContr.Fixator = LHand;
            // укажем, новое текущее состояние
            rContr.CurState = "MANY_AND_ONE";
            // но второй точки фиксации пока нет
            rContr.Fixator2 = null;
        }
        else if(rContr.CurState == "MANY_AND_ONE")
        {
            // добавим текущую точку фиксации из правой руки в левую
            rContr.FixPoints.Add(_ropeIdx);
            // освободим правую руку
            rContr.Fixator2 = null;
            // если дотянули до предела
            if (_ropeIdx < 70)
            {
                rContr.Fixator2 = null;
                _animator.SetTrigger("EndDrag");
                rContr.CurState = "MANYPOINTS";
            }    
        }
    }

    void DragRopeNewWorpPointToRight()
    {
        print(gameObject.name + " - DragRopeNewWorpPointToRight()");
        _ropeIdx -= _ropeDragStep;
        rContr.Fixator2 = RHand;
        rContr.FixPoint2 = _ropeIdx;
    }


    // Запустить анимацию после задержки
    IEnumerator FreezeRope( float wait, string trig )
    {
        // Переждать время 
        yield return new WaitForSeconds(wait);
        // Толкнуть анимацию триггером
        _animator.SetTrigger(trig);
    }


}
