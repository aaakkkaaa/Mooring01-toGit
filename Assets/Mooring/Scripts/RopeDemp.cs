using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiActor))]
public class RopeDemp : MonoBehaviour
{
    public float DemperValue = 0.95f;

    private ObiRope _obiR;
    private ObiSolver _solver;

    private void Start()
    {
        // Получить компоненты Rope и Solver
        _obiR = gameObject.GetComponent<ObiRope>();
        _solver = _obiR.solver;
    }

    private void FixedUpdate()
    {
        Demper();
    }


    // Исправить скорости всех частиц, входящих в Rope
    private void Demper()
    {
        if (_obiR == null || !_obiR.isLoaded) return;

        for (int i = 0; i < _obiR.particleCount; i++)
        {
            int solverIndex = _obiR.solverIndices[i];
            Vector3 velocity = _solver.velocities[solverIndex];
            velocity *= DemperValue;
            _solver.velocities[solverIndex] = velocity;
        }
    }

}
