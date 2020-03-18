// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Default time provider - sets the ocean time to Unity's game time.
    /// </summary>
    public class TimeProviderDefault : TimeProviderBase
    {
        public override float CurrentTime => Time.time;
        public override float DeltaTime => Time.deltaTime;
    }
}
