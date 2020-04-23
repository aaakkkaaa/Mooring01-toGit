// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// Three different versions of this:
// - Built-in pipeline - PopulateCache() called from Start()
// - URP/LWRP - RenderSingleCamera() API is used. Requires a RenderContext, so happens in BeginCameraRendering
// - HDRP - No RenderSingleCamera API it seems, and I dont think calling PopulateCache() on Start() worked, so PopulateCache() called from Update()

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Crest
{
    /// <summary>
    /// Renders terrain height / ocean depth once into a render target to cache this off and avoid rendering it every frame.
    /// This should be used for static geometry, dynamic objects should be tagged with the Render Ocean Depth component.
    /// </summary>
    public class OceanDepthCache : MonoBehaviour, IValidated
    {
        public enum OceanDepthCacheType
        {
            Realtime,
            Baked,
        }

        public enum OceanDepthCacheRefreshMode
        {
            OnFirstRender,
            OnDemand,
        }

        [Tooltip("Realtime = cache will be dynamic in accordance to refresh mode, Baked = cache will use the provided texture."), SerializeField]
        OceanDepthCacheType _type = OceanDepthCacheType.Realtime;
        public OceanDepthCacheType Type => _type;

        [Tooltip("Ignored if baked. On Start = cache will populate in Start(), On Demand = call PopulateCache() manually via scripting."), SerializeField]
        OceanDepthCacheRefreshMode _refreshMode = OceanDepthCacheRefreshMode.OnFirstRender;
        public OceanDepthCacheRefreshMode RefreshMode => _refreshMode;

        [Tooltip("The layers to render into the depth cache."), SerializeField]
        string[] _layerNames = null;

        [Tooltip("The resolution of the cached depth - lower will be more efficient."), SerializeField]
        int _resolution = 512;

        // A big hill will still want to write its height into the depth texture
        [Tooltip("The 'near plane' for the depth cache camera (top down)."), SerializeField]
        float _cameraMaxTerrainHeight = 100f;

        [Tooltip("Will render into the cache every frame. Intended for debugging, will generate garbage."), SerializeField]
#pragma warning disable 414
        bool _forceAlwaysUpdateDebug = false;
#pragma warning restore 414

        [Tooltip("Baked depth cache. Baking button available in play mode."), SerializeField]
#pragma warning disable 649
        Texture2D _savedCache;
#pragma warning restore 649
        public Texture2D SavedCache => _savedCache;

#pragma warning disable 414
        [Tooltip("Editor only: run validation checks on Start() to check for issues."), SerializeField]
        bool _runValidationOnStart = true;
#pragma warning restore 414

        RenderTexture _cacheTexture;
        public RenderTexture CacheTexture => _cacheTexture;

        GameObject _drawCacheQuad;
        Camera _camDepthCache;

        void Start()
        {
            if (OceanRenderer.Instance == null)
            {
                enabled = false;
                return;
            }

#if UNITY_EDITOR
            if (_runValidationOnStart)
            {
                Validate(OceanRenderer.Instance);
            }
#endif

            if (_type == OceanDepthCacheType.Baked)
            {
                InitCacheQuad();
            }
        }

        private void OnEnable()
        {
            if (_type == OceanDepthCacheType.Realtime && _refreshMode == OceanDepthCacheRefreshMode.OnFirstRender)
            {
                RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            }
        }
        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        }

        RenderTexture MakeRT(bool depthStencilTarget)
        {
            RenderTextureFormat fmt;

            if (depthStencilTarget)
            {
                fmt = RenderTextureFormat.Depth;
            }
            else
            {
#if UNITY_EDITOR_WIN
                fmt = RenderTextureFormat.DefaultHDR;
#else
                fmt = RenderTextureFormat.RHalf;
#endif
            }

            Debug.Assert(SystemInfo.SupportsRenderTextureFormat(fmt), "The graphics device does not support the render texture format " + fmt.ToString());
            var result = new RenderTexture(_resolution, _resolution, depthStencilTarget ? 24 : 0);
            result.name = gameObject.name + "_oceanDepth_" + (depthStencilTarget ? "DepthOnly" : "Cache");
            result.format = fmt;
            result.useMipMap = false;
            result.anisoLevel = 0;
            return result;
        }

        bool InitObjects()
        {
            if (_cacheTexture == null)
            {
                _cacheTexture = MakeRT(false);
            }

            if (_camDepthCache == null)
            {
                var errorShown = false;
                var layerMask = 0;
                foreach (var layer in _layerNames)
                {
                    int layerIdx = LayerMask.NameToLayer(layer);
                    if (string.IsNullOrEmpty(layer) || layerIdx == -1)
                    {
                        Debug.LogError("OceanDepthCache: Invalid layer specified: \"" + layer +
                        "\". Does this layer need to be added to the project (Edit/Project Settings/Tags and Layers)? Click this message to highlight the cache in question.", this);
                        errorShown = true;
                    }
                    else
                    {
                        layerMask = layerMask | (1 << layerIdx);
                    }
                }

                if (layerMask == 0)
                {
                    if (!errorShown)
                    {
                        Debug.LogError("No valid layers for populating depth cache, aborting.", this);
                    }

                    return false;
                }

                _camDepthCache = new GameObject("DepthCacheCam").AddComponent<Camera>();
                _camDepthCache.transform.position = transform.position + Vector3.up * _cameraMaxTerrainHeight;
                _camDepthCache.transform.parent = transform;
                _camDepthCache.transform.localEulerAngles = 90f * Vector3.right;
                _camDepthCache.orthographic = true;
                _camDepthCache.orthographicSize = Mathf.Max(transform.lossyScale.x / 2f, transform.lossyScale.z / 2f);
                _camDepthCache.cullingMask = layerMask;
                _camDepthCache.clearFlags = CameraClearFlags.SolidColor;
                // Clear to 'very deep'
                _camDepthCache.backgroundColor = Color.white * 1000f;
                _camDepthCache.enabled = false;
                _camDepthCache.allowMSAA = false;
                _camDepthCache.allowDynamicResolution = false;
                
                // Stops behaviour from changing in VR. I tried disabling XR before/after camera render but it makes the editor
                // go bonkers with split windows.
                _camDepthCache.cameraType = CameraType.Reflection;
                _camDepthCache.gameObject.SetActive(false);

                var additionalCameraData = _camDepthCache.gameObject.AddComponent<UniversalAdditionalCameraData>();
                additionalCameraData.renderShadows = false;
                additionalCameraData.requiresColorTexture = false;
                additionalCameraData.requiresDepthTexture = false;
                additionalCameraData.renderPostProcessing = false;
            }

            if (_camDepthCache.targetTexture == null)
            {
                _camDepthCache.targetTexture = MakeRT(true);
            }

            InitCacheQuad();

            return true;
        }

        void InitCacheQuad()
        {
            Renderer qr;

            if (_drawCacheQuad == null)
            {
                _drawCacheQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _drawCacheQuad.hideFlags = HideFlags.DontSave;
                Destroy(_drawCacheQuad.GetComponent<Collider>());
                _drawCacheQuad.name = "DepthCache_" + gameObject.name + "_NOSAVE";
                _drawCacheQuad.transform.SetParent(transform, false);
                _drawCacheQuad.transform.localEulerAngles = 90f * Vector3.right;
                _drawCacheQuad.AddComponent<RegisterSeaFloorDepthInput>();
                qr = _drawCacheQuad.GetComponent<Renderer>();
                qr.material = new Material(Shader.Find(LodDataMgrSeaFloorDepth.ShaderName));
            }
            else
            {
                qr = _drawCacheQuad.GetComponent<Renderer>();
            }

            if (_type == OceanDepthCacheType.Baked)
            {
                qr.material.mainTexture = _savedCache;
            }
            else
            {
                qr.material.mainTexture = _cacheTexture;
            }

            qr.enabled = false;
        }

        // We populate the cache here because we need a ScriptableRenderContext in order to populate the cache. Pain in the neck!
        public void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (_type == OceanDepthCacheType.Baked)
            {
                RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
                return;
            }

            // Make sure we have required objects
            if (!InitObjects())
            {
                enabled = false;
                RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
                return;
            }

            // Render scene, saving depths in depth buffer
            UnityEngine.Rendering.Universal.UniversalRenderPipeline.RenderSingleCamera(context, _camDepthCache);

            Material copyDepthMaterial = new Material(Shader.Find("Crest/Copy Depth Buffer Into Cache"));

            copyDepthMaterial.SetVector("_OceanCenterPosWorld", OceanRenderer.Instance.transform.position);

            copyDepthMaterial.SetTexture("_CamDepthBuffer", _camDepthCache.targetTexture);

            // Zbuffer params
            //float4 _ZBufferParams;            // x: 1-far/near,     y: far/near, z: x/far,     w: y/far
            float near = _camDepthCache.nearClipPlane, far = _camDepthCache.farClipPlane;
            copyDepthMaterial.SetVector("_CustomZBufferParams", new Vector4(1f - far / near, far / near, (1f - far / near) / far, (far / near) / far));

            // Altitudes for near and far planes
            float ymax = _camDepthCache.transform.position.y - near;
            float ymin = ymax - far;
            copyDepthMaterial.SetVector("_HeightNearHeightFar", new Vector2(ymax, ymin));

            // Copy from depth buffer into the cache
            Graphics.Blit(null, _cacheTexture, copyDepthMaterial);

            if (!_forceAlwaysUpdateDebug)
            {
                _camDepthCache.targetTexture = null;
                RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            }
        }

        public void PopulateCache()
        {
            // Register for render callback which will trigger population
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 0f, 1f));

            if (_type == OceanDepthCacheType.Realtime)
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
                Gizmos.DrawCube(Vector3.up * _cameraMaxTerrainHeight / transform.lossyScale.y, new Vector3(1f, 0f, 1f));
            }
        }
