using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using System;
//using System.Runtime.Remoting.Metadata.W3cXsd2001;

public class Marinero : MonoBehaviour
{
    public GameObject LHand;
    public GameObject RHand;
    private Animator _animator;
    private RopeController rContr;

    [NonSerialized]
    public string[] States = { "IDLE", 
                               "FIND_ROPE", 
                               "TAKE_HANK_R", 
                               "TAKE_HANK_L", 
                               "THROW_ROPE", 
                               "DRAG_ROPE", 
                               "FIX_ROPE",  
                               "PUSH_ROPE",
                               "WAIT_DISTANCE" };
    [NonSerialized]
    public string CurState = "IDLE";

    // канат с которым взаимодействует перс
    public ObiRope WorkRope;
    // направление броска каната
    public GameObject RopeTarget;
    // индекс рабочей частицы
    private int _ropeIdx;
    // сколько частиц тащит маринеро правой рукой за один цикл 
    private int _ropeDragStep=20;
    // прекращает таскать при достижении этого числа
    private int _dragLimit = 100;

    // Утка с которой работает маринеро
    public GameObject WorkCleat;

    private ObiSolver _solver;
    private Collider _coll;

    private IEnumerator _coroutineLimitV;


    private void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _coll = gameObject.GetComponent<Collider>();
    }

    private void Start()
    {
        CurState = "IDLE";
    }

    private void FixedUpdate()
    {
        if(CurState == "WAIT_DISTANCE")
        {
            float dist = Vector3.Magnitude(transform.position - RopeTarget.transform.position);
            if ( dist < rContr.ThrowDistance )
            {
                _animator.SetTrigger("ThrowRope");
                CurState = "THROW_ROPE";
            }
            else if(dist > rContr.FreeDistance)
            {
                // отпустить канат
                FreeRope();
            }
        }
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
            // включить полет каната к руке
            rContr = WorkRope.GetComponent<RopeController>();
            //_ropeIdx = 120;
            _ropeIdx = rContr.MaxPointIdx();
            int[] points = { _ropeIdx };
            rContr.FlyPoints.Clear();
            rContr.FlyPoints.AddRange(points);
            rContr.FlyTarget = RHand;
            CurState = "TAKE_HANK_L";
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
                        if (collider == _coll)
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
            Attractor attr = new Attractor(LHand, _ropeIdx, 10 );
            rContr.Attractors.Clear();
            rContr.Attractors.Add(attr);
            CurState = "TAKE_HANK_L";
            rContr.CurState = "ATTRACT";
        }
    }

    /*
    // надо перехватить левой рукой за последнюю частицу - не используется!
    void GetEndOfRopeToLeftHand()
    {
        print("GetEndOfRopeToLeftHand()");
        // включить притяжение крайней точки каната правой руке
        rContr = WorkRope.GetComponent<RopeController>();
        _ropeIdx = WorkRope.activeParticleCount - 1;
        rContr.Attractors[0].FixPoint = _ropeIdx;
        rContr.Attractors[0].Fixator = LHand;
        rContr.CurState = "ATTRACT";
        //print(rContr.Attractors.Count);
    }
    */

    // вызывается из анимации когда надо просунуть канат в утку
    void PushThroughToTarget1()
    {
        print("PushThroughToTarget1()");
        
        rContr = WorkRope.GetComponent<RopeController>();
        // включить притяжение крайней точки каната к Target1
        _ropeIdx = WorkRope.activeParticleCount - 1;
        rContr.Attractors[0].FixPoint = _ropeIdx;
        GameObject target1 = WorkCleat.transform.Find("Target1").gameObject;
        rContr.Attractors[0].Fixator = target1;
        CurState = "PUSH_ROPE";
        rContr.CurState = "ATTRACT";

        // выстроить крайние точки каната по линии, чтобы потом продеть в утку
        Vector3 t1 = WorkCleat.transform.Find("Target1").position;
        Vector3 t2 = WorkCleat.transform.Find("Target2").position;
        rContr.BeginArrange(t1, t2, 10);

    }


    void PushThroughToTarget2()
    {
        print("PushThroughToTarget2()");
        rContr = WorkRope.GetComponent<RopeController>();
        // выключить выравнивание каната
        rContr.EndArrange(10);
        // включить притяжение крайней точки каната к Target2
        _ropeIdx = WorkRope.activeParticleCount - 1;
        rContr.Attractors[0].FixPoint = _ropeIdx;
        GameObject target1 = WorkCleat.transform.Find("Target2").gameObject;
        rContr.Attractors[0].Fixator = target1;
        CurState = "PUSH_ROPE";
        rContr.CurState = "ATTRACT";
        rContr.BeginCleat(WorkCleat, 20, rContr.MaxPointIdx());
    }

    void AfterPush()
    {
        print(gameObject.name + " - AfterPush()");
        // включить притяжение крайней точки каната правой руке
        rContr = WorkRope.GetComponent<RopeController>();
        _ropeIdx = WorkRope.activeParticleCount - 1;
        rContr.Attractors[0].FixPoint = _ropeIdx;
        GameObject target1 = WorkCleat.transform.Find("Target2").gameObject;
        rContr.Attractors[0].Fixator = RHand;
        CurState = "PUSH_ROPE";
        rContr.CurState = "ATTRACT";
    }

    // взять канат вдобавок к правой руке еще и левой не за самый конец
    void AddLeftHand()
    {
        if (CurState == "IDLE") return;

        print(gameObject.name + " - AddLeftHand() " + rContr.Attractors.Count);
        _ropeIdx -= _ropeDragStep;
        Attractor attr = new Attractor(LHand, _ropeIdx, 2);
        rContr.Attractors.Add(attr);
    }

    // передать конец из правой в левую
    void FreeRightHand()
    {
        if (CurState == "IDLE") return;

        print("FreeRightHand()");
        rContr.Attractors[0].Fixator = LHand;
        rContr.Attractors[0].Interval = 2;
        if( !rContr.CanThrow(gameObject,RopeTarget,2) )
        {
            FreeRope();
        }
    }

    // Канат вытягивается правой рукой а бухта собирается в левой руке
    void DragRopeWorkPointToLeft()
    {
        print(gameObject.name + " - DragRopeWorkPointToLeft() " + rContr.Attractors.Count);
        rContr = WorkRope.GetComponent<RopeController>();
        int workAttr = rContr.Attractors.Count - 1;
        rContr.CurState = "ATTRACT";
        rContr.Attractors[workAttr].Fixator = LHand;
        // для проверки превышения дистанции
        float dist = Vector3.Magnitude(transform.position - RopeTarget.transform.position);
        if (!rContr.CanThrow(gameObject, RopeTarget, 2))
        {
            FreeRope();
        }
        else
        {
            // если дотянули до предела
            if (_ropeIdx < _dragLimit)
            {
                _animator.SetTrigger("ThrowRope");
                CurState = "THROW_ROPE";
                print("THROW_ROPE");
            }
        }

    }

    void DragRopeNewWorkPointToRight()
    {
        if(CurState == "THROW_ROPE" || CurState == "WAIT_DISTANCE" || CurState == "IDLE" )
        {
            return; // уже закончили вытягивать
        }
        if (!rContr.CanThrow(gameObject, RopeTarget, 2))
        {
            FreeRope();
        }
        else
        {
            print("CurState = " + CurState);
            print(gameObject.name + " - DragRopeNewWorkPointToRight() " + rContr.Attractors.Count);
            _ropeIdx -= _ropeDragStep;
            Attractor attr = new Attractor(RHand, _ropeIdx, 2);
            rContr.Attractors.Add(attr);
        }
    }

    // перекладываем бухту в правую руку перед броском, кроме последнего витка, который  передаем в левую 
    void HankToRightHand()
    {
        print("HankToRightHand() " + rContr.Attractors.Count);
        for (int i=0; i<rContr.Attractors.Count; i++)
        {
            if(rContr.Attractors[i].Fixator == LHand)
            {
                rContr.Attractors[i].Fixator = RHand;
            }
        }
        rContr.Attractors[rContr.Attractors.Count-1].Fixator = LHand;

        // закрепить на утке частицу каната, наиболее к ней близкую
        rContr.AttachPointToCleat();

    }

    // бросок в направлении sailor
    private void ThrowRope()
    {
        print(name + ".ThrowRope()");

        rContr.FlyPoints.Clear();
        rContr.FlyTarget = null;

        // определим направление броска в глобальных координатах
        Vector3 startPoint;
        startPoint = RHand.transform.position;

        // закрепить на утке частицу каната, наиболее к ней близкую
       // rContr.AttachPointToCleat();

        // бросок каната
        rContr.ThrowTo(RopeTarget.transform.position - startPoint, 3.5f);

        rContr.CurState = "FREE";       // переделать на FLY_TO
        CurState = "IDLE";
        

        // сообщим сейлору, чтобы принял позу ловца
        Sailor sailor = RopeTarget.GetComponent<Sailor>();
        sailor.CatchRope();
       
    }

    private void FreeRope()
    {
        _animator.SetTrigger("FreeRope");
        rContr.RemoveAttractors(LHand);
        rContr.RemoveAttractors(RHand);
        rContr.EndCleat();
        CurState = "IDLE";
        for (int i = 0; i < rContr.Attractors.Count; i++)
        {
            print(rContr.Attractors[i].Fixator);
        }
        // сейлор должен начать вытаскивать из воды этот канат
        Sailor sailor = RopeTarget.GetComponent<Sailor>();
        sailor.BeginPullOut();
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
