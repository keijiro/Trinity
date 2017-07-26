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
        [SerializeField, Range(0, 1)] float _voxelize;
        [SerializeField, Range(0, 1)] float _jitter;
        [SerializeField, Range(0, 1)] float _digitize;

        #endregion

        #region Hidden attributes

        [SerializeField, HideInInspector] ComputeShader _computeVoxelize;
        [SerializeField, HideInInspector] ComputeShader _computeJitter;
        [SerializeField, HideInInspector] ComputeShader _computeDigitize;
        [SerializeField, HideInInspector] ComputeShader _computeReconstruct;

        #endregion

        #region Private objects

        ComputeBuffer _positionSource;
        ComputeBuffer _positionBuffer1;
        ComputeBuffer _positionBuffer2;

        ComputeBuffer _normalSource;
        ComputeBuffer _normalBuffer;

        ComputeBuffer _tangentSource;
        ComputeBuffer _tangentBuffer;

        MaterialPropertyBlock _propertyBlock;

        #endregion

        #region Internal resource handling

        float globalTime {
            get { return Application.isPlaying ? Time.time : 10; }
        }

        int GetThreadGroupSizeX(ComputeShader compute, int kernel)
        {
            uint cx, cy, cz;
            compute.GetKernelThreadGroupSizes(kernel, out cx, out cy, out cz);
            return (int)cx;
        }

        void SetupVertices()
        {
            if (_positionSource  == null) _positionSource  = _data.CreatePositionBuffer();
            if (_positionBuffer1 == null) _positionBuffer1 = _data.CreatePositionBuffer();
            if (_positionBuffer2 == null) _positionBuffer2 = _data.CreatePositionBuffer();

            if (_normalSource == null) _normalSource = _data.CreateNormalBuffer();
            if (_normalBuffer == null) _normalBuffer = _data.CreateNormalBuffer();

            if (_tangentSource == null) _tangentSource = _data.CreateTangentBuffer();
            if (_tangentBuffer == null) _tangentBuffer = _data.CreateTangentBuffer();
        }

        void ReleaseVertices()
        {
            if (_positionSource  != null) _positionSource .Release();
            if (_positionBuffer1 != null) _positionBuffer1.Release();
            if (_positionBuffer2 != null) _positionBuffer2.Release();
            _positionSource = _positionBuffer1 = _positionBuffer2 = null;

            if (_normalSource != null) _normalSource.Release();
            if (_normalBuffer != null) _normalBuffer.Release();
            _normalSource = _normalBuffer = null;

            if (_tangentSource != null) _tangentSource.Release();
            if (_tangentBuffer != null) _tangentBuffer.Release();
            _tangentSource = _tangentBuffer = null;
        }

        void ApplyCompute(
            ComputeShader compute, int trianglePerThread, float amplitude,
            ComputeBuffer positionSource, ComputeBuffer positionOut)
        {
            var kernel = compute.FindKernel("Main");

            compute.SetBuffer(kernel, "PositionSource", positionSource);
            compute.SetBuffer(kernel, "PositionOut", positionOut);

            compute.SetInt("TriangleCount", _data.triangleCount);
            compute.SetFloat("Amplitude", amplitude);
            compute.SetFloat("Time", globalTime);

            var c = GetThreadGroupSizeX(compute, kernel) * trianglePerThread;
            compute.Dispatch(kernel, (_data.triangleCount + c - 1) / c, 1, 1);
        }

        void UpdateVertices()
        {
            ApplyCompute(_computeVoxelize, 1, _voxelize, _positionSource, _positionBuffer1);
            ApplyCompute(_computeJitter, 1, _jitter, _positionBuffer1, _positionBuffer2);
            ApplyCompute(_computeDigitize, 2, _digitize, _positionBuffer2, _positionBuffer1);

            var compute = _computeReconstruct;
            var kernel = compute.FindKernel("Main");

            compute.SetBuffer(kernel, "PositionSource", _positionSource);
            compute.SetBuffer(kernel, "NormalSource", _normalSource);
            compute.SetBuffer(kernel, "TangentSource", _tangentSource);

            compute.SetBuffer(kernel, "PositionModified", _positionBuffer2);
            compute.SetBuffer(kernel, "NormalOut", _normalBuffer);
            compute.SetBuffer(kernel, "TangentOut", _tangentBuffer);

            compute.SetInt("TriangleCount", _data.triangleCount);

            var c = GetThreadGroupSizeX(compute, kernel);
            compute.Dispatch(kernel, (_data.triangleCount + c - 1) / c, 1, 1);
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

            _propertyBlock.SetBuffer("_PositionBuffer", _positionBuffer1);
            _propertyBlock.SetBuffer("_NormalBuffer", _normalBuffer);
            _propertyBlock.SetBuffer("_TangentBuffer", _tangentBuffer);
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
