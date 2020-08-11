using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Uno.Game.Cards;
using Uno;

[RequireComponent(typeof(SpriteRenderer))]
public class CardShaderController : MonoBehaviour
{
	// Static shader property IDs		
	private static readonly int edgeTypeColor = Shader.PropertyToID("_FillColor_Color_3");
	private static readonly int middleTypeColor = Shader.PropertyToID("_FillColor_Color_1");
	private static readonly int edgeColor = Shader.PropertyToID("_FillColor_Color_2");

	private static readonly int iconOne = Shader.PropertyToID("SpriteSheetFrameUV_Frame_1");
	private static readonly int iconTwo = Shader.PropertyToID("SpriteSheetFrameUV_Frame_2");
	private static readonly int iconThree = Shader.PropertyToID("SpriteSheetFrameUV_Frame_3");

	private static readonly int backFrontDisplay = Shader.PropertyToID("_OperationBlend_Fade_4");
	private static readonly int hightlight = Shader.PropertyToID("_OperationBlend_Fade_3");
	private static readonly int uno = Shader.PropertyToID("_OperationBlend_Fade_5");

	private SpriteRenderer SpriteRenderer { get; set; }
    public int RenderOrder { get => SpriteRenderer.sortingOrder; set => SpriteRenderer.sortingOrder = value; }
    public int RenderLayer { set => SpriteRenderer.sortingLayerID = value; }

	private MaterialPropertyBlock MaterialPropertyBlock { get; set; }

	[SerializeField]
	private bool showBack = false;

	private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        MaterialPropertyBlock = new MaterialPropertyBlock();
		
        // Set Initial Values
        if (showBack)
        {
            SpriteRenderer.GetPropertyBlock(MaterialPropertyBlock);

            MaterialPropertyBlock.SetFloat(backFrontDisplay, 1);

            SpriteRenderer.SetPropertyBlock(MaterialPropertyBlock);
        }
    }
	
    #region Card Color

    private CardColor currentEdgeColor;
    public void SetEdgeColor(CardColor color)
    {
        if (color != currentEdgeColor)
        {
            SpriteRenderer.GetPropertyBlock(MaterialPropertyBlock);

            MaterialPropertyBlock.SetColor(edgeColor, color.GetColor());

            SpriteRenderer.SetPropertyBlock(MaterialPropertyBlock);

            currentEdgeColor = color;
        }
    }
    
    #endregion
    
    public void SetCard(CardData data)
    {
        var cardColor = data.Color;
        var color = cardColor.GetColor();

        SpriteRenderer.GetPropertyBlock(MaterialPropertyBlock);
        
        MaterialPropertyBlock.SetColor(edgeColor, color);

        if (cardColor == CardColor.Any)
        {
            var invis = CardColor.None.GetColor();
            MaterialPropertyBlock.SetColor(edgeTypeColor, invis);
            MaterialPropertyBlock.SetColor(middleTypeColor, invis);
        }
        else
        {
            MaterialPropertyBlock.SetColor(middleTypeColor, color);
        }
        
        var index = data.Type.GetHashCode();
        MaterialPropertyBlock.SetInt(iconOne, index);
        MaterialPropertyBlock.SetInt(iconTwo, index);
        MaterialPropertyBlock.SetInt(iconThree, index);

        SpriteRenderer.SetPropertyBlock(MaterialPropertyBlock);
    }
    
    #region Highlight

    private bool isHighlighted = false;
    public void Highlight()
    {
        if (!isHighlighted)
        {
            SpriteRenderer.GetPropertyBlock(MaterialPropertyBlock);

            MaterialPropertyBlock.SetFloat(hightlight, 0);

            SpriteRenderer.SetPropertyBlock(MaterialPropertyBlock);

            isHighlighted = true;
        }
    }
    public void UnHighlight()
    {
        if (isHighlighted)
        {
            SpriteRenderer.GetPropertyBlock(MaterialPropertyBlock);

            MaterialPropertyBlock.SetFloat(hightlight, 1);

            SpriteRenderer.SetPropertyBlock(MaterialPropertyBlock);

            isHighlighted = false;
        }
    }

    #endregion

    private bool isUnoOn = false;
    public void SetUno(bool on)
    {
        if (!isUnoOn && on)
        {
            SpriteRenderer.GetPropertyBlock(MaterialPropertyBlock);

            MaterialPropertyBlock.SetFloat(uno, 1);

            SpriteRenderer.SetPropertyBlock(MaterialPropertyBlock);

            isUnoOn = true;
        }
        else if (isUnoOn && !on)
        {
            SpriteRenderer.GetPropertyBlock(MaterialPropertyBlock);

            MaterialPropertyBlock.SetFloat(uno, 0);

            SpriteRenderer.SetPropertyBlock(MaterialPropertyBlock);

            isUnoOn = false;
        }
    }
}
