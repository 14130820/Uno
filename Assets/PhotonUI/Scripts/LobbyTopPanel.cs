using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Photon.Pun
{
    public class LobbyTopPanel : MonoBehaviour
    {
        private readonly string connectionStatusMessage = "    Connection Status: ";

        [Header("UI References")]
        public TextMeshProUGUI ConnectionStatusText;

        #region UNITY

        private ClientState currentClientState = ClientState.ConnectedToGameserver;

        public void Update()
        {
            var state = PhotonNetwork.NetworkClientState;
            if (currentClientState != state)
            {
                ConnectionStatusText.text = connectionStatusMessage + state;
                currentClientState = state;
            }
        }

        #endregion
    }
}