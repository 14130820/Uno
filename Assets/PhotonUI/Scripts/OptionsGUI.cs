using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun
{
    public class OptionsGUI : MonoBehaviour
    {
        [SerializeField]
        private Button openOptionsButton = null;
        [SerializeField]
        private GameObject optionsPanel = null;

        [SerializeField]
        private Slider volumeSlider = null;
        [SerializeField]
        private Button backButton = null;
        [SerializeField]
        private Button exitToDesktopButton = null;
        [SerializeField]
        private Button quitGameButton = null;

        private void Awake()
        {
            openOptionsButton.onClick.AddListener(() => OpenWindow());
            volumeSlider.onValueChanged.AddListener((a) => AudioListener.volume = a);
            exitToDesktopButton.onClick.AddListener(() => Application.Quit());
            quitGameButton.onClick.AddListener(() => PhotonNetwork.LeaveRoom());
            backButton.onClick.AddListener(() => CloseWindow());
        }
        
        public bool IsOpen => optionsPanel.activeSelf;
        
        // UI Callback
        
        private void OpenWindow()
        {
            openOptionsButton.gameObject.SetActive(false);
            optionsPanel.SetActive(true);
            quitGameButton.gameObject.SetActive(Uno.GameManager.Instance.CurrentGameState == Uno.GameManager.GameState.InGame ? true : false);
        }


        public void CloseWindow()
        {
            openOptionsButton.gameObject.SetActive(true);
            optionsPanel.SetActive(false);
        }
    }
}
