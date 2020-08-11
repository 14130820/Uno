using ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uno.Game.Cards;
using Uno.Game.Players;

namespace Uno.Game.Display
{
    public class TakeCardDisplayAction : DisplayActionBase
    {
        public override void ActionStart()
        {
            if (player.IsLocal)
            {
                cardToTake.SetLocalPlayerCard();
            }
            else
            {
                cardToTake.SetNonLocalHandCard();
            }

            player.Hand.CreatePlaceHolder(cardToTake);

            sentNextAction = false;
            playedEndSound = false;
            delayTimer = 0;

            AudioManager.Instance.PlayTakeCardSound(cardToTake.transform.position);
    }

        private float delayTimer;
        private bool sentNextAction;
        private bool playedEndSound;

        public override void ActionUpdate()
        {
            var time = Time.deltaTime;
            delayTimer += time;
            
            if (!sentNextAction && delayTimer >= UnoGame.DELAY_BETWEEN_DEALING_CARD)
            {
                sentNextAction = true;
                SendActionToManager(this, ActionFlags.StartNextAction);

                if (DontWaitForCardToReachHand)
                {
                    SendActionToManager(this, ActionFlags.EndAction);
                    this.PoolThis();
                }
            }

            if (!playedEndSound && delayTimer >= UnoGame.DELAY_BETWEEN_DEALING_CARD + time)
            {
                playedEndSound = true;
                AudioManager.Instance.PlayTakeCardEndSound(cardToTake.transform.position);
            }

            if (delayTimer >= UnoGame.DECK_TO_HAND_SPEED)
            {
                cardToTake.LocalPlayerCard = player.IsLocal ? true : false;
                
                player.Hand.UpdateCardPositions();

                SendActionToManager(this, ActionFlags.EndAction);
                this.PoolThis();
            }

        }

        public bool DontWaitForCardToReachHand { set; private get; }

        private Card cardToTake;
        private BaseController player;

        public void Initialize(BaseController hand)
        {
            this.player = hand;
            DontWaitForCardToReachHand = false;
        }

        public void CreateAction(Card cardToTake)
        {
            this.cardToTake = cardToTake;

            QueueThis();
        }
    }
}