using UnityEngine;

namespace NWH.DWP2.ShipController
{
    [RequireComponent(typeof(Rigidbody))]
    public class Sink : MonoBehaviour
    {
        [Tooltip("Ingress point in local coordinates. Indicated by a red sphere gizmo.")]
        public Vector3 floodedCenterOfMass = Vector3.zero;

        [Tooltip(
            "How much should center of mass drift due to water ingress. 0 = none, 1 = center of mass will move to ingress point, 0-1 = in-between.")]
        [Range(0, 1)]
        public float centerOfMassDriftPercent = 0.4f;

        [Tooltip("Percentage of initial mass that will be added each second to imitate water ingress")]
        public float addedMassPercentPerSecond = 0.1f;

        [Tooltip("Maximum added mass after water ingress. 1f = 100% of orginal mass, 2f = 200% of original mass, etc.")]
        public float maxMassPercent = 3f;

        [Tooltip("Should the ship sink? Call Begin() to initialize sinking from script.")]
        [SerializeField] private bool sink;

        private float     initialMass;
        private Vector3   initialLocalCOM;
        private Rigidbody rb;

        ///
        public Vector3 FloodedCenterOfMass
        {
            get { return transform.TransformPoint(floodedCenterOfMass); }
            set { floodedCenterOfMass = transform.InverseTransformPoint(value); }
        }


        private void Start()
        {
            rb              = GetComponent<Rigidbody>();
            initialLocalCOM = rb.centerOfMass;
            initialMass     = rb.mass;
        }


        private void FixedUpdate()
        {
            if (sink)
            {
                rb.mass += initialMass * addedMassPercentPerSecond * Time.fixedDeltaTime;
                rb.centerOfMass = initialLocalCOM +
                                  Mathf.Clamp01((rb.mass - initialMass) / (maxMassPercent * initialMass)) *
                                  centerOfMassDriftPercent * floodedCenterOfMass;
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(FloodedCenterOfMass, 0.2f);
        }


        public void Begin()
        {
            sink = true;
        }


        public void Stop()
        {
            sink = false;
        }


        public void Reset()
        {
            Stop();
            rb.mass         = initialMass;
            rb.centerOfMass = initialLocalCOM;
        }
    }
}