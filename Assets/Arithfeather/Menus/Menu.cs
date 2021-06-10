using UnityEngine;

namespace Arithfeather.Menus
{
	/// <summary>
	/// Base for all menus.
	/// </summary>
	public abstract class Menu : MonoBehaviour
	{
		public MenuType MenuType;

		public Canvas Canvas { get; private set; }

		protected virtual void Initialize() { }

		/// <summary>
		/// Warning: Override Initialize instead!
		/// </summary>
		private void Awake()
		{
			Canvas = GetComponent<Canvas>();
			Initialize();
		}
	}
}