#endif

        public bool Validate(OceanRenderer ocean)
        {
            if (_type == OceanDepthCacheType.Baked)
            {
                if (_savedCache == null)
                {
                    Debug.LogError("Validation: Depth cache type is 'Saved Cache' but no saved cache data is provided. Click this message to highlight the cache in question.", this);
                }
            }
            else
            {
                if ((_layerNames == null || _layerNames.Length == 0))
                {
                    Debug.LogError("Validation: No layers specified for rendering into depth cache, and no geometries manually provided. Click this message to highlight the cache in question.", this);
                }

                if (_forceAlwaysUpdateDebug)
                {
                    Debug.LogWarning("Validation: Force Always Update Debug option is enabled on depth cache " + gameObject.name + ", which means it will render every frame instead of running from the cache. Click this message to highlight the cache in question.", this);
                }

                foreach (var layerName in _layerNames)
                {
                    var layer = LayerMask.NameToLayer(layerName);
                    if (layer == -1)
                    {
                        Debug.LogError("Invalid layer specified for objects/geometry providing the ocean depth: \"" + layerName +
                            "\". Does this layer need to be added to the project (Edit/Project Settings/Tags and Layers)? Click this message to highlight the cache in question.", this);
                    }
                }

                if (_resolution < 4)
                {
                    Debug.LogError("Cache resolution " + _resolution + " is very low. Is this intentional? Click this message to highlight the cache in question.", this);
                }

                // We used to test if nothing is present that would render into the cache, but these could probably come from other scenes, and AssignLayer means
                // objects can be tagged up at run-time.
            }

            if (transform.lossyScale.magnitude < 5f)
            {
                Debug.LogWarning("Validation: Ocean depth cache transform scale is small and will capture a small area of the world. The scale sets the size of the area that will be cached, and this cache is set to render a very small area. Click this message to highlight the cache in question.", this);
            }

            if (transform.lossyScale.y < 0.001f || transform.localScale.y < 0.01f)
            {
                Debug.LogError($"Validation: Ocean depth cache scale Y should be set to 1.0. Its current scale in the hierarchy is {transform.lossyScale.y}.", this);
            }

            if (Mathf.Abs(transform.position.y - ocean.transform.position.y) > 0.00001f)
            {
                Debug.LogWarning("Validation: It is recommended that the cache is placed at the same height (y component of position) as the ocean, i.e. at the sea level. If the cache is created before the ocean is present, the cache height will inform the sea level. Click this message to highlight the cache in question.", this);
            }

            var rend = GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                Debug.LogWarning("Validation: It is not expected that a depth cache object has a Renderer component in its hierarchy. The cache is typically attached to an empty GameObject. Click this message to highlight the Renderer. Please refer to the example content.", rend);
            }

            return true;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OceanDepthCache))]
    public class OceanDepthCacheEditor : Editor
    {
        readonly string[] _propertiesToExclude = new string[] { "m_Script", "_type", "_refreshMode", "_savedCache", "_layerNames", "_resolution", "_cameraMaxTerrainHeight", "_forceAlwaysUpdateDebug" };

        public override void OnInspectorGUI()
        {
            // We won't just use default inspector because we want to show some of the params conditionally based on cache type

            // First show standard 'Script' field
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;

            // Next expose cache type and refresh mode

            var typeProp = serializedObject.FindProperty("_type");
            EditorGUILayout.PropertyField(typeProp);

            var cacheType = (OceanDepthCache.OceanDepthCacheType)typeProp.intValue;

            if (cacheType == OceanDepthCache.OceanDepthCacheType.Realtime)
            {
                // Only expose the following if real-time cache type
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_refreshMode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_layerNames"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_resolution"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_cameraMaxTerrainHeight"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_forceAlwaysUpdateDebug"));
            }
            else
            {
                // Only expose saved cache if non-real-time
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_savedCache"));
            }

            // Draw rest of inspector fields
            DrawPropertiesExcluding(serializedObject, _propertiesToExclude);

            // Apply inspector changes
            serializedObject.ApplyModifiedProperties();

            var playing = EditorApplication.isPlaying;

            var dc = target as OceanDepthCache;
            var isOnDemand = cacheType == OceanDepthCache.OceanDepthCacheType.Realtime &&
                dc.RefreshMode == OceanDepthCache.OceanDepthCacheRefreshMode.OnDemand;
            var isBakeable = cacheType == OceanDepthCache.OceanDepthCacheType.Realtime &&
                (!isOnDemand || dc.CacheTexture != null);

            if (playing && isOnDemand && GUILayout.Button("Populate cache"))
            {
                dc.PopulateCache();
            }

            if (playing && isBakeable && GUILayout.Button("Save cache to file"))
            {
                var rt = dc.CacheTexture;
                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                RenderTexture.active = null;

                byte[] bytes;
                bytes = tex.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);

                string path = dc.SavedCache ?
                    AssetDatabase.GetAssetPath(dc.SavedCache) : $"Assets/OceanDepthCache_{Guid.NewGuid()}.exr";
                System.IO.File.WriteAllBytes(path, bytes);
                AssetDatabase.ImportAsset(path);

                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                ti.textureType = TextureImporterType.SingleChannel;
                ti.sRGBTexture = false;
                ti.alphaSource = TextureImporterAlphaSource.None;
                ti.alphaIsTransparency = false;
                ti.SaveAndReimport();

                Debug.Log("Cache saved to " + path, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
            }
        }
    }
#endif
}
