using UnityEngine;

namespace Uno.Game.Players
{
    public class PlayerUpdater : MonoBehaviour
    {
        private void Awake()
        {
            PlayerController.Instance.Awake();
        }

        private void Update()
        {
            if (!GameManager.Instance.IsMenuOpen)
            PlayerController.Instance.Update();
        }
    }
}
