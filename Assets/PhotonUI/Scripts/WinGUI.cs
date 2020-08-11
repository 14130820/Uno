using ExitGames.Client.Photon;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Photon.Pun
{
    public class WinGUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI winnerNameText = null;
        [SerializeField]
        private Button returnToRoomButton = null;
        [SerializeField]
        private Button playAgainButton = null;

        public bool IsOpen => this.gameObject.activeSelf;

        // Unity

        private void Awake()
        {
            returnToRoomButton.onClick.AddListener(() => PhotonNetwork.RaiseEvent(MyPhoton.BACK_TO_ROOM, null, MyPhoton.EventOptions, SendOptions.SendReliable));
            playAgainButton.onClick.AddListener(() => PhotonNetwork.RaiseEvent(MyPhoton.RESTART_GAME, null, MyPhoton.EventOptions, SendOptions.SendReliable));
        }
        
        // Logic

        public void CloseWindow() => this.gameObject.SetActive(false);

        public void SetWinner(string winner)
        {
            this.gameObject.SetActive(true);

            winnerNameText.text = winner;

            if (PhotonNetwork.IsMasterClient)
            {
                playAgainButton.gameObject.SetActive(true);
                returnToRoomButton.gameObject.SetActive(true);
            }
            else
            {
                returnToRoomButton.gameObject.SetActive(false);
                playAgainButton.gameObject.SetActive(false);
            }
        }
    }
}
