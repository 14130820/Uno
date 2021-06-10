using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Arithfeather.Menus
{
	/// <summary>
	/// Controls Menu navigation.
	/// </summary>
	public class MenuController : MonoBehaviour
	{
		public static MenuController Singleton;

		[SerializeField] private Canvas GUIBackground;
		[SerializeField]
		private float backgroundScrollTime = 2.0f;

		private Menu[] menus;
		private Material backgroundMaterial;
		private Vector4 scrollDirection;
		private int scrollDirectionId;

		private void Awake()
		{
			Singleton = this;
			menus = GetComponentsInChildren<Menu>();
			backgroundMaterial = GUIBackground.GetComponent<RawImage>().material;
			scrollDirectionId = Shader.PropertyToID("_Direction");

			scrollDirection.x = Random.Range(0, 2) > 0.5 ? 1 : -1;
			scrollDirection.y = Random.Range(0, 2) > 0.5 ? 1 : -1;
			backgroundMaterial.SetVector(scrollDirectionId, scrollDirection);
		}

		/// <param name="menuType">The target menu to enable.</param>
		public void SwitchMenus(MenuType menuType)
		{
			foreach (Menu menu in menus)
			{
				bool enableMenu = menu.MenuType == menuType;

				menu.Canvas.enabled = enableMenu;
				menu.enabled = enableMenu;

				if (enableMenu)
					EnableMenu(menu);
				else if (menu.enabled)
					DisableMenu(menu);
			}
		}

		private void EnableMenu(Menu menu)
		{
			menu.Canvas.enabled = true;
			
			if (Random.Range(0, 2) > 0.5)
			{
				scrollDirection.x *= -1;
			}
			else
			{
				scrollDirection.y *= -1;
			}

			DOTween.To(
		() => backgroundMaterial.GetVector(scrollDirectionId),
		x => backgroundMaterial.SetVector(scrollDirectionId, x),
			 scrollDirection, backgroundScrollTime);

			// EventSystem Selection
			DefaultSelectedGUIElement firstTargetElement = menu.GetComponentInChildren<DefaultSelectedGUIElement>();
			if (firstTargetElement != null)
			{
				EventSystem.current.SetSelectedGameObject(firstTargetElement.gameObject);
			}
		}

		private void DisableMenu(Menu menu)
		{
			menu.Canvas.enabled = false;
		}
	}
}