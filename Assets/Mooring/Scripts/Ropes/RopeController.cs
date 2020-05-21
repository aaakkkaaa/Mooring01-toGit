using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using System;
using Obi;

// Взаимодействие с персонажами, броски, закрепление
public class RopeController : MonoBehaviour
{
    public float Stretch = 1.3f;
    public float MaxForce = 2000;

    // для таскания рукой за одну или несколько точек
    private float _springStiffness = 5000;
    private float _springDamping = 50;
    private int _intervalSpring = 4;        // на сколько еще частиц в обе стороны применить силу
    private int _intervalFly = 8;

    [NonSerialized]
    public List<Attractor> Attractors;  // описывание притяжения: к какому объекту какая частица каната притягивается

    [NonSerialized]
    public float ThrowDistance = 5.0f;

    [NonSerialized]
    public List<int> FlyPoints;         // точки, притягиваемые к цели полета
    [NonSerialized]
    public GameObject FlyTarget;          // объект, к которому идет притяжение в полете

    private ObiSolver _solver;
    private ObiRope _rope;

    // для выбора поведения в FixedUpdate
    [NonSerialized]
    public string[] States = { "FREE", "FLY_TO", "ATTRACT", "ATTACH"};
    [NonSerialized]
    public string CurState = "FREE";    // ONEPOINT, MANYPOINTS, FREE

    // утка, с которой взаимодействует канат, передается из marinero
    private GameObject _marieroCleat;
    // точки в утке, вдоль которых должен стараться пролечь канат (в системе координат солвера)
    private Vector4 _t1;
    private Vector4 _t2;
    // коллайдер утки, с которым будет отлавливаться взаимодействие
    private Collider _coll;
    private List<int> CollPoints;

    // номер точки (в солвере!), приаттаченая к утке на берегу
    private int AttachOnShoreIdx;


    private void Awake()
    {
        FlyPoints = new List<int>();
        Attractors = new List<Attractor>();
    }

    private void OnEnable()
    {
        _rope = GetComponent<ObiRope>();
        _solver = _rope.solver;
    }


    private void FixedUpdate()
    {
        if (CurState == "" || CurState == "FREE") return;
        if (_solver == null)
        {
            _rope = GetComponent<ObiRope>();
            _solver = _rope.solver;
        }

        if (CurState == "ATTRACT")
        {
            for(int i=0; i< Attractors.Count; i++)
            {
                GameObject Fixator = Attractors[i].Fixator;
                Vector4 targetPosition;
                if (Fixator != null)
                {
                    targetPosition = _solver.transform.InverseTransformPoint(Fixator.transform.position);
                }
                else
                {
                    targetPosition = _solver.transform.InverseTransformPoint(Attractors[i].Pos);
                }
                
                int particleIndex = Attractors[i].FixPoint;
                int pIdx = _rope.solverIndices[particleIndex];
                float invMass = _solver.invMasses[pIdx];
                if (invMass > 0)
                {
                    // Вычисление и применение силы таскающей пружины:
                    Vector4 position = _solver.positions[pIdx];
                    Vector4 velocity = _solver.velocities[pIdx];
                    Vector4 force = ((targetPosition - position) * _springStiffness - velocity * _springDamping) / invMass;
                    force *= Attractors[i].ForceMult;
                    _solver.externalForces[pIdx] = force;

                    // воздействовать еще на несколько шариков
                    Vector4 dF = force / (Attractors[i].Interval + 1);
                    for (int j = 1; j <= Attractors[i].Interval; j++)
                    {
                        force -= dF;    // ослабить силу
                        if (particleIndex + j < _rope.particleCount)
                        {
                            pIdx = _rope.solverIndices[particleIndex + j];
                            _solver.externalForces[pIdx] = force;
                        }
                        if (particleIndex - j > 0)
                        {
                            pIdx = _rope.solverIndices[particleIndex - j];
                            _solver.externalForces[pIdx] = force;
                        }
                    }
                }

            }
        }

        if(CurState == "FLY_TO")
        {
            if (FlyTarget != null)
            {
                Vector4 targetPosition = _solver.transform.InverseTransformPoint(FlyTarget.transform.position);
                //print(_rope.particleCount);
                //print("FixPoints.Count = " + FixPoints.Count);
                for (int i = 0; i < FlyPoints.Count; i++)
                {
                    int particleIndex = FlyPoints[i];
                    int pIdx = _rope.solverIndices[particleIndex];
                    // Calculate effective inverse mass:
                    float invMass = _solver.invMasses[pIdx];
                    if (invMass > 0)
                    {
                        Vector4 position = _solver.positions[pIdx];
                        Vector4 velocity = _solver.velocities[pIdx];
                        Vector4 dir = (targetPosition - position);
                        float value = velocity.magnitude;
                        _solver.velocities[pIdx] = dir * value;

                        for (int j = 1; j <= _intervalFly; j++)
                        {
                            if (particleIndex + j < _rope.particleCount)
                            {
                                pIdx = _rope.solverIndices[particleIndex + j];
                                _solver.velocities[pIdx] = dir * value;
                            }
                            if (particleIndex - j > 0)
                            {
                                pIdx = _rope.solverIndices[particleIndex - j];
                                _solver.velocities[pIdx] = dir * value;
                            }
                        }
                    }

                }
            }

        }
        /*
        if (CurState == "ATTACH")
        {
            print("AttachOnShoreIdx = " + AttachOnShoreIdx);
            _solver.invMasses[AttachOnShoreIdx] = 0;
        }
        */

        // притянуть частицы к оси утки
        if (CollPoints != null)
        {
            for (int i = 0; i < CollPoints.Count; i++)
            {
                int idx = CollPoints[i];
                // на линии _t1:_t2 найти точку, на которую проецируется шарик
                Vector3 vector = _solver.positions[idx] - _t1;
                Vector3 onNormal = _t2 - _t1;
                Vector3 project = Vector3.Project(vector, onNormal);
                Vector3 attractPos = (Vector4)project + _t1;
                // добавить частице силу в направлении этой точки
                float invMass = _solver.invMasses[idx];
                if (invMass > 0)
                {
                    // Вычисление и применение силы таскающей пружины:
                    Vector4 position = _solver.positions[idx];
                    Vector4 velocity = _solver.velocities[idx];
                    Vector4 force = (((Vector4)attractPos - position) * _springStiffness - velocity * _springDamping) / invMass;
                    _solver.externalForces[idx] = force/3;
                    //print(idx + "  " + force);
                }
            }

        }


    }

