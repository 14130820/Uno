using ObjectPooling;
using Uno.Game.Cards;
using Uno.Game.Display;

namespace Uno.Game.Players
{
    public class AIController : BaseController
    {
        public override void YourTurn()
        {
            var cards = Hand.OwnedCards;

            // AI Logic
            // Order: Plus 2, reverse or skip, normal card, wild card.

            Card skipOrReverse = null;
            bool skipOrReverseSet = false;

            Card wild = null;
            bool wildSet = false;

            Card normal = null;
            bool normalSet = false;

            var length = cards.Count;
            for (int i = 0; i < length; i++)
            {
                var card = cards[i];

                if (card.CanBePlayed)
                {
                    switch (card.CardType)
                    {
                        case CardType.Skip:
                        case CardType.Reverse:
                            if (!skipOrReverseSet)
                            {
                                skipOrReverse = card;
                                skipOrReverseSet = true;
                            }
                            continue;

                        case CardType.PlusTwo:
                            if (TryPlayCard(card))
                            {
                                CheckForLastCard();
                                return;
                            }
                            continue;

                        case CardType.PlusFour:
                        case CardType.Wild:
                            if (!wildSet)
                            {
                                wild = card;
                                wildSet = true;
                            }
                            continue;
                    }

                    if (!normalSet)
                    {
                        normal = card;
                        normalSet = true;
                    }
                }

            }

            if ((skipOrReverseSet && TryPlayCard(skipOrReverse)) ||
                (normalSet && TryPlayCard(normal)) ||
                (wildSet && TryPlayCard(wild)))
            {
            }
            else
            {
                TryPickupCard();
                (GameManager.Instance.PauseDAPool.GetInstance() as PauseDisplayAction).CreateAction(0.5f);
            }

            CheckForLastCard();
        }

        private void CheckForLastCard()
        {
            var chance = UnityEngine.Random.Range(0, 100);

            if (chance > 30) // Chance to call uno
            {
                FlowManager.UnoButton(this);
            }
        }

        public override void PickColor()
        {
            var cards = Hand.OwnedCards;

            int yellows = 0;
            int blues = 0;
            int reds = 0;
            int greens = 0;

            var length = cards.Count;
            for (int i = 0; i < length; i++)
            {
                switch (cards[i].CardColor)
                {
                    case CardColor.Red:
                        reds++;
                        break;
                    case CardColor.Yellow:
                        yellows++;
                        break;
                    case CardColor.Green:
                        greens++;
                        break;
                    case CardColor.Blue:
                        blues++;
                        break;
                }
            }

            CardColor bestColor = CardColor.Yellow;
            if (blues > yellows) bestColor = CardColor.Blue;
            if (reds > blues) bestColor = CardColor.Red;
            if (greens > reds) bestColor = CardColor.Green;

            switch (bestColor)
            {
                case CardColor.Yellow:
                    GameManager.Instance.ColorSelectorGUI.SelectColor(CardColor.Yellow);
                    break;
                case CardColor.Red:
                    GameManager.Instance.ColorSelectorGUI.SelectColor(CardColor.Red);
                    break;
                case CardColor.Green:
                    GameManager.Instance.ColorSelectorGUI.SelectColor(CardColor.Green);
                    break;
                case CardColor.Blue:
                    GameManager.Instance.ColorSelectorGUI.SelectColor(CardColor.Blue);
                    break;
            }
        }
    }
}