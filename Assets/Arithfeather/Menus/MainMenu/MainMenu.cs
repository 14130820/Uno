using UnityEngine;
using UnityEngine.UI;

namespace Arithfeather.Menus
{
	public class MainMenu : Menu
	{
		public Button Singleplayer;
		public Button Multiplayer;
		public Button Options;
		public Button Exit;

		protected override void Initialize()
		{
			Singleplayer.onClick.AddListener(OpenSinglePlayerWindow);
			Multiplayer.onClick.AddListener(OpenMultiPlayerWindow);
			Options.onClick.AddListener(OpenOptionsWindow);
			Exit.onClick.AddListener(Application.Quit);
		}

		private void OpenSinglePlayerWindow()
		{
			MenuController.Singleton.SwitchMenus(MenuType.SinglePlayerMenu);
		}

		private void OpenMultiPlayerWindow()
		{
			MenuController.Singleton.SwitchMenus(MenuType.MultiPlayerMenu);
		}

		private void OpenOptionsWindow()
		{
			MenuController.Singleton.SwitchMenus(MenuType.OptionsMenu);
		}
	}
}