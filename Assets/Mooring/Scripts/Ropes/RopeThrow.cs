using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class RopeThrow : MonoBehaviour
{
    public Vector3 ThrowDorection;
    public float ThrowValue;

    public bool doThrow=false;

    private ObiRope _obiR;
    private ObiSolver _solver;

    void Awake()
    {
        _obiR = gameObject.GetComponent<ObiRope>();
    }

    // Update is called once per frame
    void Update()
    {
        if(doThrow)
        {
            DoThrow();
            doThrow = false;
        }
    }

    private void DoThrow()
    {
        if (_obiR == null || !_obiR.isLoaded) return;
        _solver = _obiR.solver;

        ThrowDorection = Vector3.Normalize(ThrowDorection) * ThrowValue;

        for (int i = 0; i < _obiR.particleCount; i++)
        {
            int solverIndex = _obiR.solverIndices[i];
            _solver.velocities[solverIndex] = ThrowDorection;
        }


    }

}
