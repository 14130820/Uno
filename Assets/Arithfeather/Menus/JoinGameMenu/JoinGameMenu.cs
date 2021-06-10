using System;
using kcp2k;
using Mirror;
using TMPro;
using UnityEngine.UI;

namespace Arithfeather.Menus
{
	public class JoinGameMenu : Menu
	{
		public Button ConnectButton, CancelButton;
		public TMP_InputField playerName, ipAddress;

		private TextMeshProUGUI connectButtonText;

		protected override void Initialize()
		{
			ConnectButton.onClick.AddListener(ConnectToGame);
			CancelButton.onClick.AddListener(() => MenuController.Singleton.SwitchMenus(MenuType.MultiPlayerMenu));
			connectButtonText = ConnectButton.GetComponentInChildren<TextMeshProUGUI>();
			CustomNetworkManager.ClientDisconnect += OnEnable;
		}

		private void OnEnable()
		{
			connectButtonText.text = "Connect";
		}

		private void ConnectToGame()
		{
			if (string.IsNullOrEmpty(ipAddress.text) ||
			ipAddress.text.Length < 2)
			{
				ConsoleMessageManager.Singleton.DisplayMessage(new ConsoleMessageManager.ConsoleMessage("Invalid IP Address", 1));
				return;
			}

			//NetworkClient.localPlayer.name = name.text;

			NetworkManager.singleton.networkAddress = ipAddress.text;

			try
			{
				NetworkManager.singleton.StartClient();
			}
			catch (Exception e)
			{
				ConsoleMessageManager.Singleton.DisplayMessage(new ConsoleMessageManager.ConsoleMessage(e.Message, 2));
				NetworkManager.singleton.StopClient();
				return;
			}

			connectButtonText.text = "Connecting...";
		}
	}
}