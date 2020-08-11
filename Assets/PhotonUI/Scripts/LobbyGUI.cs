using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Photon.Realtime;
using FeatherWorks.Pooling;
using System.Runtime.CompilerServices;

namespace Photon.Pun
{
    public class LobbyGUI : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private GameObject contentPanel = null;
        [SerializeField]
        private Button backButton = null;
        [SerializeField]
        [AssetsOnly]
        private GameObject roomEntryPrefab = null;

        private readonly Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
        private readonly Dictionary<string, FeatherPoolInstance> roomListEntries = new Dictionary<string, FeatherPoolInstance>();
        private FeatherPool roomEntryGroup;

        private void Awake()
        {
            roomEntryGroup = FeatherPoolManager.Instance.GetPool(roomEntryPrefab);

            backButton.onClick.AddListener(() => OnBackButtonClick());
        }

        public override void OnEnable()
        {
            base.OnEnable();

            PhotonNetwork.JoinLobby();
        }

        // Events

        public delegate void ExitLobby();
        public event ExitLobby OnExitLobby;

        // PUN Callbacks

        public override void OnLeftLobby()
        {
            this.gameObject.SetActive(false);

            cachedRoomList.Clear();
            ClearRoomListView();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ClearRoomListView();

            UpdateCachedRoomList(roomList);

            // Update Room List View
            foreach (RoomInfo info in cachedRoomList.Values)
            {
                var entry = roomEntryGroup.Spawn();
                entry.transform.SetParent(contentPanel.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

                roomListEntries.Add(info.Name, entry);
            }
        }

        // UI Callbacks

        private void OnBackButtonClick()
        {
            PhotonNetwork.LeaveLobby();
            OnExitLobby?.Invoke();
        }

        // Logic

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearRoomListView()
        {
            foreach (FeatherPoolInstance entry in roomListEntries.Values)
            {
                roomEntryGroup.Despawn(entry);
            }

            roomListEntries.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
        }
    }
}
