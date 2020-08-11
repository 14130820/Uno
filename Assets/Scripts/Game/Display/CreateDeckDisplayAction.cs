using ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uno.Game.Cards;
using static Uno.Game.LerpMovement;

namespace Uno.Game.Display
{
	public class CreateDeckDisplayAction : DisplayActionBase
	{
        private const float SHUFFLE_ANIMATION_SPEED = 2f;
		/// <summary>
		/// How fast the cards shuffle
		/// </summary>
		private const float CardShuffleInterval = 0;//0.02f;

		private static readonly Vector3 startPosition = new Vector3(0.75f, 0, 0);

		public override void ActionStart() { }

        private static readonly Vector3BezierCurve bezier = new Vector3BezierCurve(new Vector3(3 ,0, -5), new Vector3(0 ,0, 8));
        private static readonly LerpCurve lerp = new LerpCurve(3, LerpCurve.Curve.Both);
        private float endActionTimer = SHUFFLE_ANIMATION_SPEED;

		public override void ActionUpdate()
		{
			shuffleTimer -= Time.deltaTime;
			if (shuffleTimer <= 0)
			{
				currentCard++;

				if (currentCard < cardsToShuffle)
				{
					var card = cards[currentCard];

                    card.SetDefaultCard();
                    card.RenderOrder = 0;

                    card.MovementScript.MoveTo(
                        startPosition + UnoGame.CARD_STACKING_DISTANCE * currentCard,
                        Quaternion.LookRotation(Vector3.up, Vector3.forward), 
                        bezier, lerp, SHUFFLE_ANIMATION_SPEED);
                    
					shuffleTimer = CardShuffleInterval;
				}
				else
				{
                    endActionTimer -= Time.deltaTime;
                    if (endActionTimer <= 0)
                    {
                        SendActionToManager(this);
                        this.PoolThis();
                    }
				}
			}
		}

        private List<Card> cards = null;
        private int cardsToShuffle = 0;

        private int currentCard = 0;
        private float shuffleTimer = 0f;
        
        public void CreateAction(List<Card> cards)
		{
            shuffleTimer = 0;
            currentCard = -1;

            endActionTimer = SHUFFLE_ANIMATION_SPEED;
			this.cards = cards;
            cardsToShuffle = cards.Count;

            QueueThis();
        }
	}
}