using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using TMPro;

namespace Photon.Pun
{
    public class PlayerListEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI playerNameText = null;
        [SerializeField]
        private Button playerReadyButton = null;
        [SerializeField]
        private Image playerReadyImage = null;

        private TextMeshProUGUI playerReadyButtonText;

        private int ownerId;
        private bool isPlayerReady = false;

        public bool IsPlayerReady
        {
            get => isPlayerReady;
            set
            {
                if ((value && !isPlayerReady) || (!value && isPlayerReady))
                {
                    isPlayerReady = value;
                    playerReadyButtonText.text = value ? "Not Ready." : "Ready?";
                    playerReadyImage.enabled = value;
                }
            }
        }


        // Unity

        private void Awake() => playerReadyButtonText = playerReadyButton.GetComponentInChildren<TextMeshProUGUI>();
        
        private void OnDisable() => playerReadyButton.onClick.RemoveAllListeners();
        
        // UI Callback

        private void OnReadyButtonClick()
        {
            Hashtable props = new Hashtable() { { MyPhoton.PLAYER_READY, !IsPlayerReady } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        // Logic

        public void Initialize(int playerId, string playerName)
        {
            ownerId = playerId;
            playerNameText.text = playerName;
            isPlayerReady = false;

            if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
            {
                playerReadyButton.gameObject.SetActive(false);
            }
            else
            {
                playerReadyButton.gameObject.SetActive(true);
                
                playerReadyButton.onClick.AddListener(() => OnReadyButtonClick());

                isPlayerReady = true;
                OnReadyButtonClick();
            }
        }
    }
}