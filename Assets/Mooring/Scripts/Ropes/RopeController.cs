using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;
using System;
using Obi;

// Взаимодействие с персонажами, броски, закрепление
public class RopeController : MonoBehaviour
{
    public float Stretch = 1.3f;
    public float MaxForce = 20000;

    // скрипт меша - имитатор каната
    public RopeTrick Trick;

    // для таскания рукой за одну или несколько точек
    private float _springStiffness = 5000;
    private float _springDamping = 50;
    private int _intervalSpring = 4;        // на сколько еще частиц в обе стороны применить силу
    private int _intervalFly = 8;

    [NonSerialized]
    public List<Attractor> Attractors;  // описывание притяжения: к какому объекту какая частица каната притягивается

    [NonSerialized]
    public List<Connector> Connectors;  // описывание аттача: к какому объекту какая частица каната присоединена

    [NonSerialized]
    public float ThrowDistance = 3.5f;  // максимально возможное расстояние для броска
    [NonSerialized]
    public float FreeDistance = 10.0f;   // если расстояние больше - прекращаем ожидание броска, отпускаем канат

    [NonSerialized]
    public List<int> FlyPoints;         // точки, притягиваемые к цели полета
    [NonSerialized]
    public GameObject FlyTarget;          // объект, к которому идет притяжение в полете

    private ObiSolver _solver;
    private ObiRope _rope;
    private YachtSolver _yacht;

    public int MaxRopeIdx { get { return _rope.activeParticleCount; } }

    // для выбора поведения в FixedUpdate
    [NonSerialized]
    public string[] States = { "FREE", "FLY_TO", "ATTRACT", "ATTACH"};
    [NonSerialized]
    public string CurState = "FREE";    // ONEPOINT, MANYPOINTS, FREE

    // утка, с которой взаимодействует канат, передается из marinero и из sailor
    private GameObject _workCleat;
    // точки в утке, вдоль которых должен стараться пролечь канат (в системе координат солвера)
    private Transform _target1;
    private Transform _target2;
    // коллайдер утки, с которым будет отлавливаться взаимодействие
    private Collider _coll;
    // диапазон номеров частиц, которые нужно обрабатывать после коллайдинга с уткой
    private int _minIdx;
    private int _maxIdx;

