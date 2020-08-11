using FeatherWorks.Pooling;
using ObjectPooling;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Uno.Game.Display;
using Uno.RuleSets;

namespace Uno.Game.Cards
{
    /// <summary>
    /// Manages the deck and garbage, shuffling, rebuilding the deck
    /// Holds references to the cards.
    /// </summary>
    [System.Serializable]
    public class CardManager
    {
        /// <summary>
        /// The cards in the deck on the  table.
        /// </summary>
        [ShowInInspector]
        [PropertyOrder(3)]
        private readonly Deck deck = new Deck();

        /// <summary>
        /// The cards under the current card in play.
        /// </summary>
        [ShowInInspector]
        [PropertyOrder(3)]
        private readonly Garbage garbage = new Garbage();

        public void Awake()
        {
            CreateRawCards();
            CreateCardPrefabs();
        }

        public void StartGame() => CreateDeck();

        /// <summary>
        /// Restarts the game with same rules
        /// </summary>
        public void RestartGame()
        {
            Reset();
            CreateDeck();
        }

        public void ResetFull() => Reset();
        // If reseting prefabs
        //var pool = GameManager.Instance.CardPrefabGroup;
        //var length = cardPrefabs.Count;
        //for (int i = 0; i < length; i++)
        //{
        //    pool.Despawn(cardPrefabs[i].gameObject);
        //}

        //cardPrefabs.Clear();


        private void Reset()
        {
            deck.Clear();
            garbage.ClearGarbage();

            var length = cardPrefabs.Count;
            for (int i = 0; i < length; i++)
            {
                cardPrefabs[i].Resetcard();
            }

            var pool = GameManager.Instance.CardPrefabGroup;
            length = extra_cardPrefabs.Count;
            for (int i = 0; i < length; i++)
            {
                var card = extra_cardPrefabs[i];
                card.Resetcard();
                pool.Despawn(card.gameObject);
            }

            extra_cardPrefabs.Clear();
        }

        /// <summary>
        /// Resets the cardmanager for a new game.
        /// Remember to clear player hands.
        /// </summary>
        private void CreateDeck()
        {
            var gm = GameManager.Instance;

            cardPrefabs.Shuffle();

            (gm.CreateDeckDAPool.GetInstance() as CreateDeckDisplayAction).CreateAction(cardPrefabs);

            var count = cardPrefabs.Count;
            for (int i = 0; i < count; i++)
            {
                deck.GiveCard(cardPrefabs[i]);
            }

            deck.TryGetCard(out Card topCard);
            garbage.GiveCard(topCard);

            (gm.PlayCardDAPool.GetInstance() as PlayCardDisplayAction).CreateAction(topCard);
            (gm.DirectionColorDAPool.GetInstance() as DirectionColorDisplayAction).CreateAction(topCard.CardColor);
        }

        /// <summary>
        /// Returns a card from the top of the deck pile.
        /// </summary>
        public bool TryDrawCard(out Card card, TakeCardDisplayAction displayAction)
        {
            if (deck.TryGetCard(out Card _card))
            {
                displayAction.CreateAction(_card);

                // Check if we have no more cards
                if (deck.Count == 0)
                {
                    var garbageCount = garbage.Count;

                    // Check if we can't shuffle
                    if (garbageCount == 0)
                    {
                        OutOfCards();
                    }
                    else
                    {
                        ShuffleCards();
                    }
                }

                card = _card;
                return true;
            }
            else
            {
                displayAction.PoolThis();
                card = null;
                return false;
            }
        }

