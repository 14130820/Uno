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
	public class Garbage
	{
		private readonly List<Card> cards = new List<Card>(UnoGame.CARD_CAPACITY);
		public List<Card> Cards => cards;
		
		private static bool HasTopCard { get; set; }
		public static Card TopCard { get; private set; }
		public static int NumberOfSameCards { get; private set; } = 1;

		/// <summary>
		/// Put card on the garbage pile
		/// </summary>
		public void GiveCard(Card card)
		{
			if (HasTopCard)
			{
				cards.Add(TopCard);

				if (TopCard.CardType == card.CardType)
				{
					NumberOfSameCards++;
				}
				else
				{
					NumberOfSameCards = 1;
				}
			}
			else
			{
				HasTopCard = true;
			}
			
			TopCard = card;
		}
		
		public void ClearGarbage()
		{
			HasTopCard = false;
			cards.Clear();
		}

		public int Count => cards.Count;
	}
}
