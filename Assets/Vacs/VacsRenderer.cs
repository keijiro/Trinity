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

        #region Private objects

        ComputeBuffer _positionBuffer;
        ComputeBuffer _normalBuffer;
        ComputeBuffer _tangentBuffer;

        // Custom properties applied to the mesh renderer.
        MaterialPropertyBlock _propertyBlock;

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

            meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        #endregion

        #region MonoBehaviour methods

        void OnDisable()
        {
            // In edit mode, we release the compute buffers OnDisable not
            // OnDestroy, because Unity spits out warnings before OnDestroy.
            // (OnDestroy is too late to prevent warning.)
            if (!Application.isPlaying)
            {
                if (_positionBuffer != null) _positionBuffer.Release();
                if (_normalBuffer   != null) _normalBuffer.  Release();
                if (_tangentBuffer  != null) _tangentBuffer. Release();
                _positionBuffer = _normalBuffer = _tangentBuffer = null;
            }
        }

        void OnDestroy()
        {
            if (_positionBuffer != null) _positionBuffer.Release();
            if (_normalBuffer   != null) _normalBuffer.  Release();
            if (_tangentBuffer  != null) _tangentBuffer. Release();
            _positionBuffer = _normalBuffer = _tangentBuffer = null;
        }

        void LateUpdate()
        {
            if (_data == null) return;

            if (_positionBuffer == null) _positionBuffer = _data.CreatePositionBuffer();
            if (_normalBuffer   == null) _normalBuffer   = _data.CreateNormalBuffer  ();
            if (_tangentBuffer  == null) _tangentBuffer  = _data.CreateTangentBuffer ();

            UpdateMeshFilter();
            UpdateMeshRenderer();
        }

        #endregion
    }
}
