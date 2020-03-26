using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using System.Security.Cryptography;

public class DetectCol : MonoBehaviour
{
    public ObiSolver solver;
    //public Transform marker;

    // список всех столкновений случившихся на этом шаге
    private List<ActiveCollider> _actCol;

    // утки, к которым привязаны канаты
    private Cleat[] _cleats;

    private void Awake()
    {
        _actCol = new List<ActiveCollider>();
        _cleats = GameObject.FindObjectsOfType<Cleat>();

        // найти и передать уткам все коллайдеры самой яхты
        ObiCollider[] obiCols = GameObject.FindObjectsOfType<ObiCollider>();
        Collider[] yachtCols = new Collider[obiCols.Length];
        for(int i=0; i<obiCols.Length; i++)
        {
            yachtCols[i] = obiCols[i].SourceCollider;
        }
        for(int i=0; i<_cleats.Length; i++)
        {
            _cleats[i].yachtCols = yachtCols;
        }
    }

    void OnEnable()
    {
        solver.OnCollision += SolverOnCollision;
    }

    void OnDisable()
    {
        solver.OnCollision -= SolverOnCollision;
    }

    private void FixedUpdate()
    {
        // все обнаруженные коллайдеры раздать Уткам, к которым привязаны соотв. канаты
        for(int i=0; i< _cleats.Length; i++)
        {
            for(int k=0; k < _cleats[i].Ropes.Count; k++)
            {
                _cleats[i].Ropes[k].actCol.Clear();
                for (int j = 0; j < _actCol.Count; j++)
                {

                    if (_cleats[i].Ropes[k].obiRope == _actCol[j].actor)
                    {
                        _cleats[i].Ropes[k].actCol.Add(_actCol[j]);
                    }
                }

            }
            // когда для утки сформированы все коллайдеры, пусть рисует и рассчитывает силы
            _cleats[i].SolveAllRopes();
        }
       

        // очистить коллайдеры для нового заполнения в SolverOnCollision
        _actCol.Clear();
    }

    void SolverOnCollision(object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
    {
        foreach (Oni.Contact contact in e.contacts)
        {
            // this one is an actual collision:
            if (contact.distance < 0.01)
            {
                ActiveCollider ac = new ActiveCollider();
                ObiSolver.ParticleInActor pa = solver.particleToActor[contact.particle];
                ac.actor = pa.actor;
                ac.idxInAct = pa.indexInActor;
                ac.idxInSol = contact.particle;
                Component collider;
                if (ObiCollider.idToCollider.TryGetValue(contact.other, out collider))
                {
                    Vector3 SolvColPos = new Vector3(contact.point[0], contact.point[1], contact.point[2]);
                    //print("Куб: Обнаружен контакт: " + contact.particle + " " + contact.point);
                    Vector3 WorldColPos = solver.gameObject.transform.TransformPoint(SolvColPos);
                    //marker.position = WorldColPos;
                    ac.pos = WorldColPos;
                    if( collider is Collider) ac.col = collider as Collider;
                }
                _actCol.Add(ac);
            }
        }
    }

    
}


public class ActiveCollider
{
    public ObiActor actor;
    public int idxInAct;
    public int idxInSol;
    public Collider col;
    public Vector3 pos;

}

