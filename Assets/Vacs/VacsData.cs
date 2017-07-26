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

        /// Number of triangles.
        public int triangleCount {
            get { return _positionData.Length / 3; }
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
            var inNormals  = source.normals;
            var inTangents = source.tangents;
            var inUV       = source.uv;
            var inIndices  = source.GetIndices(0);

            // Vertex/Triangle count
            var vcount = inIndices.Length;
            var tcount = vcount / 3;

            // Output
            var outVertices = new Vector4 [vcount];
            var outNormals  = new Vector4 [vcount];
            var outTangents = new Vector4 [vcount];
            var outUV       = new Vector2 [vcount];

            // Enumerate all the triangles and belonging vertices.
            for (var i = 0; i < tcount; i++)
            {
                var i1 = inIndices[i * 3    ];
                var i2 = inIndices[i * 3 + 1];
                var i3 = inIndices[i * 3 + 2];

                var o1 = i;
                var o2 = i + tcount;
                var o3 = i + tcount * 2;

                outVertices[o1] = inVertices[i1];
                outVertices[o2] = inVertices[i2];
                outVertices[o3] = inVertices[i3];

                outNormals[o1] = inNormals[i1];
                outNormals[o2] = inNormals[i2];
                outNormals[o3] = inNormals[i3];

                outTangents[o1] = inTangents[i1];
                outTangents[o2] = inTangents[i2];
                outTangents[o3] = inTangents[i3];

                outUV[i * 3    ] = inUV[i1];
                outUV[i * 3 + 1] = inUV[i2];
                outUV[i * 3 + 2] = inUV[i3];
            }

            // Enumerate the vertex indices.
            var indices = Enumerable.Range(0, outVertices.Length).ToArray();

            // Build a new mesh.
            _mesh = new Mesh();
            _mesh.name = source.name;
            _mesh.vertices = new Vector3 [outVertices.Length];
            _mesh.uv = outUV;
            _mesh.subMeshCount = 1;
            _mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            _mesh.bounds = source.bounds;
            _mesh.UploadMeshData(true);

            // Output arrays.
            _positionData = outVertices;
            _normalData   = outNormals ;
            _tangentData  = outTangents;
        }

        #endif

        #endregion
    }
}
