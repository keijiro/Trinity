using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Kvant/Spray")]
    public class SprayOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Kvant.SprayMV[] _targets;

        #endregion

        #region Node I/O

        [Inlet]
        public float input {
            set {
                if (!enabled) return;

                const float threshold = 0.001f;
                var isZero = (value < threshold);

                if (isZero)
                {
                    // Input is nearly zero: just clear throttles.
                    foreach (var t in _targets) t.throttle = 0;
                }
                else
                {
                    // Update throttles.
                    foreach (var t in _targets) t.throttle = value;

                    // Is this a zero-to-on change?
                    if (_zeroInput)
                    {
                        // Re-enable the targets and their renderers.
                        foreach (var t in _targets)
                        {
                            t.enabled = true;
                            t.GetComponent<MeshRenderer>().enabled = true;
                        }
                    }
                }

                _zeroInput = isZero;
            }
        }
        
        #endregion

        #region Private members

        bool _zeroInput;
        float _delayToOff;
        float _delayTimer;

        #endregion

        #region MonoBehaviour methods

        void Start()
        {
            // Use the maximum length of life as a delay-to-off value.
            foreach (var t in _targets)
                _delayToOff = Mathf.Max(_delayToOff, t.life);

            // Reset as off.
            _zeroInput = true;
            _delayTimer = _delayToOff;

            foreach (var t in _targets)
            {
                t.throttle = 0;
                t.enabled = false;
                t.GetComponent<Renderer>().enabled = false;
            }
        }

        void Update()
        {
            if (!enabled) return;

            if (_zeroInput)
            {
                // Disable the targets with delay timer.
                if (_delayTimer < _delayToOff)
                {
                    _delayTimer += Time.deltaTime;

                    if (_delayTimer >= _delayToOff)
                    {
                        foreach (var t in _targets)
                        {
                            t.enabled = false;
                            t.GetComponent<Renderer>().enabled = false;
                        }
                    }
                }
            }
            else
            {
                _delayTimer = 0;
            }
        }

        #endregion
    }
}
