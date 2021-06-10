using UnityEngine;

namespace Arithfeather.Cards
{
	public class CardShaderController : MonoBehaviour
	{
		private static readonly int colorId = Shader.PropertyToID("_Color");
		private static readonly int frameId = Shader.PropertyToID("_Frame");
		private static readonly int highLightId = Shader.PropertyToID("_HighlightIntensity");
		
		private Renderer Renderer { get; set; }
		private MaterialPropertyBlock MaterialPropertyBlock { get; set; }
		
		private void Awake()
		{
			Renderer = GetComponent<Renderer>();
			MaterialPropertyBlock = new MaterialPropertyBlock();
			Renderer.GetPropertyBlock(MaterialPropertyBlock);
		}
		
		public void SetCard(CardData data)
		{
			CardColor cardColor = data.Color;
			Color color = cardColor.GetColor();
			
			MaterialPropertyBlock.SetColor(colorId, color);

			int cardFrame = (int)data.Type;
			int y = Mathf.FloorToInt(cardFrame / 4f);
			int x = (cardFrame % 4);
			
			MaterialPropertyBlock.SetVector(frameId, new Vector4(x, y, 0, 0));

			Renderer.SetPropertyBlock(MaterialPropertyBlock);
		}
		
		public void Highlight()
		{

		}

		public void SetUno(bool on)
		{

		}
	}
}