using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Photon.Pun
{
    public class LoginGUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField nameInputField = null;
        [SerializeField]
        private Button logInButton = null;
        [SerializeField]
        private Button offlineModeButton = null;

        // Unity

        private void Awake()
        {
            nameInputField.text = "Player " + Random.Range(1000, 10000);

            logInButton.onClick.AddListener(() => OnLoginButtonClicked());
            offlineModeButton.onClick.AddListener(() => OnEnterOfflineModeButtonClicked());
        }
        
        // UI Callback

        /// <summary>
        /// Attempts to log in to server.
        /// </summary>
        private void OnLoginButtonClicked()
        {
            string playerName = nameInputField.text;

            if (!string.IsNullOrEmpty(playerName))
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
                this.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Please enter a name.");
            }
        }

        /// <summary>
        /// Enters Offline Mode.
        /// </summary>
        private void OnEnterOfflineModeButtonClicked()
        {
            PhotonNetwork.LocalPlayer.NickName = "Player";
            PhotonNetwork.OfflineMode = true;

            MyPhoton.CreateRoom(string.Empty, true, 7, 6);
            this.gameObject.SetActive(false);
        }
    }
}