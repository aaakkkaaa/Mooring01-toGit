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
    private float springStiffness = 5000;
    private float springDamping = 50;
    private int Interval = 4;        // на сколько еще частиц в обе стороны применить силу
    [NonSerialized]
    public List<int> FixPoints;         // точки, притягиваемые к руке
    [NonSerialized]
    public GameObject Fixator;          // объект, к которому идет притяжение

    private ObiSolver _solver;
    private ObiRope _rope;

    // для выбора поведения в FixedUpdate
    [NonSerialized]
    public string[] States = { "ONEPOINT", "MANYPOINTS", "FREE" };
    [NonSerialized]
    public string CurState = "FREE";    // ONEPOINT, MANYPOINTS, FREE


    private void Awake()
    {
        FixPoints = new List<int>();
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


        if (CurState == "ONEPOINT")
        {
            if(Fixator != null)
            {

            }
        }
        else if(CurState == "MANYPOINTS")
        {
            if (Fixator != null)
            {
                Vector4 targetPosition = _solver.transform.InverseTransformPoint(Fixator.transform.position);
                for (int i=0; i< FixPoints.Count; i++)
                {
                    int particleIndex = FixPoints[i];
                    int pIdx = _rope.solverIndices[particleIndex];
                    // Calculate effective inverse mass:
                    float invMass = _solver.invMasses[pIdx];

                    if (invMass > 0)
                    {
                        // Вычисление и применение силы таскающей пружины:
                        Vector4 position = _solver.positions[pIdx];
                        Vector4 velocity = _solver.velocities[pIdx];
                        Vector4 force = ((targetPosition - position) * springStiffness - velocity * springDamping) / invMass;
                        _solver.externalForces[pIdx] = force;

                        // воздействовать еще на несколько шариков
                        Vector4 dF = force / (Interval + 1);
                        for (int j = 1; j <= Interval; j++)
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

        }

    }

    public void ThrowTo(Vector3 direct)
    {
        Vector3 throwDirection = direct;
        throwDirection.y += throwDirection.magnitude / 2;
        float value = direct.magnitude*2;
        Vector3 solvDir = _solver.transform.InverseTransformDirection(throwDirection);
        for (int i = 0; i < _rope.particleCount; i++)
        {
            int solverIndex = _rope.solverIndices[i];
            _solver.velocities[solverIndex] = solvDir * value;
        }

    }

}
