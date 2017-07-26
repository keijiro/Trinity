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

        void ApplyCompute(ComputeShader compute, ComputeBuffer positionSource,
            ComputeBuffer positionOut, int trianglePerThread)
        {
            var kernel = compute.FindKernel("Main");

            compute.SetBuffer(kernel, "PositionSource", positionSource);
            compute.SetBuffer(kernel, "PositionOut", positionOut);
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
            ApplyCompute(_computeNoise, _positionSource, _positionBuffer1, 1);
            ApplyCompute(_computeDigitize, _positionBuffer1, _positionBuffer2, 2);

            var kernel = _computeReconstruct.FindKernel("Main");

            _computeReconstruct.SetBuffer(kernel, "PositionSource", _positionSource);
            _computeReconstruct.SetBuffer(kernel, "PositionModified", _positionBuffer2);
            _computeReconstruct.SetBuffer(kernel, "NormalSource", _normalSource);
            _computeReconstruct.SetBuffer(kernel, "TangentSource", _tangentSource);
            _computeReconstruct.SetBuffer(kernel, "NormalOut", _normalBuffer);
            _computeReconstruct.SetBuffer(kernel, "TangentOut", _tangentBuffer);

            _computeReconstruct.SetInt("TriangleCount", _data.triangleCount);

            uint cx, cy, cz;
            _computeReconstruct.GetKernelThreadGroupSizes(kernel, out cx, out cy, out cz);

            var group = (_data.triangleCount + (int)cx - 1) / (int)cx;
            _computeReconstruct.Dispatch(kernel, group, 1, 1);
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