    // если не null, то делается выстраивание точек каната вдоль оси утки
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
        _yacht = GameObject.Find("TrainingVessel").GetComponent<YachtSolver>();
    }


    private void FixedUpdate()
    {
        if (CurState == "" || CurState == "FREE") return;
        if (_solver == null)
        {
            _rope = GetComponent<ObiRope>();
            _solver = _rope.solver;
        }

        if( transform.parent != _solver.transform )
        {
            return;
        }

        if (CurState == "ATTRACT" && gameObject.transform.parent == _solver.transform )
        {
            for (int i = 0; i < Attractors.Count; i++)
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

        if (CurState == "FLY_TO")
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

        // притянуть частицы к оси утки
        if (CollPoints != null)
        {
            Vector3 _tg1 = _target1.position;
            Vector3 _tg2 = _target2.position;
            Vector4 _t1 = _rope.solver.transform.InverseTransformPoint(_tg1);
            Vector4 _t2 = _rope.solver.transform.InverseTransformPoint(_tg2);

            for (int i = 0; i < CollPoints.Count; i++)
            {
                int pIdx = CollPoints[i];
                if(pIdx < _minIdx || pIdx > _maxIdx)
                {
                    break;
                }
                int idx = _rope.solverIndices[pIdx];
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
                    //_solver.externalForces[idx] = force / 3;
                    _solver.externalForces[idx] = force;
                    //print(idx + " ->  " + force);
                }
            }
            //print("-----------------");
        }

        // держать на своих местах частицы жестко присоединенные к кому-то
        if (Connectors != null)
        {
            for (int i = 0; i < Connectors.Count; i++)
            {
                Connector con = Connectors[i];
                int idx = _rope.solverIndices[con.FixPoint];
                Vector3 pos = _rope.solver.transform.InverseTransformPoint(con.Fixator.transform.position);
                _solver.positions[idx] = pos;
                //print(con.Fixator.name);
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

    // передаем утку, из нее извлекаем все нужное, включаем коллайдер для определения частиц каната
    public void BeginCleat(GameObject cleat, int minI, int maxI)
    {
        print("BeginCleat() -> " + cleat.name + "  minI = " + minI + "  maxI = " + maxI );
        _workCleat = cleat;
        // выстроить крайние точки каната по линии, чтобы потом продеть в утку
        _target1 = _workCleat.transform.Find("Target1");
        _target2 = _workCleat.transform.Find("Target2");

        // ищем коллайдер, с которым будем взаимодействовать
        _coll = _workCleat.transform.Find("Zona").gameObject.GetComponent<Collider>();

        CollPoints = new List<int>();
        // диапазон номеров частиц, которые нужно обрабатывать
        _minIdx = minI;
        _maxIdx = maxI;
        _rope.solver.OnCollision += SolverOnCleatCollision;
    }

    public void EndCleat()
    {
        print("EndCleat()");
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
                            CollPoints.Add(pa.indexInActor);
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

    public void SetAttractors( GameObject obj, int[] points, int interv = 1, float force = 1.0f)
    {
        Attractors.Clear();
        for(int i=0; i<points.Length; i++)
        {
            Attractor attr = new Attractor(obj, points[i], interv, force);
            Attractors.Add(attr);
        }
    }

    // удалить из аттракторов те, которые притягивают к obj
    public void RemoveAttractors(GameObject obj)
    {
        print("RemoveAttractors -> было: " + Attractors.Count);
        List<Attractor> tmp = new List<Attractor>();
        for(int i=0; i<Attractors.Count; i++)
        {
            if (Attractors[i].Fixator != obj)
            {
                tmp.Add(Attractors[i]);
            }
        }
        Attractors = tmp;
        print("RemoveAttractors -> стало: " + Attractors.Count);
    }

    // зафиксировать на утке частицу каната
    public void AttachPointToCleat()
    {
        print("Rope.AttachPointToCleat() -> " + _workCleat.name );
        print("CollPoints = " + CollPoints);
        if(CollPoints.Count == 0)
        {
            print("Невозможно соединится с уткой, нет подходящих точек!");
        }
        else
        {
            AttachOnShoreIdx = CollPoints[(CollPoints.Count - 1)/ 2];
            //CurState = "ATTACH";
            print("AttachOnShoreIdx = " + AttachOnShoreIdx);
            GameObject center = _workCleat.transform.Find("Center").gameObject;
            AddConnect(center, AttachOnShoreIdx);
            //_solver.invMasses[AttachOnShoreIdx] = 0;

        }
        // убрать обработчик коллизий и обнулить список CollPoints
        EndCleat();
    }

    // добавление жесткой фиксации
    public void AddConnect(GameObject obj, int fix)
    {
        if (Connectors == null)
        {
            Connectors = new List<Connector>();
        }
        Connector con = new Connector(obj, fix);
        int pIdx = _rope.solverIndices[fix];
        con.InvMass = _solver.invMasses[pIdx];  // сохранили для восстановления свободы частицы
        _solver.invMasses[pIdx] = 0;
        Connectors.Add(con);
   }

    // удаление жесктой фиксации
    public void DelConnect(int fix)
    {
        Connector con=null;
        if (Connectors != null)
        {
            int pIdx = _rope.solverIndices[fix];
            for(int i=0; i<Connectors.Count; i++)
            {
                con = Connectors[i];
                if(con.FixPoint == fix )
                {
                    _solver.invMasses[pIdx] = con.InvMass;
                }
            }
            if(con != null )
            {
                Connectors.Remove(con);
            }
        }
    }

    public void ShowTrickRope()
    {
        GameObject knot = _workCleat.gameObject.transform.Find("RopeKnot").gameObject;
        knot.SetActive(true);
        if(Trick != null)
        {
            // показать имитацию каната
            Trick.gameObject.SetActive(true);
            // спрятать канат ObiRope
            //transform.SetParent(GameObject.Find("BakedRope").transform);
            gameObject.SetActive(false);
            // запустить физику
        }
        else
        {
            print("Нет объекта Trick");
        }
    }


    public int MaxPointIdx()
    {
        if( _rope == null )
        {
            _rope = GetComponent<ObiRope>();
            _solver = _rope.solver;
        }
        return _rope.activeParticleCount-1;
    }

    public bool CanThrow(GameObject marinero, GameObject sailor, float controlTime = 0)
    {
        print("RopeController.CanThrow( " + marinero.name + ", " + sailor.name + ", " + controlTime + " )");

        Vector3 distVec = sailor.transform.position - marinero.transform.position;
        print("distVec = " + distVec + "    distVec.magnitude = " + distVec.magnitude);
        if (distVec.magnitude > ThrowDistance)
        {
            return false;
        }
        Vector3 yachtV = _yacht.transform.TransformVector(new Vector3(_yacht.Vx, 0, _yacht.Vz));
        print("yachtV = " + yachtV);

        // проверим дистанцию с учетом скорости
        if( (distVec + yachtV * controlTime).magnitude > ThrowDistance)
        {
            return false;
        }

        return true;
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

    public Attractor(GameObject obj, int fix, int interv = 1, float force = 1.0f)
    {
        Fixator = obj;
        FixPoint = fix;
        Interval = interv;
        ForceMult = force;
    }
}

public class Connector
{
    public GameObject Fixator;  // к нему присоединены 
    public int FixPoint;        // номер притягиваемого шарика
    public float InvMass;       // сохранение старой инверсной массы, для восстановления

    public Connector(GameObject obj, int fix)
    {
        Fixator = obj;
        FixPoint = fix;
    }
}

