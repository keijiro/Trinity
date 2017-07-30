using UnityEngine;

namespace Trinity
{
    public class Configurator : MonoBehaviour
    {
        void Start()
        {
            if (!Application.isEditor)
            {
                TryActivateDisplay(0);
                TryActivateDisplay(1);
                TryActivateDisplay(2);
                TryActivateDisplay(3);
            }
        }

        void TryActivateDisplay(int index)
        {
            if (index < Display.displays.Length)
                Display.displays[index].Activate();
        }
    }
}
