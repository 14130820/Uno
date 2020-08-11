using UnityEngine;
using UnityEngine.UI;

namespace Uno.GUI
{
    public class UnoButtonGUI : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => OnUnoButton?.Invoke());
        }

        public delegate void UnoButton();
        public event UnoButton OnUnoButton;
    }
}
