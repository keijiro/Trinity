using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/Random Color")]
    public class RandomColor : NodeBase
    {
        #region Node I/O

        [Inlet]
        public void Bang()
        {
            if (!enabled) return;
            _outputEvent.Invoke(randomColor);
        }

        [SerializeField, Outlet]
        ColorEvent _outputEvent = new ColorEvent();

        #endregion

        #region Private members

        Color randomColor {
            get { return new Color(Random.value, Random.value, Random.value); }
        }

        #endregion
    }
}
