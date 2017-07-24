using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Vacs
{
    public sealed class VacsData : ScriptableObject
    {
        #region Public properties and methods

        /// Number of vertices.
        public int vertexCount {
            get { return _positionData.Length; }
        }

        /// Reference to the template mesh object.
        public Mesh templateMesh {
            get { return _mesh; }
        }

        /// Create a compute buffer for the position data.
        /// The returned buffer must be released by the caller.
        public ComputeBuffer CreatePositionBuffer()
        {
            var buffer = new ComputeBuffer(vertexCount, sizeof(float) * 4);
            buffer.SetData(_positionData);
            return buffer;
        }

        /// Create a compute buffer for the normal data.
        /// The returned buffer must be released by the caller.
        public ComputeBuffer CreateNormalBuffer()
        {
            var buffer = new ComputeBuffer(vertexCount, sizeof(float) * 4);
            buffer.SetData(_normalData);
            return buffer;
        }

        /// Create a compute buffer for the tangent data.
        /// The returned buffer must be released by the caller.
        public ComputeBuffer CreateTangentBuffer()
        {
            var buffer = new ComputeBuffer(vertexCount, sizeof(float) * 4);
            buffer.SetData(_tangentData);
            return buffer;
        }

        #endregion

        #region Serialized data fields

        [SerializeField] Mesh _mesh;
        [SerializeField] Vector4[] _positionData;
        [SerializeField] Vector4[] _normalData;
        [SerializeField] Vector4[] _tangentData;

        #endregion

        #region Editor functions

        #if UNITY_EDITOR

        public void CreateFromMesh(Mesh source)
        {
            // Input
            var inVertices = source.vertices;
            var inNormals = source.normals;
            var inTangents = source.tangents;
            var inUV = source.uv;
            var inIndices = source.GetIndices(0);

            // Output
            var outVertices = new List<Vector4>();
            var outNormals = new List<Vector4>();
            var outTangents = new List<Vector4>();
            var outUV = new List<Vector2>();

            // Enumerate all the triangles and belonging vertices.
            for (var i = 0; i < inIndices.Length; i += 3)
            {
                // Simply copy the original vertex attributes.
                // (position, normal, tangent, UV1)
                var i1 = inIndices[i + 0];
                var i2 = inIndices[i + 1];
                var i3 = inIndices[i + 2];

                outVertices.Add(inVertices[i1]);
                outVertices.Add(inVertices[i2]);
                outVertices.Add(inVertices[i3]);

                outNormals.Add(inNormals[i1]);
                outNormals.Add(inNormals[i2]);
                outNormals.Add(inNormals[i3]);

                outTangents.Add(inTangents[i1]);
                outTangents.Add(inTangents[i2]);
                outTangents.Add(inTangents[i3]);

                outUV.Add(inUV[i1]);
                outUV.Add(inUV[i2]);
                outUV.Add(inUV[i3]);
            }

            // Enumerate vertex indices.
            var indices = Enumerable.Range(0, outVertices.Count).ToArray();

            // Build a new mesh.
            _mesh = new Mesh();
            _mesh.name = source.name;
            _mesh.vertices = new Vector3[outVertices.Count];
            _mesh.SetUVs(0, outUV);
            _mesh.subMeshCount = 1;
            _mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            _mesh.bounds = source.bounds;
            _mesh.UploadMeshData(true);

            // Convert the data into Vector4 arrays.
            _positionData = outVertices.ToArray();
            _normalData   = outNormals .ToArray();
            _tangentData  = outTangents.ToArray();
        }

        #endif

        #endregion
    }
}
