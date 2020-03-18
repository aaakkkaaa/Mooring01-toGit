// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Assign this to depth masks - objects that will occlude the water. This ensures that the mask will render before any of the ocean surface.
    /// </summary>
    public class RegisterMaskInput : MonoBehaviour
    {
        void Start()
        {
            // Render before the surface mesh
            GetComponent<Renderer>().sortingOrder = -LodDataMgr.MAX_LOD_COUNT - 1;
        }
    }
}
