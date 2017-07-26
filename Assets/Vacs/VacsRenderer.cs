using UnityEngine;

namespace Vacs
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Vacs/Renderer")]
    public sealed class VacsRenderer : MonoBehaviour
    {
        #region Exposed attributes

        [SerializeField] VacsData _data;

        #endregion

        #region Hidden attributes

        [SerializeField, HideInInspector] ComputeShader _computeNoise;
        [SerializeField, HideInInspector] ComputeShader _computeDigitize;

        #endregion

        #region Private objects

        ComputeBuffer _positionSource;
        ComputeBuffer _positionBuffer1;
        ComputeBuffer _positionBuffer2;

        ComputeBuffer _normalSource;
        ComputeBuffer _normalBuffer1;
        ComputeBuffer _normalBuffer2;

        ComputeBuffer _tangentSource;
        ComputeBuffer _tangentBuffer1;
        ComputeBuffer _tangentBuffer2;

        MaterialPropertyBlock _propertyBlock;

        #endregion

        #region Internal resource handling

        void SetupVertices()
        {
            if (_positionSource  == null) _positionSource  = _data.CreatePositionBuffer();
            if (_positionBuffer1 == null) _positionBuffer1 = _data.CreatePositionBuffer();
            if (_positionBuffer2 == null) _positionBuffer2 = _data.CreatePositionBuffer();

            if (_normalSource  == null) _normalSource  = _data.CreateNormalBuffer();
            if (_normalBuffer1 == null) _normalBuffer1 = _data.CreateNormalBuffer();
            if (_normalBuffer2 == null) _normalBuffer2 = _data.CreateNormalBuffer();

            if (_tangentSource  == null) _tangentSource  = _data.CreateTangentBuffer();
            if (_tangentBuffer1 == null) _tangentBuffer1 = _data.CreateTangentBuffer();
            if (_tangentBuffer2 == null) _tangentBuffer2 = _data.CreateTangentBuffer();
        }

        void ReleaseVertices()
        {
            if (_positionSource  != null) _positionSource .Release();
            if (_positionBuffer1 != null) _positionBuffer1.Release();
            if (_positionBuffer2 != null) _positionBuffer2.Release();

            if (_normalSource  != null) _normalSource .Release();
            if (_normalBuffer1 != null) _normalBuffer1.Release();
            if (_normalBuffer2 != null) _normalBuffer2.Release();

            if (_tangentSource  != null) _tangentSource .Release();
            if (_tangentBuffer1 != null) _tangentBuffer1.Release();
            if (_tangentBuffer2 != null) _tangentBuffer2.Release();

            _positionSource  = _normalSource  = _tangentSource  = null;
            _positionBuffer1 = _normalBuffer1 = _tangentBuffer1 = null;
            _positionBuffer2 = _normalBuffer2 = _tangentBuffer2 = null;
        }

        void ApplyCompute(
            ComputeShader compute,
            ComputeBuffer positionSource, ComputeBuffer normalSource, ComputeBuffer tangentSource,
            ComputeBuffer positionOut, ComputeBuffer normalOut, ComputeBuffer tangentOut,
            int trianglePerThread
        )
        {
            var kernel = compute.FindKernel("Update");

            compute.SetBuffer(kernel, "PositionSource", positionSource);
            compute.SetBuffer(kernel, "NormalSource", normalSource);
            compute.SetBuffer(kernel, "TangentSource", tangentSource);

            compute.SetBuffer(kernel, "PositionOut", positionOut);
            compute.SetBuffer(kernel, "NormalOut", normalOut);
            compute.SetBuffer(kernel, "TangentOut", tangentOut);

            compute.SetInt("TriangleCount", _data.triangleCount);

            if (Application.isPlaying)
                compute.SetFloat("Time", Time.time);
            else
                compute.SetFloat("Time", 10);

            uint cx, cy, cz;
            compute.GetKernelThreadGroupSizes(kernel, out cx, out cy, out cz);

            var tcount = (int)cx * trianglePerThread;
            var group = (_data.triangleCount + tcount - 1) / tcount;

            compute.Dispatch(kernel, group, 1, 1);
        }

        void UpdateVertices()
        {
            ApplyCompute(
                _computeNoise,
                _positionSource, _normalSource, _tangentSource,
                _positionBuffer1, _normalBuffer1, _tangentBuffer1,
                1
            );

            ApplyCompute(
                _computeDigitize,
                _positionBuffer1, _normalBuffer1, _tangentBuffer1,
                _positionBuffer2, _normalBuffer2, _tangentBuffer2,
                2
            );
        }

        #endregion

        #region External component handling

        void UpdateMeshFilter()
        {
            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.hideFlags = HideFlags.NotEditable;
            }

            if (meshFilter.sharedMesh != _data.templateMesh)
                meshFilter.sharedMesh = _data.templateMesh;
        }

        void UpdateMeshRenderer()
        {
            var meshRenderer = GetComponent<MeshRenderer>();

            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            _propertyBlock.SetBuffer("_PositionBuffer", _positionBuffer2);
            _propertyBlock.SetBuffer("_NormalBuffer", _normalBuffer2);
            _propertyBlock.SetBuffer("_TangentBuffer", _tangentBuffer2);
            _propertyBlock.SetFloat("_TriangleCount", _data.triangleCount);

            meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        #endregion

        #region MonoBehaviour methods

        void OnDisable()
        {
            // In edit mode, we release the compute buffers OnDisable not
            // OnDestroy, because Unity spits out warnings before OnDestroy.
            // (OnDestroy is too late to prevent warning.)
            if (!Application.isPlaying) ReleaseVertices();
        }

        void OnDestroy()
        {
            ReleaseVertices();
        }

        void LateUpdate()
        {
            if (_data != null)
            {
                SetupVertices();
                UpdateVertices();
                UpdateMeshFilter();
                UpdateMeshRenderer();
            }
        }

        #endregion
    }
}
