using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Uno.Game.Cards;

namespace Uno.GUI
{
    public class ColorSelectorGUI : MonoBehaviour
    {
        [SerializeField]
        private Button redButton = null;
        [SerializeField]
        private Button blueButton = null;
        [SerializeField]
        private Button yellowButton = null;
        [SerializeField]
        private Button greenButton = null;

        private void Awake()
        {
            redButton.onClick.AddListener(() => SelectColor(CardColor.Red));
            blueButton.onClick.AddListener(() => SelectColor(CardColor.Blue));
            yellowButton.onClick.AddListener(() => SelectColor(CardColor.Yellow));
            greenButton.onClick.AddListener(() => SelectColor(CardColor.Green));
        }

        public delegate void ColorSelected(CardColor color);
        public event ColorSelected OnColorSelected;

        public void SelectColor(CardColor color)
        {
            OnColorSelected?.Invoke(color);
            this.gameObject.SetActive(false);
        }

        public void OpenColorSelector() => this.gameObject.SetActive(true);
    }
}