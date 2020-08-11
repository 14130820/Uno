using ObjectPooling;
using Photon.Pun;
using UnityEngine;
using Uno.Game.Cards;
using Uno.Game.Players;

namespace Uno.Game.Display
{
    public class ShowTurnDisplayAction : DisplayActionBase
	{
        private BaseController currentTurn;
        private BaseController previousTurn;
		private bool hasPreviousHand;
        
        public void CreateAction(BaseController currentTurn, BaseController previousTurn)
		{
            this.currentTurn = currentTurn;
            this.previousTurn = previousTurn;
			hasPreviousHand = true;

            QueueThis();
        }
        public void CreateAction(BaseController currentTurn)
        {
            this.currentTurn = currentTurn;
            hasPreviousHand = false;

            QueueThis();
        }

        public override void ActionStart()
        {
            if (hasPreviousHand)
            {
                // Check for winner
                if (previousTurn.Hand.OwnedCards.Count == 0)
                {
                    GameManager.Instance.SetWinner(previousTurn.Name);

                    SendActionToManager(this);
                    this.PoolThis();
                    return;
                }
                
                if (!previousTurn.IsLocal)
                {
                    previousTurn.NameComponent.color = CardColor.Green.GetColor();


                }
                else
                {

                }

                if (previousTurn.IsProtected && previousTurn.Hand.OwnedCards.Count == 1) previousTurn.Hand.OwnedCards[0].SetUno(true);
            }

            if (!currentTurn.IsLocal)
            {
                currentTurn.NameComponent.color = CardColor.Red.GetColor();
				

            }
            else
            {

            }

            if (currentTurn.IsProtected)
            {
                var holder = currentTurn.Hand.CardPlaceHolders;
                var length = holder.Count;
                for (int i = 0; i < length; i++)
                {
                    holder[i].Card.SetUno(false);
                }

                currentTurn.IsProtected = false;
            }

            currentTurn.YourTurn();

            SendActionToManager(this);
            this.PoolThis();
        }
        
        public override void ActionUpdate() { }
	}
}
 