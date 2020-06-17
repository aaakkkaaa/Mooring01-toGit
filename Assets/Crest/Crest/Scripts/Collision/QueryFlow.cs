﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Samples horizontal motion of water volume
    /// </summary>
    public class QueryFlow : QueryBase
    {
        readonly int sp_LD_TexArray_Flow = Shader.PropertyToID("_LD_TexArray_Flow");
        readonly int sp_ResultFlows = Shader.PropertyToID("_ResultFlows");

        protected override string QueryShaderName => "QueryFlow";
        protected override string QueryKernelName => "CSMain";

        public static QueryFlow Instance { get; private set; }

        protected override void OnEnable()
        {
            Instance = this;

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            // We don't set Instance to null here because it breaks exiting play mode, as OnDisable is called but no matching call to OnEnable :/.
            // This would probably be better if the Query system did not inherit from MonoBehaviour and was built up by the OceanRenderer..

            base.OnDisable();
        }

        protected override void BindInputsAndOutputs(PropertyWrapperComputeStandalone wrapper, ComputeBuffer resultsBuffer)
        {
            if (OceanRenderer.Instance._lodDataFlow != null)
            {
                OceanRenderer.Instance._lodDataFlow.BindResultData(wrapper);
                ShaderProcessQueries.SetTexture(_kernelHandle, sp_LD_TexArray_Flow, OceanRenderer.Instance._lodDataFlow.DataTexture);
            }
            ShaderProcessQueries.SetBuffer(_kernelHandle, sp_ResultFlows, resultsBuffer);
        }

        public int Query(int i_ownerHash, float i_minSpatialLength, Vector3[] i_queryPoints, Vector3[] o_resultFlows)
        {
            return Query(i_ownerHash, i_minSpatialLength, i_queryPoints, o_resultFlows, null, null);
        }

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        static void InitStatics()
        {
            // Init here from 2019.3 onwards
            Instance = null;
        }
    }
}
