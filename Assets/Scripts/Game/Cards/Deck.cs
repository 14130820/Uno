using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uno.Game.Display;

namespace Uno.Game.Cards
{
	[HideReferenceObjectPicker]
	[HideLabel]
    [System.Serializable]
	public class Deck
	{
		private readonly Stack<Card> cards = new Stack<Card>(UnoGame.CARD_CAPACITY);

        public static bool HasTopCard { get; private set; }
        public static Card TopCard { get; private set; }

		/// <summary>
		/// Put card in bottom of the deck.
		/// </summary>
		public void GiveCard(Card card)
		{
			cards.Push(card);

            TopCard = card;
            HasTopCard = true;
		}

		/// <summary>
		/// Take a card from the deck
		/// Take a card from the deck
		/// </summary>
		public bool TryGetCard(out Card card)
        {
            if (cards.Count == 0)
			{
                card = null;
                HasTopCard = false;
                return false;
			}
			else
            {
                card = cards.Pop();

                if (cards.Count != 0)
                {
                    TopCard = cards.Peek();
                    HasTopCard = true;

                }
                else
                {
                    HasTopCard = false;
                }

                return true;
			}
		}

		public void Clear() => cards.Clear();
		public int Count => cards.Count;
	}
}