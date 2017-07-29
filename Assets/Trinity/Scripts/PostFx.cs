using UnityEngine;

namespace Trinity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class PostFx : MonoBehaviour
    {
        #region Exposed attributes

        [Space]
        [SerializeField, Range(0, 1)] float _scanlineNoise;
        [SerializeField, Range(0, 1)] float _blockDisplace;
        [Space]
        [SerializeField, Range(0, 1)] float _overlayShuffle;
        [SerializeField, Range(0, 1)] float _overlaySlits;
        [Space]
        [SerializeField] float _wiperSpeed = 1;
        [Space]
        [SerializeField] Color _lineColor = Color.black;
        [SerializeField, ColorUsage(false)] Color _fillColor1 = Color.blue;
        [SerializeField, ColorUsage(false)] Color _fillColor2 = Color.red;
        [SerializeField, ColorUsage(false)] Color _fillColor3 = Color.white;
        [Space]
        [SerializeField, Range(0, 0.2f)] float _colorThreshold = 0.1f;
        [SerializeField, Range(0, 0.2f)] float _depthThreshold = 0.1f;
        [Space]
        [SerializeField] Texture _overlayTexture;
        [SerializeField] Color _overlayColor = Color.red;

        public float scanlineNoise { set { _scanlineNoise = value; } }
        public float blockDisplace { set { _blockDisplace = value; } }

        public float overlayShuffle { set { _overlayShuffle = value; } }
        public float overlaySlits { set { _overlaySlits = value; } }

        public Color lineColor { set { _lineColor = value; } }
        public Color fillColor1 { set { _fillColor1 = value; } }
        public Color fillColor2 { set { _fillColor2 = value; } }
        public Color fillColor3 { set { _fillColor3 = value; } }

        public Texture overlayTexture { set { _overlayTexture = value; } }
        public Color overlayColor { set { _overlayColor = value; } }

        #endregion

        #region Private variables

        [SerializeField, HideInInspector] Shader _shader;
        Material _material;

        float[] _wipers;
        int _wipeCount;

        #endregion

        #region MonoBehaviour methods

        void Start()
        {
            _wipers = new float[3];
        }

        void OnDestroy()
        {
            if (Application.isPlaying)
                Destroy(_material);
            else
                DestroyImmediate(_material);
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                // Wiper animation
                var dt = Time.deltaTime * _wiperSpeed;
                for (var i = 0; i < 3; i++)
                {
                    var moveTo = (_wipeCount + i) / 3;
                    _wipers[i] = Mathf.Clamp(_wipers[i] + dt, moveTo - 1, moveTo);
                }
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            _material.SetColor("_LineColor", _lineColor);
            _material.SetColor("_FillColor1", _fillColor1);
            _material.SetColor("_FillColor2", _fillColor2);
            _material.SetColor("_FillColor3", _fillColor3);

            _material.SetFloat("_ColorThreshold", _colorThreshold);
            _material.SetFloat("_DepthThreshold", _depthThreshold);

            _material.SetTexture("_OverlayTex", _overlayTexture);
            _material.SetColor("_OverlayColor", _overlayColor);

            var time = Application.isPlaying ? Time.time : 10.1f;
            _material.SetFloat("_Progress", time);
            _material.SetFloat("_ScanlineNoise", _scanlineNoise);
            _material.SetFloat("_BlockDisplace", _blockDisplace);
            _material.SetFloat("_OverlayShuffle", _overlayShuffle);
            _material.SetFloat("_OverlaySlits", _overlaySlits);
            _material.SetFloat("_OverlayWiper1", _wipers[0]);
            _material.SetFloat("_OverlayWiper2", _wipers[1]);
            _material.SetFloat("_OverlayWiper3", _wipers[2]);

            Graphics.Blit(source, destination, _material, 0);
        }

        #endregion
    }
}
