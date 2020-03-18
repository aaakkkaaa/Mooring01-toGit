// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEditor;
using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Tags this object as an ocean depth provider. Renders depth every frame and should only be used for dynamic objects.
    /// For static objects, use an Ocean Depth Cache.
    /// </summary>
    public class RegisterSeaFloorDepthInput : RegisterLodDataInput<LodDataMgrSeaFloorDepth>
    {
        [SerializeField] bool _assignOceanDepthMaterial = true;

        public override float Wavelength => 0f;

        protected override Color GizmoColor => new Color(1f, 0f, 0f, 0.5f);

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_assignOceanDepthMaterial)
            {
                var rend = GetComponent<Renderer>();
                rend.material = new Material(Shader.Find("Crest/Inputs/Depth/Ocean Depth From Geometry"));
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RegisterSeaFloorDepthInput))]
    public class RegisterSeaFloorDepthInputEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("There is currently a bug in Crest that may prevent Sea Floor Depth inputs being registered correctly. We are actively working to resolve this.\n\nIn the meantime sea floor depth inputs must be provided through a depth cache, similar to how the island landmasses are captured in the PirateCove and main example scenes.", MessageType.Warning);

            base.OnInspectorGUI();
        }
    }

#endif
    }
