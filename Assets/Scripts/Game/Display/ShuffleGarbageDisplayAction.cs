using ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uno.Game.Cards;
using static Uno.Game.LerpMovement;

namespace Uno.Game.Display
{
    public class ShuffleGarbageDisplayAction : DisplayActionBase
    {
        private static readonly Vector3 movePosition = new Vector3(-0.74f, 1, 1);
        private static readonly Quaternion rotation = Quaternion.LookRotation(Vector3.forward);
        private static readonly Vector3BezierCurve curve = new Vector3BezierCurve(Vector3.forward * 15);

        public override void ActionStart() { }

        public override void ActionUpdate()
        {
            if (cardsToShuffle >= 0)
            {
                var card = oldCards[cardsToShuffle];

                card.MovementScript.MoveTo(movePosition, rotation, curve, LerpCurve.Default, 1f, false);

                topCard.transform.Translate(-UnoGame.CARD_STACKING_DISTANCE, Space.World);

                cardsToShuffle--;
            }
            else
            {
                SendActionToManager(this);
                this.PoolThis();
            }
        }

        private List<Card> oldCards;
        private List<Card> shuffledCards;
        private Card topCard;
        private int cardsToShuffle;
        private int garbageSize;
        
        public void CreateAction(List<Card> oldCards, List<Card> shuffledCards, Card topCard)
        {
            this.oldCards = oldCards;
            this.shuffledCards = shuffledCards;
            this.topCard = topCard;

            garbageSize = oldCards.Count - 1;
            cardsToShuffle = garbageSize;

            QueueThis();
        }
    }
}