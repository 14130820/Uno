using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Photon.Pun
{
    public class RoomListEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI roomNameText = null;
        [SerializeField]
        private TextMeshProUGUI roomPlayersText = null;
        [SerializeField]
        private Button joinRoomButton = null;

        private string roomName;

        // Unity

        public void Awake()
        {
            joinRoomButton.onClick.AddListener(() => OnJoinRoomButtonClick());
        }
        
        // UI Callback

        private void OnJoinRoomButtonClick()
        {
            PhotonNetwork.LeaveLobby();

            PhotonNetwork.JoinRoom(roomName);
        }

        // Logic

        public void Initialize(string name, byte currentPlayers, byte maxPlayers)
        {
            roomName = name;

            roomNameText.text = name;
            roomPlayersText.text = currentPlayers + " / " + maxPlayers;
        }
    }
}