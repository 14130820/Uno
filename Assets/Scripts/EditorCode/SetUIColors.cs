#if UNITY_EDITOR

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Uno.Game.Cards;

namespace Uno.EditorCode
{
    public class SetUIColors : MonoBehaviour
    {
        [Button(ButtonHeight = 30)]
        public void SetColors()
        {
            var RedButton = GameObject.Find("RedButton").GetComponent<Button>();
            var BlueButton = GameObject.Find("BlueButton").GetComponent<Button>();
            var YellowButton = GameObject.Find("YellowButton").GetComponent<Button>();
            var GreenButton = GameObject.Find("GreenButton").GetComponent<Button>();

            if (RedButton != null &&
                BlueButton != null &&
                YellowButton != null &&
                GreenButton != null)
            {
                const float TRANSPARENCY = 0.3f;
                var defaultBlock = ColorBlock.defaultColorBlock;

                var color = CardColor.Red.GetColor();
                defaultBlock.highlightedColor = color;
                color.a = TRANSPARENCY;
                defaultBlock.normalColor = color;
                RedButton.colors = defaultBlock;

                color = CardColor.Blue.GetColor();
                defaultBlock.highlightedColor = color;
                color.a = TRANSPARENCY;
                defaultBlock.normalColor = color;
                BlueButton.colors = defaultBlock;

                color = CardColor.Yellow.GetColor();
                defaultBlock.highlightedColor = color;
                color.a = TRANSPARENCY;
                defaultBlock.normalColor = color;
                YellowButton.colors = defaultBlock;

                color = CardColor.Green.GetColor();
                defaultBlock.highlightedColor = color;
                color.a = TRANSPARENCY;
                defaultBlock.normalColor = color;
                GreenButton.colors = defaultBlock;
            }
        }
    }
}
#endif