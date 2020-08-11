using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun
{
    public class SelectionGUI : MonoBehaviour
    {
        [SerializeField]
        private Button createRoomButton = null;
        [SerializeField]
        private Button randomRoomButton = null;
        [SerializeField]
        private Button roomListButton = null;
        [SerializeField]
        private Button logoutButton = null;

        private void Awake()
        {
            createRoomButton.onClick.AddListener(() => OnCreateRoom?.Invoke());
            randomRoomButton.onClick.AddListener(() => OnRandomRoom?.Invoke());
            roomListButton.onClick.AddListener(() => OnRoomList?.Invoke());
            logoutButton.onClick.AddListener(() => OnLogout());

            OnCreateRoom += SetInactive;
            OnRandomRoom += SetInactive;
            OnRoomList += SetInactive;
        }

        // Events

        public delegate void SwitchRoom();

        public event SwitchRoom OnCreateRoom;
        public event SwitchRoom OnRandomRoom;
        public event SwitchRoom OnRoomList;

        private void OnLogout()
        {
            PhotonNetwork.Disconnect();
            SetInactive();
        }

        // Logic

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetInactive() => this.gameObject.SetActive(false);
    }
}
