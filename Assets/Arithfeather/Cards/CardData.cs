﻿namespace Arithfeather.Cards
{
    public struct CardData
    {
        public CardData(CardColor color, CardType type)
        {
            Color = color;
            Type = type;
        }

        public CardColor Color { get; set; }
        public CardType Type { get; set; }
    }
}