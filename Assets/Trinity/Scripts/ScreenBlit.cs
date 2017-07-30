using UnityEngine;
using UnityEngine.Rendering;

namespace Trinity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public sealed class ScreenBlit : MonoBehaviour
    {
        [SerializeField] Texture _sourceTexture;
        [SerializeField, Range(0, 3)] int _screenIndex;
        [SerializeField] bool _flip;

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
                _blitMaterial.SetTexture("_MainTex", _sourceTexture);
                _blitMaterial.SetFloat("_Displace", (_screenIndex - 1) / 3.0f);
                _blitMaterial.SetFloat("_VFlip", _flip ? 1 : 0);
            }

            if (_commandBuffer == null)
            {
                var isMonitor = (_screenIndex == 0);
                _commandBuffer = new CommandBuffer();
                _commandBuffer.DrawProcedural(
                    Matrix4x4.identity, _blitMaterial, isMonitor ? 1 : 0,
                    MeshTopology.Triangles, isMonitor ? 6 : 3
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
