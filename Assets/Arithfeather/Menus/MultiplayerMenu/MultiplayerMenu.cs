using Mirror;
using UnityEngine.UI;

namespace Arithfeather.Menus
{
	public class MultiplayerMenu : Menu
	{
		public Button joinGameButton;
		public Button hostGameButton;
		public Button backButton;

		protected override void Initialize()
		{
			joinGameButton.onClick.AddListener(OpenJoinGameWindow);
			hostGameButton.onClick.AddListener(OpenHostGameWindow);
			backButton.onClick.AddListener(OpenMainMenuWindow);
		}

		private void OpenJoinGameWindow()
		{
			MenuController.Singleton.SwitchMenus(MenuType.JoinGameMenu);
		}

		private void OpenHostGameWindow()
		{
			MenuController.Singleton.SwitchMenus(MenuType.LobbyMenu);
			NetworkManager.singleton.StartHost();
		}

		private void OpenMainMenuWindow()
		{
			MenuController.Singleton.SwitchMenus(MenuType.MainMenu);
		}

	}
}