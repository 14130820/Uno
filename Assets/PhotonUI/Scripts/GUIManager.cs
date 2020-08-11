using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
using Uno.RuleSets;
using TMPro;
using Uno.Game;
using Uno;
using System.Runtime.CompilerServices;
using Uno.GUI;

namespace Photon.Pun
{
    /// <summary>
    /// All child menus should close themselves and be opened by this script.
    /// This class is the parent for all GUI's.
    /// </summary>
    public class GUIManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        // Serialized UI GO's and Prefabs 

        [SerializeField]
        private GameObject MenusGUI = null;
        private LoginGUI loginGUI;
        private SelectionGUI selectionGUI;
        private CreateRoomGUI createRoomGUI;
        [SerializeField]
        private GameObject randomRoomGUI = null;
        private LobbyGUI lobbyGUI;
        private RoomGUI roomGUI;
        private WinGUI winGUI;
        private OptionsGUI optionsGUI;

        // vars

        //private RuleSetBase rules = new RuleSetBase(
        //new DefaultDeckType(),
        //7
        //);

        private void Awake()
        {
            // Gets all the menus
            loginGUI = GetComponentInChildren<LoginGUI>();
            selectionGUI = GetComponentInChildren<SelectionGUI>();
            createRoomGUI = GetComponentInChildren<CreateRoomGUI>();
            lobbyGUI = GetComponentInChildren<LobbyGUI>();
            roomGUI = GetComponentInChildren<RoomGUI>();
            winGUI = GetComponentInChildren<WinGUI>();
            optionsGUI = GetComponentInChildren<OptionsGUI>();
            nameTextComponents = NameTextParent.GetComponentsInChildren<TextMeshProUGUI>();

            ColorSelectorGUI = GetComponentInChildren<ColorSelectorGUI>();
            UnoButtonGUI = GetComponentInChildren<UnoButtonGUI>();

            // Subscribe to menu callbacks for changing windows.
            selectionGUI.OnCreateRoom += OpenCreateRoomGUI;
            selectionGUI.OnRandomRoom += OpenRandomRoomGUI;
            selectionGUI.OnRoomList += OpenLobbyGUI;
            createRoomGUI.OnCancel += OpenSelectionGUI;
            lobbyGUI.OnExitLobby += OpenSelectionGUI;
        }

        public bool IsMenuOpen => optionsGUI.IsOpen || winGUI.IsOpen || GameManager.Instance.CurrentGameState == GameManager.GameState.InLobby;

        public void SetGameWinner(string winnerName) => winGUI.SetWinner(winnerName);

        // Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenMenus() => MenusGUI.SetActive(true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenLoginGUI() => loginGUI.gameObject.SetActive(true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenSelectionGUI()
        {
            if (PhotonNetwork.OfflineMode)
            {
                OpenLoginGUI();
            }
            else
            {
                selectionGUI.gameObject.SetActive(true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenCreateRoomGUI() => createRoomGUI.gameObject.SetActive(true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenRandomRoomGUI()
        {
            randomRoomGUI.SetActive(true);

            if (!PhotonNetwork.JoinRandomRoom())
            {
                randomRoomGUI.SetActive(false);
                MyPhoton.CreateRoom("Room " + Random.Range(1000, 10000), false, 7, 0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenLobbyGUI() => lobbyGUI.gameObject.SetActive(true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenRoomGUI() => roomGUI.gameObject.SetActive(true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CloseGameWindows()
        {
            optionsGUI.CloseWindow();
            winGUI.CloseWindow();
        }

        /// <summary>
        /// Stay in room
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetToGUI()
        {
            GameManager.Instance.EndGame();
            ResetNames();

            CloseGameWindows();
            OpenMenus();
        }

        // Pun Callbacks

        public override void OnDisconnected(DisconnectCause cause) => OpenLoginGUI();

        public override void OnCreateRoomFailed(short returnCode, string message) => OpenSelectionGUI();

        public override void OnConnectedToMaster()
        {
            if (!PhotonNetwork.OfflineMode) OpenSelectionGUI();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            randomRoomGUI.SetActive(false);
            MyPhoton.CreateRoom("Room " + Random.Range(1000, 10000), false, 7, 0);
        }

        public override void OnJoinedRoom()
        {
            randomRoomGUI.SetActive(false);
            OpenRoomGUI();
        }

        public override void OnLeftRoom()
        {
            if (GameManager.Instance.CurrentGameState == GameManager.GameState.InGame)
            {
                ResetToGUI();
            }

            OpenSelectionGUI();
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case MyPhoton.START_GAME:

                    GameManager.Instance.StartGame((byte)PhotonNetwork.CurrentRoom.CustomProperties[MyPhoton.NUMBER_OF_BOTS]);

                    roomGUI.gameObject.SetActive(false);
                    MenusGUI.SetActive(false);

                    break;


                case MyPhoton.RESTART_GAME:

                    CloseGameWindows();

                    GameManager.Instance.ResetGame();

                    break;


                case MyPhoton.BACK_TO_ROOM:

                    ResetToGUI();

                    OpenRoomGUI();

                    break;
            }
        }

        //++ Game Management

           
        [HideInInspector]
        public ColorSelectorGUI ColorSelectorGUI { get; private set; }
        [HideInInspector]
        public UnoButtonGUI UnoButtonGUI { get; private set; }

        #region Name Text

        [SerializeField]
        public GameObject NameTextParent = null;

        private TextMeshProUGUI[] nameTextComponents;
        private int nameTextComponentIndex = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetNames()
        {
            var length = nameTextComponents.Length;
            for (int i = 0; i < length; i++)
            {
                var comp = nameTextComponents[i];
                comp.text = string.Empty;
                comp.color = CardColor.Green.GetColor();
            }

            nameTextComponentIndex = -1;
        }

        public TextMeshProUGUI GetNameTextComponent()
        {
            nameTextComponentIndex++;
            return nameTextComponents[nameTextComponentIndex];
        }

        #endregion
    }
}