        /// <summary>
        /// Adds a card to the top of the garbage pile.
        /// </summary>
        public void PlayCard(Card card, PlayCardDisplayAction action)
        {
            garbage.GiveCard(card);

            action.CreateAction(card, garbage.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OutOfCards()
        {
            // Create prefabs
            var featherPool = FeatherPoolManager.Instance;

            var cardDataLength = rawCardData.Length;
            int numberOfCards;

            for (int i = 0; i < cardDataLength; i++)
            {
                var cardTemplate = rawCardData[i];

                if (cardTemplate.Type == CardType.PlusTwo || cardTemplate.Type == CardType.PlusFour)
                {
                    continue;
                }

                if (cardTemplate.Color == CardColor.Any)
                {
                    numberOfCards = 2;
                }
                else
                {
                    numberOfCards = 1;
                }

                var group = GameManager.Instance.CardPrefabGroup;
                while (numberOfCards > 0)
                {
                    numberOfCards--;

                    var cardObj = group.Spawn();
                    var card = cardObj.GetComponent<Card>();

                    card.SetCard(rawCardData[i]);
                    extra_cardPrefabs.Add(card);
                }
            }

            // Create deck
            extra_cardPrefabs.Shuffle();

            (GameManager.Instance.CreateDeckDAPool.GetInstance() as CreateDeckDisplayAction).CreateAction(extra_cardPrefabs);

            var count = extra_cardPrefabs.Count;
            for (int i = 0; i < count; i++)
            {
                deck.GiveCard(extra_cardPrefabs[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ShuffleCards()
        {
            var cards = garbage.Cards;

            var copy = cards.GetCopy();
            var shuffledCards = copy.GetCopy();
            shuffledCards.Shuffle();
            cards.Clear();

            var garbageCount = shuffledCards.Count;
            for (int i = 0; i < garbageCount; i++)
            {
                var card = shuffledCards[i];

                card.Resetcard();

                deck.GiveCard(card);
            }

            (GameManager.Instance.ShuffleGarbageDAPool.GetInstance() as ShuffleGarbageDisplayAction).CreateAction(copy, shuffledCards, Garbage.TopCard);
            (GameManager.Instance.CreateDeckDAPool.GetInstance() as CreateDeckDisplayAction).CreateAction(shuffledCards);
        }

        #region Deck Factory

        /// <summary>
        /// The ScriptableObject card data.
        /// </summary>
        private CardData[] rawCardData = new CardData[54];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateRawCards()
        {
            for (byte i = 0; i < 13; i++)
            {
                CardType type = (CardType)i;
                rawCardData[i * 4 + 0] = (new CardData(CardColor.Blue, type));
                rawCardData[i * 4 + 1] = (new CardData(CardColor.Yellow, type));
                rawCardData[i * 4 + 2] = (new CardData(CardColor.Green, type));
                rawCardData[i * 4 + 3] = (new CardData(CardColor.Red, type));
            }

            rawCardData[52] = (new CardData(CardColor.Any, CardType.PlusFour));
            rawCardData[53] = (new CardData(CardColor.Any, CardType.Wild));
        }

        /// <summary>
        /// Total cards in an uno deck.
        /// Caching this value to start a new game faster.
        /// </summary>
        private List<Card> cardPrefabs = new List<Card>(UnoGame.CARD_CAPACITY);
        /// <summary>
        /// Total cards in an uno deck.
        /// Caching this value to start a new game faster.
        /// </summary>
        private List<Card> extra_cardPrefabs = new List<Card>(UnoGame.CARD_CAPACITY / 2);

        /// <summary>
        /// Creates the cards.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateCardPrefabs()
        {
            var featherPool = FeatherPoolManager.Instance;

            var cardDataLength = rawCardData.Length;
            int numberOfCards;

            for (int i = 0; i < cardDataLength; i++)
            {
                var cardTemplate = rawCardData[i];

                if (cardTemplate.Color == CardColor.Any)
                {
                    numberOfCards = 4;
                }
                else
                {
                    numberOfCards = 2;
                }

                var group = GameManager.Instance.CardPrefabGroup;
                while (numberOfCards > 0)
                {
                    numberOfCards--;

                    var cardObj = group.Spawn();
                    var card = cardObj.GetComponent<Card>();

                    card.SetCard(rawCardData[i]);
                    cardPrefabs.Add(card);
                }
            }
        }

        #endregion
    }
}