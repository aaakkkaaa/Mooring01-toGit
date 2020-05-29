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

    private int _ropeIdx;
    // сколько частиц тащит сейлор правой рукой за один цикл 
    private int _ropeDragStep = 10;
    // прекращает таскать при достижении этого числа
    private int _dragLimit = 110;

    // Утка с которой работает сейлор
    public GameObject WorkCleat;

    private ObiSolver _solver;
    private Collider _coll;

    // индекс рабочей частицы
    private int _workIdx;

    // правильное положение и углы
    private bool needCorrectPose = false;
    Vector3 pos = new Vector3( -1.491f, 1.045f, -5.442f );
    Vector3 ang = new Vector3(0, -150, 0);

    private void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _coll = gameObject.GetComponent<Collider>();
    }

    private void Start()
    {
        CurState = "IDLE";
        //_animator.Play("m_idle_neutral_01");
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
                    //_animator.Play("64_26_2");
                    // "Разжать правую руку"
                    _animator.SetTrigger("FindRope");
                    //_animator.SetLayerWeight(1, 1f);
                }
                else
                {
                    // остановить анимации
                    CurState = "IDLE";
                    _animator.Play("m_idle_neutral_01");
                }
            }
        }
        if (CurState == "WAIT_DISTANCE")
        {
            rContr = WorkRope.GetComponent<RopeController>();
            float dist = Vector3.Magnitude(transform.position - RopeTarget.transform.position);
            if (dist < 5)
            {
                _animator.SetTrigger("ThrowRope");
                CurState = "THROW_ROPE";
            }

        }
        if(needCorrectPose)
        {
            transform.localPosition = pos;
            transform.localEulerAngles = ang;
            //transform.localPosition = Vector3.Lerp(transform.localPosition, pos, 0.1f);
            //transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, ang, 0.1f); 
        }
    }


    // Взять бухту каната в правую руку
    private void TakeRopeHank()
    {
        print("TakeRopeHank");
        // Find убрать при возможности
        GameObject.Find("Obi Rope").transform.SetParent(GameObject.Find("Obi Solver").transform);
        //_animator.SetBool("TakeRope", true); // Сжать правую руку

        if (WorkRope == null)
        {
            print("Не назначен WorkRope");
            return;
        }

        rContr = WorkRope.GetComponent<RopeController>();
        int[] points = { 50, 90, 130 };
        rContr.SetAttractors(RHand, points, 3);
      
        CurState = "WAIT_DISTANCE";
        rContr.CurState = "ATTRACT";
    }

    // Проверить дистанцию и или бросить, или ждать
    private void VerifyDistance()
    {
        print("VerifyDistance");
        rContr = WorkRope.GetComponent<RopeController>();
        float dist = Vector3.Magnitude(transform.position - RopeTarget.transform.position);
        if (dist < rContr.ThrowDistance)
        {
            _animator.SetTrigger("ThrowRope");
            CurState = "THROW_ROPE";
        }
        else
        {
            _animator.SetTrigger("WaitDistance");
            CurState = "WAIT_DISTANCE";
        }
    }

    private void ThrowRope()
    {
        print(name + ".ThrowRope");

        if (WorkRope == null)
        {
            print("Не назначен WorkRope");
            return;
        }

        rContr = WorkRope.GetComponent<RopeController>();
        rContr.FlyPoints.Clear();
        rContr.FlyTarget = null;
        // определим направление броска в глобальных координатах
        Vector3 startPoint;
        if (CurState == "TAKE_HANK_R")
        {
            startPoint = RHand.transform.position;
        }
        else
        {
            startPoint = LHand.transform.position;
        }
        rContr.ThrowTo(RopeTarget.transform.position - startPoint, 2.5f);
        rContr.CurState = "FREE";

        CurState = "IDLE";

       // _animator.SetLayerWeight(1, 0f); // "Разжать правую руку"

        // сообщим маринеро, чтобы принял позу ловца
        Marinero marinero = RopeTarget.GetComponent<Marinero>();
        marinero.CatchRope();
    }

    // Вызывается из Marinero, когда он бросает канат
    public void CatchRope()
    {
        print("Sailor.CatchRope()");
        _animator.SetTrigger("BeReadyToCatch");
        if (WorkRope != null)
        {
            // подписаться на обработку столкновений
            _solver = WorkRope.solver;
            _solver.OnCollision += SolverOnCollision;
            // включить полет каната к руке
            rContr = WorkRope.GetComponent<RopeController>();
            _ropeIdx = rContr.MaxPointIdx()-3;
            int[] points = { _ropeIdx };
            rContr.FlyPoints.Clear();
            rContr.FlyPoints.AddRange(points);
            rContr.FlyTarget = RHand;
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
                        if (collider == _coll)
                        {
                            isRopeContact = true;
                        }
                    }
                }
            }
        }
        if (isRopeContact)
        {
            print("Канат долетел до коллайдера Сейлора");
            // отключаем отлов столкновений
            _solver.OnCollision -= SolverOnCollision;

            // запускаем анимацию ловли
            _animator.SetTrigger("CatchNow");

            // включить притяжение каната к руке
            rContr = WorkRope.GetComponent<RopeController>();
            Attractor attr = new Attractor(RHand, _ropeIdx, 3);
            rContr.Attractors.Clear();
            rContr.Attractors.Add(attr);
            CurState = "TAKE_HANK_R";
            rContr.CurState = "ATTRACT";
            // анимация закрепления каната на утке запускается автоматически
        }
    }

    // в начале зацепа за утку надо скинуть канат с палубы в воду
    private void RopeToWater()
    {
        print(gameObject.name + ".RopeToWater()");
        GameObject attrObj = WorkCleat.transform.Find("Target2").gameObject;
        for (int i=0; i<1; i++)
        {
            Attractor attr = new Attractor(attrObj, _ropeIdx-10-i*5, 3, 1.0f);
            rContr.Attractors.Add(attr);
        }
    }

    // исключим лишние аттракторы
    private void HookRope()
    {
        print(gameObject.name + ".HookRope()");
        GameObject attrObj = WorkCleat.transform.Find("Target2").gameObject;
        rContr.RemoveAttractors(attrObj);
        rContr.BeginCleat(WorkCleat, 20, rContr.MaxPointIdx());
    }

    // временная мера, для установки сейлора в правильное положение для вытягивания каната
    private void SetStartPose()
    {
        needCorrectPose = true;
        //transform.localPosition = new Vector3(-1.491f, 1.045f, -5.442f);
        //transform.localEulerAngles = new Vector3(0, -150, 0);
    }


    // при вытягивании каната перехват левой рукой
    private void CatchRopeToLeftHand()
    {
        print("CatchRopeToLeftHand() " + rContr.Attractors.Count );
        _ropeIdx -= _ropeDragStep;
        print("_ropeIdx = " + _ropeIdx + "  _dragLimit = " + _dragLimit + " max = " + rContr.MaxPointIdx() );
        rContr.RemoveAttractors(RHand);
        if (_ropeIdx >_dragLimit)
        {
            rContr.SetAttractors(LHand, new int[]{ _ropeIdx}, 2 );
        }
        else
        {
            FixRope();
        }
    }

    // при вытягивании каната перехват правой рукой
    private void CatchRopeToRightHand()
    {
        _ropeIdx -= _ropeDragStep;
        rContr.RemoveAttractors(LHand);
        if (_ropeIdx > _dragLimit)
        {
            rContr.SetAttractors(RHand, new int[] { _ropeIdx }, 2 );
        }
        else
        {
            FixRope();
        }

    }

    // определить частицу каната ближайшую к точке фиксации и зафиксировать ее
    private void FixRope()
    {
        rContr.AttachPointToCleat();
    }


    private void DragRope()
    {

    }



}
