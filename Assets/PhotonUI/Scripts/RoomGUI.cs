using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using FeatherWorks.Pooling;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;

namespace Photon.Pun
{
    public class RoomGUI : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private Transform playerListContent = null;
        [SerializeField]
        private Slider maxPlayersSlider = null;
        [SerializeField]
        private TextMeshProUGUI maxPlayerDisplayText = null;
        [SerializeField]
        private Slider addBotsSlider = null;
        [SerializeField]
        private TextMeshProUGUI addBotsCountText = null;
        [SerializeField]
        private TextMeshProUGUI playerCountText = null;
        [SerializeField]
        private GameObject optionsFader = null;
        [SerializeField]
        private Button startGameButton = null;
        [SerializeField]
        private Button exitGameButton = null;
        [SerializeField]
        [AssetsOnly]
        private GameObject playerListEntryPrefab = null;

        private FeatherPool playerListEntryGroup;

        private int _numberOfBots;
        private int _currentNumberOfPlayers;
        private int _maxNumberOfSlots;
        
        private Dictionary<int, PlayerListEntry> playerListEntries = new Dictionary<int, PlayerListEntry>();

        // Unity

        private void Awake()
        {
            playerListEntryGroup = FeatherPoolManager.Instance.GetPool(playerListEntryPrefab.GetInstanceID());

            startGameButton.onClick.AddListener(() => OnStartGameButtonClicked());
            exitGameButton.onClick.AddListener(() => OnLeaveGameButtonClicked());

            addBotsSlider.onValueChanged.AddListener((a) => OnBotsCountChange(a));
            maxPlayersSlider.onValueChanged.AddListener((a) => OnMaxPlayerChange(a));

            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable() { }

        public override void OnEnable() { }


        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                if (!PhotonNetwork.IsMasterClient) // If we are joining a room
                {
                    var list = PhotonNetwork.PlayerList;
                    CurrentNumberOfPlayers = list.Length;

                    for (int i = 0; i < CurrentNumberOfPlayers; i++)
                    {
                        var p = list[i];
                        var actorN = p.ActorNumber;

                        // Create Player List Entries for all players in room
                        if (p.CustomProperties.TryGetValue(MyPhoton.PLAYER_READY, out object ready))
                        {
                            var entry = CreateEntry(p);
                            entry.IsPlayerReady = (bool)ready;
                        }
                    }
                    
                    optionsFader.SetActive(true);
                }
                else // If we are making a room
                {
                    // create our player List Entry.
                    CurrentNumberOfPlayers = 1;
                    
                    optionsFader.SetActive(false);
                }

                CreateEntry(PhotonNetwork.LocalPlayer);

                // Set Room Props
                var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;

                Random.InitState((int)roomProps[MyPhoton.RANDOM_SEED]);
                MaxNumberOfSlots = (byte)roomProps[MyPhoton.NUMBER_OF_PLAYERS];
                NumberOfBots = (byte)roomProps[MyPhoton.NUMBER_OF_BOTS];
            }
        }

        // Pun Callbacks

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.TryGetValue(MyPhoton.PLAYER_READY, out object isPlayerReady))
            {
                if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out PlayerListEntry entry))
                {
                    entry.IsPlayerReady = (bool)isPlayerReady;
                }

                UpdateStartGameButton();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            CurrentNumberOfPlayers--;

            // Remove player list entry
            var actorN = otherPlayer.ActorNumber;
            
            playerListEntryGroup.Despawn(playerListEntries[actorN].gameObject);
            playerListEntries.Remove(actorN);

            // check if everyone is ready
            UpdateStartGameButton();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            CurrentNumberOfPlayers++;

            CreateEntry(newPlayer);
        }

        public override void OnLeftRoom()
        {
            foreach (PlayerListEntry entry in playerListEntries.Values)
            {
                playerListEntryGroup.Despawn(entry.GetComponent<FeatherPoolInstance>());
            }

            playerListEntries.Clear();
            this.gameObject.SetActive(false);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
                optionsFader.SetActive(true);
                UpdateStartGameButton();
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                if (propertiesThatChanged.TryGetValue(MyPhoton.NUMBER_OF_BOTS, out object value))
                {
                    NumberOfBots = (byte)value;
                }
                else if (propertiesThatChanged.TryGetValue(MyPhoton.NUMBER_OF_PLAYERS, out value))
                {
                    MaxNumberOfSlots = (byte)value;
                }
            }
        }

        // UI Callbacks

        private void OnLeaveGameButtonClicked()
        {
            PhotonNetwork.LeaveRoom();

            if (PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.Disconnect();
            }

            this.gameObject.SetActive(false);
        }

        private void OnStartGameButtonClicked()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            PhotonNetwork.RaiseEvent(MyPhoton.START_GAME, null, MyPhoton.EventOptions, SendOptions.SendReliable);
        }

        private void OnMaxPlayerChange(float value)
        {
            var sliderValue = (int)value;

            if (sliderValue >= NumberOfSlotsTaken) // Valid
            {
                MaxNumberOfSlots = sliderValue;

                // Update Clients
                Hashtable props = new Hashtable() { { MyPhoton.NUMBER_OF_PLAYERS, (byte)sliderValue } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);

                // check if everyone is ready
                UpdateStartGameButton();
            }
            else // Invalid
            {
                if (MaxNumberOfSlots != NumberOfSlotsTaken) // There is still a change
                {
                    MaxNumberOfSlots = NumberOfSlotsTaken;

                    // Update Clients
                    Hashtable props = new Hashtable() { { MyPhoton.NUMBER_OF_PLAYERS, (byte)sliderValue } };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(props);

                    // check if everyone is ready
                    UpdateStartGameButton();
                }
                else
                {
                    maxPlayersSlider.value = NumberOfSlotsTaken;
                }
            }
        }

        private void OnBotsCountChange(float value)
        {
            var sliderValue = (int)value;
            var maxBots = MaxNumberOfSlots - CurrentNumberOfPlayers;

            if (sliderValue <= maxBots) // Valid
            {
                NumberOfBots = sliderValue;

                // Send value to clients
                Hashtable props = new Hashtable() { { MyPhoton.NUMBER_OF_BOTS, (byte)sliderValue } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);

                // check if everyone is ready
                UpdateStartGameButton();
            }
            else // Not valid
            {
                if (NumberOfBots != maxBots)
                {
                    NumberOfBots = maxBots;

                    // Send value to clients
                    Hashtable props = new Hashtable() { { MyPhoton.NUMBER_OF_BOTS, (byte)sliderValue } };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(props);

                    // check if everyone is ready
                    UpdateStartGameButton();
                }
                else
                {
                    addBotsSlider.value = maxBots;
                }
            }
        }

        // Logic

        private int MaxNumberOfSlots
        {
            get => _maxNumberOfSlots;
            set
            {
                if (_maxNumberOfSlots != value)
                {
                    _maxNumberOfSlots = value;

                    maxPlayersSlider.value = value;
                    maxPlayerDisplayText.text = value.ToString();
                    UpdatePlayerCount();
                }
            }
        }

        private int NumberOfBots
        {
            get => _numberOfBots;
            set
            {
                if (NumberOfBots != value)
                {
                    _numberOfBots = value;

                    addBotsSlider.value = value;
                    addBotsCountText.text = value.ToString();
                    UpdatePlayerCount();
                }
            }
        }

        private int NumberOfSlotsTaken => CurrentNumberOfPlayers + NumberOfBots;

        private int CurrentNumberOfPlayers
        {
            get => _currentNumberOfPlayers;
            set
            {
                if (_currentNumberOfPlayers != value)
                {
                    _currentNumberOfPlayers = value;
                    UpdatePlayerCount();
                }
            }
        }

        private bool ArePlayersReady
        {
            get
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    return false;
                }

                // Make sure there's at least 2 players
                if (!(CurrentNumberOfPlayers + NumberOfBots >= 2))
                {
                    return false;
                }

                if (PhotonNetwork.OfflineMode)
                {
                    return true;
                }

                var list = PhotonNetwork.PlayerList;
                var length = list.Length;
                for (int i = 0; i < length; i++)
                {
                    var p = list[i];

                    if (p.CustomProperties.TryGetValue(MyPhoton.PLAYER_READY, out object isPlayerReady))
                    {
                        if (!(bool)isPlayerReady)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePlayerCount()
        {
            PhotonNetwork.CurrentRoom.MaxPlayers = (byte)(MaxNumberOfSlots - NumberOfBots);
            playerCountText.text = string.Format("Players  -  {0}/{1}", NumberOfSlotsTaken, MaxNumberOfSlots);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateStartGameButton() => startGameButton.gameObject.SetActive(ArePlayersReady);

        private PlayerListEntry CreateEntry(Player player)
        {
            var entry = playerListEntryGroup.Spawn().GetComponent<PlayerListEntry>();
            var entryTransform = entry.transform;

            entry.transform.SetParent(playerListContent);
            entry.transform.localScale = Vector3.one;

            var actorN = player.ActorNumber;

            playerListEntries.Add(actorN, entry);
            entry.Initialize(actorN, player.NickName);

            return entry;
        }
    }
}