// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Base class for objects that float on water.
    /// </summary>
    public abstract class FloatingObjectBase : MonoBehaviour
    {
        public abstract float ObjectWidth { get; }
        public abstract bool InWater { get; }
        public abstract Vector3 Velocity { get; }

        /// <summary>
        /// The ocean data has horizontal displacements. This represents the displacement that lands at this object position.
        /// </summary>
        public abstract Vector3 CalculateDisplacementToObject();
    }
}