    // переместить точки каната от 0 до idx так, чтобы они были дальше от t2 чем t1 и выстроены по линии
    public void BeginArrange(Vector3 t1, Vector3 t2, int idx )
    {
        // куда притягивать, зависит от того, на каком расстоянии друг от друга сейчас точки
        float s = 0;
        for (int i=1; i< idx; i++)
        {
            int pIdx0 = _rope.solverIndices[ _rope.activeParticleCount - i - 1 ];
            int pIdx1 = _rope.solverIndices[ _rope.activeParticleCount - i ];
            s += (_solver.positions[pIdx0] - _solver.positions[pIdx1]).magnitude;
        }
        // разместим эти точки на прямой
        Vector3 newPos = t1 + (t1 - t2) * s;
        Attractor attr = new Attractor(null, _rope.activeParticleCount - 1 - idx);
        attr.Pos = newPos;
        Attractors.Add(attr);
    }

    public void EndArrange(int idx)
    {
        int delIdx=-1;

        for(int i=0; i<Attractors.Count; i++)
        {
            if(Attractors[i].FixPoint == _rope.activeParticleCount - 1 - idx)
            {
                delIdx = i;
            }
        }
        if(delIdx >= 0)
        {
            Attractors.RemoveAt(delIdx);
        }

    }

    // передаем утку, из нее извлекаем все нужное
    public void BeginCleat(GameObject cleat)
    {
        _marieroCleat = cleat;
        // выстроить крайние точки каната по линии, чтобы потом продеть в утку
        Vector3 _tg1 = _marieroCleat.transform.Find("Target1").position;
        Vector3 _tg2 = _marieroCleat.transform.Find("Target2").position;
        _t1 = _rope.solver.transform.InverseTransformPoint(_tg1);
        _t2 = _rope.solver.transform.InverseTransformPoint(_tg2);
        // ищем коллайдер, с которым будем взаимодействовать
        _coll = _marieroCleat.transform.Find("Zona").gameObject.GetComponent<Collider>();
        CollPoints = new List<int>();
        _rope.solver.OnCollision += SolverOnCleatCollision;
    }
    public void EndCleat()
    {
        _rope.solver.OnCollision -= SolverOnCleatCollision;
        CollPoints = null;
    }

    // убедиться, что случился контакт именно _rope и _cool, запомнить частицу
    void SolverOnCleatCollision(object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
    {
        if (CollPoints == null) return;

        CollPoints.Clear();
        foreach (Oni.Contact contact in e.contacts)
        {
            if (contact.distance < 0.01)
            {
                int idx = contact.particle;
                ObiSolver.ParticleInActor pa = _solver.particleToActor[idx];
                if (pa.actor == _rope)
                {
                    Component collider;
                    if (ObiCollider.idToCollider.TryGetValue(contact.other, out collider))
                    {
                        if (collider == _coll)
                        {
                            CollPoints.Add(idx);
                        }
                    }
                }
            }
        }
        //print("---------------");
    }


    public void ThrowTo(Vector3 direct, float dH)
    {
        Vector3 throwDirection = direct;
        throwDirection.y += throwDirection.magnitude + dH;
        float value = throwDirection.magnitude/2;
        Vector3 solvDir = _solver.transform.InverseTransformDirection(throwDirection);
        for (int i = 0; i < _rope.particleCount; i++)
        {
            int solverIndex = _rope.solverIndices[i];
            _solver.velocities[solverIndex] = solvDir * value;
        }
    }

    public void SetAttractors( GameObject obj, int[] points, int num = 1, float force = 1.0f)
    {
        Attractors.Clear();
        for(int i=0; i<points.Length; i++)
        {
            Attractor attr = new Attractor(obj, points[i], num, force);
            Attractors.Add(attr);
        }
    }

    public void AttachPointToCleat()
    {
        print("Rope.AttachPointToCleat()");
        if(CollPoints.Count == 0)
        {
            print("Невозможно соединится с уткой, нет подходящих точек!");
        }
        else
        {
            AttachOnShoreIdx = CollPoints[(CollPoints.Count - 1)/ 2];
            CurState = "ATTACH";
            print("AttachOnShoreIdx = " + AttachOnShoreIdx);
            _solver.invMasses[AttachOnShoreIdx] = 0;

        }
        // убрать обработчик коллизий и обнулить список CollPoints
        EndCleat();
    }


}

// для моделирования притяжения к объекту одной точкой каната
public class Attractor
{
    public GameObject Fixator;  // к нему притягиваемся
    public Vector3 Pos;         // альтернативный способ задавать точку притяжения, если Fixator=null
    public int FixPoint;        // номер притягиваемого шарика
    public int Interval;        // сколько соседних шариков притягиваются туда же
    public float ForceMult;     // усилитель стандартной (5000) силы притяжения

    public Attractor() { }

    public Attractor(GameObject obj, int fix, int num = 1, float force = 1.0f)
    {
        Fixator = obj;
        FixPoint = fix;
        Interval = num;
        ForceMult = force;
    }
}

