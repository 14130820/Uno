using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Arithfeather.Menus
{
	public class StartMenu : Menu
	{
		public GameObject TitleGameObject;

		public float swirtStart = -40f;
		public float swirlSpeed = 2f;

		public float intensityStart = 1f;
		public float intensityPeak = 1.5f;
		public float intensitySpeed = 0.3f;

		public float blendOpacityTarget = 0.05f;
		public float blendSpeed = 3f;

		public Vector2 targetCornerPosition;
		public float targetCornerScale = 0.1f;
		public float cornerMoveSpeed = 1.5f;

		private Material swirlMaterial;
		private RectTransform rectTransform;
		private Sequence startSequence;
		private int swirlId;
		private int intensityId;
		private int blendOpacityId;
		private Vector3 savedScale;
		private Vector2 startPosition;

		protected override void Initialize()
		{
			GetComponentInChildren<Button>().onClick.AddListener(OpenMainMenuWindow);

			rectTransform = TitleGameObject.GetComponent<RectTransform>();
			savedScale = new Vector3(targetCornerScale, targetCornerScale, targetCornerScale);
			swirlMaterial = TitleGameObject.GetComponent<RawImage>().material;
			startPosition = rectTransform.anchoredPosition;
			swirlId = Shader.PropertyToID("_Swirl");
			intensityId = Shader.PropertyToID("_Intensity");
			blendOpacityId = Shader.PropertyToID("_BlendOpacity");
		}

		private void OpenMainMenuWindow()
		{
			MenuController.Singleton.SwitchMenus(MenuType.MainMenu);

			DOTween.To(
				() => rectTransform.anchoredPosition,
				x => rectTransform.anchoredPosition = x,
				targetCornerPosition,
				cornerMoveSpeed);

			TitleGameObject.transform.DOScale(
				savedScale,
				cornerMoveSpeed);
		}

		private void OnEnable()
		{
			swirlMaterial.SetFloat(swirlId, swirtStart);
			swirlMaterial.SetFloat(intensityId, intensityStart);
			swirlMaterial.SetFloat(blendOpacityId, 0);
			
			DOTween.To(
				() => rectTransform.anchoredPosition,
				x => rectTransform.anchoredPosition = x,
				startPosition,
				cornerMoveSpeed * 0.5f);

			TitleGameObject.transform.DOScale(
				Vector3.one,
				cornerMoveSpeed * 0.5f);

			startSequence = DOTween.Sequence();

			startSequence.Append(DOTween.To(
				() => swirlMaterial.GetFloat(swirlId),
				(a) => swirlMaterial.SetFloat(swirlId, a),
				0,
				swirlSpeed)).Append(DOTween.To(
				() => swirlMaterial.GetFloat(intensityId),
				(a) => swirlMaterial.SetFloat(intensityId, a),
				intensityPeak,
				intensitySpeed)).Append(DOTween.To(
				() => swirlMaterial.GetFloat(intensityId),
				(a) => swirlMaterial.SetFloat(intensityId, a),
				intensityStart,
				intensitySpeed)).Append(DOTween.To(
				() => swirlMaterial.GetFloat(blendOpacityId),
				(a) => swirlMaterial.SetFloat(blendOpacityId, a),
				blendOpacityTarget,
				blendSpeed));
		}
	}
}