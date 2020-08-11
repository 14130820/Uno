using ObjectPooling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.Game.Cards;

namespace Uno.Game.Display
{
    public class DirectionColorDisplayAction : DisplayActionBase
    {
        private bool setColor = false;
        private CardColor cardColor = CardColor.Any;

        public void CreateAction(CardColor color)
        {
            cardColor = color;
            this.setColor = true;

            QueueThis();
        }

        public void CreateAction()
        {
            this.setColor = false;

            QueueThis();
        }

        public override void ActionStart()
        {
            if (setColor)
            {
                GameManager.Instance.SetDirectionColor(cardColor);
            }
            else
            {
                GameManager.Instance.ChangeDirection();
            }
            SendActionToManager(this);
            this.PoolThis();
        }

        public override void ActionUpdate() { }


    }
}
