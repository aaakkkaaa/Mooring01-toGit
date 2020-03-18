// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Base class for scripts that provide the time to the ocean system. See derived classes for examples.
    /// </summary>
    public abstract class TimeProviderBase : MonoBehaviour
    {
        public abstract float CurrentTime { get; }
        public abstract float DeltaTime { get; }

        // Delta time used for dynamics such as the ripple sim
        public virtual float DeltaTimeDynamics => DeltaTime;
    }
}
