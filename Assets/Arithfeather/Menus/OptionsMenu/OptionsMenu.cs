using UnityEngine.UI;

namespace Arithfeather.Menus
{
	public class OptionsMenu : Menu
	{
		public Button BackButton;

		protected override void Initialize()
		{
			BackButton.onClick.AddListener(() => MenuController.Singleton.SwitchMenus(MenuType.MainMenu));
		}
	}
}