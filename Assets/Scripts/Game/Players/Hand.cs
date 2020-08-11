using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Uno.Game.Cards;
using Uno.Game.Display;

namespace Uno.Game.Players
{
	public class Hand : MonoBehaviour
	{
        private readonly List<Card> ownedCards = new List<Card>(30);
        private readonly List<CardPlaceHolder> cardPlaceHolders = new List<CardPlaceHolder>(30);

        private Transform thisTransform;
        private bool isLocal;
        private float maxHandWidth;
        
        private void Awake()
        {
            thisTransform = this.transform;
        }

        public List<Card> OwnedCards => ownedCards;
        public List<CardPlaceHolder> CardPlaceHolders => cardPlaceHolders;

        public bool IsLocal
        {
            set
            {
                var length = cardPlaceHolders.Count;
                if (value == true)
                {
                    maxHandWidth = 8;
                    for (int i = 0; i < length; i++)
                    {
                        cardPlaceHolders[i].transform.localRotation = Quaternion.LookRotation(Vector3.back);
                    }
                }
                else
                {
                    maxHandWidth = 2;
                    for (int i = 0; i < length; i++)
                    {
                        cardPlaceHolders[i].transform.localRotation = Quaternion.LookRotation(Vector3.forward);
                    }
                }
                isLocal = value;
            }
        }

        /// <summary>
        /// Faster card loop 
        /// </summary>
        public void RemoveCard(Card card)
        {
            var length = ownedCards.Count;
            for (int i = 0; i < length; i++)
            {
                if (card.GetInstanceID() == ownedCards[i].GetInstanceID())
                {
                    ownedCards.RemoveAt(i);
                    break;
                }
            }
        }

        public void Reset()
        {
            var length = cardPlaceHolders.Count;
            var pool = GameManager.Instance.PlaceHolderPrefabGroup;

            for (int i = 0; i < length; i++)
            {
                var PH = cardPlaceHolders[i];

                PH.Card.ResetParent();

                pool.Despawn(PH.gameObject.GetInstanceID());
            }

            cardPlaceHolders.Clear();

            ownedCards.Clear();
        }

        #region PlaceHolders
        
        public void UpdateCardPositions(float speed = 0.2f)
        {
            int num_cards = cardPlaceHolders.Count;
            for (int i = 0; i < num_cards; i++)
            {
                cardPlaceHolders[i].MoveCardToPosition(speed);
            }
        }
        
        public void CreatePlaceHolder(Card card)
        {
            var placeHolder = GameManager.Instance.PlaceHolderPrefabGroup.Spawn().GetComponent<CardPlaceHolder>();

            var placeHolderTransform = placeHolder.transform;
            placeHolderTransform.parent = thisTransform;
            placeHolderTransform.localPosition = Vector3.zero;
            placeHolderTransform.localRotation = isLocal ? Quaternion.identity : Quaternion.LookRotation(Vector3.back);

            placeHolder.SetCard(card);
            cardPlaceHolders.Add(placeHolder);
            UpdateLayout();
            placeHolder.MoveCardToPosition(UnoGame.DECK_TO_HAND_SPEED);
        }

        public void RemovePlaceHolder(Card card)
        {
            card.transform.parent = GameManager.Instance.CardPoolGroup;

            // Find the placeholder that owns this card
            var placeHolderIndex = FindPlaceHolderIndex(card);
            var placeHolder = cardPlaceHolders[placeHolderIndex];

            cardPlaceHolders.RemoveAt(placeHolderIndex);

            GameManager.Instance.PlaceHolderPrefabGroup.Despawn(placeHolder.gameObject.GetInstanceID());
            UpdateLayout();
            UpdateCardPositions();
        }

        public void SwitchPlaceHolderCard(Card card, Card toCard)
        {
            var toIndex = FindPlaceHolderIndex(toCard) + 1;
            var fromIndex = FindPlaceHolderIndex(card);
            var placeHolder = cardPlaceHolders[fromIndex];

            cardPlaceHolders.Insert(toIndex, placeHolder);
            if (fromIndex > toIndex) fromIndex++;
            cardPlaceHolders.RemoveAt(fromIndex);
            
            UpdateLayout();
            UpdateCardPositions();
        }

        private void UpdateLayout()
        {
            const float CARD_WIDTH = 1.4f;

            int num_cards = cardPlaceHolders.Count;
            float hand_width = Mathf.Min(num_cards * CARD_WIDTH, maxHandWidth * CARD_WIDTH);
            Vector3 centerOffset = new Vector3(-(hand_width / 2) + (CARD_WIDTH / num_cards), 0, 0);
            float spacing = hand_width / num_cards;

            var initialStart = thisTransform.position - thisTransform.right;
            for (int i = 0; i < num_cards; i++)
            {
                cardPlaceHolders[i].SetPosition(i, centerOffset + (Vector3.right * (spacing * i)));
            }
        }

        private int FindPlaceHolderIndex(Card card)
        {
            var length = cardPlaceHolders.Count;
            for (int i = 0; i < length; i++)
            {
                if (cardPlaceHolders[i].Card.GetInstanceID() == card.GetInstanceID())
                    return i;
            }
            throw new System.Exception("Card not found in placeholders");
        }

        private CardPlaceHolder FindPlaceHolder(Card card)
        {
            var length = cardPlaceHolders.Count;
            for (int i = 0; i < length; i++)
            {
                var PH = cardPlaceHolders[i];
                if (PH.Card.GetInstanceID() == card.GetInstanceID())
                    return PH;
            }
            return null;
        }

        #endregion
    }
}
