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
    private int _ropeDragStep = 20;
    // прекращает таскать при достижении этого числа
    private int _dragLimit = 100;

    // Утка с которой работает сейлор
    public GameObject WorkCleat;

    private ObiSolver _solver;
    private Collider _coll;

    // индекс рабочей частицы
    private int _workIdx;

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
            _ropeIdx = 125;
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

    private void HookRope()
    {
        rContr.BeginCleat(WorkCleat);
    }


    private void DragRope()
    {

    }



}
