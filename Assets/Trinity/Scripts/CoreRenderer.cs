using UnityEngine;

namespace Trinity
{
    [ExecuteInEditMode]
    public class CoreRenderer : MonoBehaviour
    {
        #region Exposed properties

        [SerializeField] int _triangleCount = 128;

        public int triangleCount {
            get { return _triangleCount; }
            set { _triangleCount = value; }
        }

        [SerializeField] float _triangleExtent = 0.1f;

        public float triangleExtent {
            get { return _triangleExtent; }
            set { _triangleExtent = value; }
        }

        [SerializeField] float _shuffleSpeed = 4;

        public float shuffleSpeed {
            get { return _shuffleSpeed; }
            set { _shuffleSpeed = value; }
        }

        [SerializeField] float _noiseAmplitude = 1;

        public float noiseAmplitude {
            get { return _noiseAmplitude; }
            set { _noiseAmplitude = value; }
        }

        [SerializeField] float _noiseFrequency = 1;

        public float noiseFrequency {
            get { return _noiseFrequency; }
            set { _noiseFrequency = value; }
        }

        [SerializeField] Vector3 _noiseMotion = Vector3.up;

        public Vector3 noiseMotion {
            get { return _noiseMotion; }
            set { _noiseMotion = value; }
        }

        #endregion

        #region Built-in assets

        [SerializeField, HideInInspector] ComputeShader _compute;
        [SerializeField, HideInInspector] Shader _shader;

        #endregion

        #region Private variables

        ComputeBuffer _drawArgsBuffer;
        ComputeBuffer _positionBuffer;
        ComputeBuffer _normalBuffer;

        Mesh _mesh;
        Material _material;

        float _time;
        Vector3 _noiseOffset;

        #endregion

        #region Compute configurations

        const int kThreadCount = 128;
        int ThreadGroupCount { get { return _triangleCount / kThreadCount; } }
        int TriangleCount { get { return kThreadCount * ThreadGroupCount; } }

        #endregion

        #region MonoBehaviour methods

        void OnValidate()
        {
            _triangleCount = Mathf.Max(kThreadCount, _triangleCount);
            _triangleExtent = Mathf.Max(0, _triangleExtent);
            _noiseFrequency = Mathf.Max(0, _noiseFrequency);
        }

        void Start()
        {
            // Mesh with single triangle.
            _mesh = new Mesh();
            _mesh.hideFlags = HideFlags.DontSave;
            _mesh.vertices = new Vector3 [3];
            _mesh.SetIndices(new [] {0, 1, 2}, MeshTopology.Triangles, 0);
            _mesh.UploadMeshData(true);

            // Material for drawing.
            _material = new Material(_shader);
            _material.hideFlags = HideFlags.DontSave;
        }

        void OnDestroy()
        {
            if (_mesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_mesh);
                    Destroy(_material);
                }
                else
                {
                    DestroyImmediate(_mesh);
                    DestroyImmediate(_material);
                }
            }

            if (_drawArgsBuffer != null)
            {
                _drawArgsBuffer.Release();
                _positionBuffer.Release();
                _normalBuffer.Release();
            }
        }

        void OnDisable()
        {
            // In edit mode, we release the compute buffers OnDisable not only
            // OnDestroy, because Unity spits out warning messages before
            // OnDestroy -- OnDestroy is too late to prevent warning.
            if (_drawArgsBuffer != null)
            {
                _drawArgsBuffer.Release();
                _positionBuffer.Release();
                _normalBuffer.Release();
            }
        }

        void Update()
        {
            // Allocate/Reallocate the compute buffers when it hasn't been
            // initialized or the triangle count was changed from the last frame.
            if (_positionBuffer == null || _positionBuffer.count != TriangleCount * 3)
            {
                if (_positionBuffer != null) _positionBuffer.Release();
                if (_normalBuffer != null) _normalBuffer.Release();

                _positionBuffer = new ComputeBuffer(TriangleCount * 3, 16);
                _normalBuffer = new ComputeBuffer(TriangleCount * 3, 16);

                if (_drawArgsBuffer == null)
                    _drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

                _drawArgsBuffer.SetData(new uint[5] {3, (uint)TriangleCount, 0, 0, 0});
            }

            // Invoke the update compute kernel.
            var kernel = _compute.FindKernel("Update");

            _compute.SetFloat("Time", _time);
            _compute.SetFloat("Extent", _triangleExtent);
            _compute.SetFloat("NoiseAmplitude", _noiseAmplitude);
            _compute.SetFloat("NoiseFrequency", _noiseFrequency);
            _compute.SetVector("NoiseOffset", _noiseOffset);

            _compute.SetBuffer(kernel, "PositionBuffer", _positionBuffer);
            _compute.SetBuffer(kernel, "NormalBuffer", _normalBuffer);

            _compute.Dispatch(kernel, ThreadGroupCount, 1, 1);

            // Update the internal state.
            if (Application.isPlaying)
            {
                _time += _shuffleSpeed * Time.deltaTime;
                _noiseOffset += _noiseMotion * Time.deltaTime;
            }
        }

        void LateUpdate()
        {
            // Draw the mesh with instancing.
            var bounds = new Bounds(transform.position, transform.lossyScale * 5);

            _material.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
            _material.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);

            _material.SetBuffer("_PositionBuffer", _positionBuffer);
            _material.SetBuffer("_NormalBuffer", _normalBuffer);

            Graphics.DrawMeshInstancedIndirect(_mesh, 0, _material, bounds, _drawArgsBuffer);
        }

        #endregion
    }
}
