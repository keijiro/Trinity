using UnityEngine;

namespace Trinity
{
    class NestedStructure : MonoBehaviour
    {
        #region Editable properties

        [SerializeField] GameObject _template;
        [SerializeField] int _instanceCount = 4;
        [SerializeField] float _scaleSpeed = 10;
        [SerializeField] float _cutOffSpeed = 1;

        #endregion

        #region Public properties and methods (used for wiring)

        public float deformAmplitude { get; set; }

        #endregion

        #region Private members

        GameObject[] _instances;
        Vector3 _originalScale;

        #endregion

        #region MonoBehaviour functions

        void Start()
        {
            _instances = new GameObject[_instanceCount];

            for (var i = 0; i < _instanceCount; i++)
            {
                if (i == 0)
                    _instances[i] = _template;
                else
                    _instances[i] = Instantiate(_template, transform);
            }

            _originalScale = _template.transform.localScale;
        }

        void Update()
        {
            var globalTime = Time.time;

            for (var i = 0; i < _instanceCount; i++)
            {
                var time = (_cutOffSpeed * globalTime + (float)i / _instanceCount) % 1.0f;
                var scale = time * _scaleSpeed;

                _instances[i].GetComponentInChildren<Vacs.VacsRenderer>().dissolve = time;
                _instances[i].transform.localScale = _originalScale + Vector3.one * scale;
            }
        }

        #endregion
    }
}
