using ObjectPooling;
using UnityEngine;
using Uno.Game.Cards;
using Uno.Game.Players;
using static Uno.Game.LerpMovement;

namespace Uno.Game.Display
{
    public class PlayCardDisplayAction : DisplayActionBase
    {
        private static readonly Vector3 startPosition = new Vector3(-0.75f, 0, 0);
        private static readonly Vector3BezierCurve bezCurve = new Vector3BezierCurve(Vector3.zero, Vector3.up * 3);
        private static readonly LerpCurve lerpCurve = new LerpCurve(3, LerpCurve.Curve.SmoothEnd);
        
        public override void ActionStart()
        {
            if (fromHand)
            {
                hand.RemovePlaceHolder(cardToPlay);
                cardToPlay.HighLightBack(false);
                cardToPlay.RenderOrder = 0;
            }

            // Put card on garbage.
            if (topCardSet)
            {
                var topCardPosition = startPosition + garbageSize * UnoGame.CARD_STACKING_DISTANCE;

                cardToPlay.MovementScript.MoveTo(
                    topCardPosition,
                    Quaternion.LookRotation(Vector3.down),
                    bezCurve, lerpCurve, UnoGame.HAND_TO_DECK_SPEED);
            }
            else
            {
                cardToPlay.SetNonLocalHandCard();

                cardToPlay.MovementScript.MoveTo(
                    startPosition,
                    Quaternion.LookRotation(Vector3.down),
                    bezCurve, lerpCurve, UnoGame.HAND_TO_DECK_SPEED);
            }
            

            delayTimer = UnoGame.HAND_TO_DECK_SPEED;
            playedSound = false;
        }

        private float delayTimer;
        private bool playedSound;

        public override void ActionUpdate()
        {
            delayTimer -= Time.deltaTime;

            if (!playedSound && delayTimer <= 0.18f)
            {
                playedSound = true;
                AudioManager.Instance.PlayCardEndSound(cardToPlay.transform.position);
            }

            if (delayTimer <= 0)
            {
                if (!fromHand)
                {
                    GameManager.Instance.DirectionIndicator.gameObject.SetActive(true);
                }

                cardToPlay.SetDefaultCard();
                SendActionToManager(this);
                this.PoolThis();
            }
        }

        private Card cardToPlay;
        private bool topCardSet;
        private int garbageSize;

        private Hand hand;
        private bool fromHand;

        public void Initialize(Hand fromHand)
        {
            this.hand = fromHand;
            this.fromHand = true;
        }
        public void CreateAction(Card cardToPlay, int garbageSize)
        {
            this.cardToPlay = cardToPlay;
            this.topCardSet = true;
            this.garbageSize = garbageSize;

            QueueThis();
        }

        public void CreateAction(Card cardToPlay)
        {
            this.cardToPlay = cardToPlay;
            this.topCardSet = false;
            this.fromHand = false;

            QueueThis();
        }
    }
}