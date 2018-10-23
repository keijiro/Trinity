using UnityEngine;
using UnityEngine.Rendering;

namespace Trinity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public sealed class ScreenBlit : MonoBehaviour
    {
        [SerializeField] Texture _sourceTexture;
        [SerializeField] float _marginTop;
        [SerializeField] float _marginRight;
        [SerializeField] float _marginBottom;
        [SerializeField] float _marginLeft;

        [SerializeField, HideInInspector] Shader _blitShader;

        Camera _camera;

        Material _blitMaterial;
        CommandBuffer _commandBuffer;

        void OnDisable()
        {
            if (_camera != null)
            {
                _camera.RemoveCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
                _camera = null;
            }
        }

        void OnDestroy()
        {
            if (_blitMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(_blitMaterial);
                else
                    DestroyImmediate(_blitMaterial);
                _blitMaterial = null;
            }

            if (_commandBuffer != null)
            {
                _commandBuffer.Release();
                _commandBuffer = null;
            }
        }

        void Update()
        {
            if (_blitMaterial == null)
            {
                _blitMaterial = new Material(_blitShader);
                _blitMaterial.hideFlags = HideFlags.DontSave;
            }

            _blitMaterial.SetTexture("_MainTex", _sourceTexture);
            _blitMaterial.SetVector("_Margins", new Vector4(
                _marginLeft, _marginBottom, _marginRight, _marginTop
            ));

            if (_commandBuffer == null)
            {
                _commandBuffer = new CommandBuffer();
                _commandBuffer.DrawProcedural(
                    Matrix4x4.identity, _blitMaterial, 0,
                    MeshTopology.Triangles, 3
                );
            }

            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
                _camera.AddCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
            }
        }
    }
}
