using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uno.Game.Cards;

namespace Uno
{
    public static class UnoGame
    {
        public const int NUMBER_CARDS_DEALT = 7;

        public const int CARD_CAPACITY = 120;

        public const float DELAY_BETWEEN_DEALING_CARD = 0.1f;
        public const float DECK_TO_HAND_SPEED = 0.4f;
        public const float HAND_TO_DECK_SPEED = 0.8f;

        public static Vector3 CARD_STACKING_DISTANCE = new Vector3(0, 0.005f, 0);

        private static readonly Color transparentColor = new Color(0, 0, 0, 0);
        private static readonly Color red = new Color(0.92f, 0.076f, 0.07f);
        private static readonly Color green = new Color(0.2f, 0.8f, 0.06f);
        private static readonly Color yellow = new Color(0.9f, 0.86f, 0.12f);
        private static readonly Color blue = new Color(0.35f, 0.29f, 0.89f);

        public static Color GetColor(this CardColor color)
        {
            switch (color)
            {
                case CardColor.None:
                    return transparentColor;
                case CardColor.Red:
                    return red;
                case CardColor.Yellow:
                    return yellow;
                case CardColor.Green:
                    return green;
                case CardColor.Blue:
                    return blue;
                case CardColor.Any:
                    return Color.black;
            }
            return default;
        }
    }

    #region Global Enums

    [System.Serializable]
    public enum CardColor : byte
    {
        None,
        Red,
        Yellow,
        Green,
        Blue,
        Any
    }

    [System.Serializable]
    public enum CardType : byte
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Skip = 10,
        Reverse = 11,
        PlusTwo = 12,
        PlusFour = 13,
        Wild = 14
    }

    #endregion
}