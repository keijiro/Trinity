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

        [SerializeField, HideInInspector] ComputeShader _compute;

        #endregion

        #region Private objects

        ComputeBuffer _positionSource;
        ComputeBuffer _positionBuffer;

        ComputeBuffer _normalSource;
        ComputeBuffer _normalBuffer;

        ComputeBuffer _tangentSource;
        ComputeBuffer _tangentBuffer;

        MaterialPropertyBlock _propertyBlock;

        #endregion

        #region Internal resource handling

        void SetupVertices()
        {
            if (_positionSource == null) _positionSource = _data.CreatePositionBuffer();
            if (_positionBuffer == null) _positionBuffer = _data.CreatePositionBuffer();
            if (_normalSource == null) _normalSource = _data.CreateNormalBuffer();
            if (_normalBuffer == null) _normalBuffer = _data.CreateNormalBuffer();
            if (_tangentSource == null) _tangentSource  = _data.CreateTangentBuffer();
            if (_tangentBuffer == null) _tangentBuffer  = _data.CreateTangentBuffer();
        }

        void ReleaseVertices()
        {
            if (_positionSource != null) _positionSource.Release();
            if (_positionBuffer != null) _positionBuffer.Release();
            if (_normalSource != null) _normalSource.Release();
            if (_normalBuffer != null) _normalBuffer.Release();
            if (_tangentSource != null) _tangentSource.Release();
            if (_tangentBuffer != null) _tangentBuffer.Release();
            _positionSource = _normalSource = _tangentSource = null;
            _positionBuffer = _normalBuffer = _tangentBuffer = null;
        }

        void UpdateVertices()
        {
            var kernel = _compute.FindKernel("Update");

            _compute.SetBuffer(kernel, "PositionSource", _positionSource);
            _compute.SetBuffer(kernel, "PositionOut", _positionBuffer);
            _compute.SetBuffer(kernel, "NormalSource", _normalSource);
            _compute.SetBuffer(kernel, "NormalOut", _normalBuffer);
            _compute.SetBuffer(kernel, "TangentSource", _tangentSource);
            _compute.SetBuffer(kernel, "TangentOut", _tangentBuffer);

            _compute.SetInt("TriangleCount", _data.triangleCount);

            if (Application.isPlaying)
                _compute.SetFloat("Time", Time.time);
            else
                _compute.SetFloat("Time", 10);

            uint cx, cy, cz;
            _compute.GetKernelThreadGroupSizes(kernel, out cx, out cy, out cz);

            var group = (_data.triangleCount + (int)cx * 2 - 1) / ((int)cx * 2);
            _compute.Dispatch(kernel, group, 1, 1);
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

            _propertyBlock.SetBuffer("_PositionBuffer", _positionBuffer);
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
