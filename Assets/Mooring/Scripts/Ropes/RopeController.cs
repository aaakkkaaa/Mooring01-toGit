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
    public List<int> FixPoints;         // точки, притягиваемые к руке
    [NonSerialized]
    public GameObject Fixator;          // объект, к которому идет притяжение
    [NonSerialized]
    public int FixPoint2;               // точка, притягиваемая ко второму фиксатору
    [NonSerialized]
    public GameObject Fixator2;         // другой объект, к которому идет притяжение

    private ObiSolver _solver;
    private ObiRope _rope;

    // для выбора поведения в FixedUpdate
    [NonSerialized]
    public string[] States = { "ONEPOINT", "MANYPOINTS", "FREE", "FLY_TO", "MANY_AND_ONE" };
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


        if (CurState == "MANY_AND_ONE")
        {
            if(Fixator2 != null && FixPoint2>0)
            {
                Vector4 targetPosition2 = _solver.transform.InverseTransformPoint(Fixator2.transform.position);
                int pIdx = _rope.solverIndices[FixPoint2];
                float invMass = _solver.invMasses[pIdx];
                if (invMass > 0)
                {
                    // Вычисление и применение силы таскающей пружины:
                    Vector4 position = _solver.positions[pIdx];
                    Vector4 velocity = _solver.velocities[pIdx];
                    Vector4 force = ((targetPosition2 - position) * _springStiffness - velocity * _springDamping) / invMass;
                    _solver.externalForces[pIdx] = force;
                }
            }
        }
        if(CurState == "MANYPOINTS" || CurState == "MANY_AND_ONE")
        {
            if (Fixator != null)
            {
                Vector4 targetPosition = _solver.transform.InverseTransformPoint(Fixator.transform.position);
                for (int i=0; i< FixPoints.Count; i++)
                {
                    //print("Rope - " + CurState + " " + FixPoints.Count);
                    int particleIndex = FixPoints[i];
                    int pIdx = _rope.solverIndices[particleIndex];
                    // Calculate effective inverse mass:
                    float invMass = _solver.invMasses[pIdx];
                    if (invMass > 0)
                    {
                        // Вычисление и применение силы таскающей пружины:
                        Vector4 position = _solver.positions[pIdx];
                        Vector4 velocity = _solver.velocities[pIdx];
                        Vector4 force = ((targetPosition - position) * _springStiffness - velocity * _springDamping) / invMass;
                        _solver.externalForces[pIdx] = force;

                        // воздействовать еще на несколько шариков
                        Vector4 dF = force / (_intervalSpring + 1);
                        for (int j = 1; j <= _intervalSpring; j++)
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
        if(CurState == "FLY_TO")
        {
            if (Fixator != null)
            {
                Vector4 targetPosition = _solver.transform.InverseTransformPoint(Fixator.transform.position);
                //print(_rope.particleCount);
                //print("FixPoints.Count = " + FixPoints.Count);
                for (int i = 0; i < FixPoints.Count; i++)
                {
                    int particleIndex = FixPoints[i];
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

    }

    // переместить точки каната от 0 до idx так, чтобы они были дальше от t2 чем t1 и выстроены по линии
    public void MoveTo(Vector3 t1, Vector3 t2, int idx )
    {
        float[] ds = new float[idx];
        ds[0] = 0;
        // на каком расстоянии друг от друга сейчас точки
        for(int i=1; i< idx; i++)
        {
            int pIdx0 = _rope.solverIndices[ _rope.activeParticleCount - i - 1 ];
            int pIdx1 = _rope.solverIndices[ _rope.activeParticleCount - i ];
            ds[i] = (_solver.positions[pIdx0] - _solver.positions[pIdx1]).magnitude;
        }
        // разместим эти точки на прямой
        float s = 0;
        for (int i = 0; i < idx; i++)
        {
            s += ds[i];
            int pIdx = _rope.solverIndices[ _rope.activeParticleCount - 1 - i ];
            Vector3 newPos = t1 + (t1 - t2) * s;
            _solver.positions[pIdx] = newPos;
            _solver.velocities[pIdx] = Vector3.zero;
        }


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

}
