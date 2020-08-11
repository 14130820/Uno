using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Uno.Game.Cards;
using static Uno.Game.LerpMovement;

namespace Uno.Game.Display
{
    public class CardPlaceHolder : MonoBehaviour
    {
        Transform thisTransform;

        private void Awake()
        {
            thisTransform = this.transform;
        }
        
        public void SetPosition(int order, Vector3 position)
        {
            Card.transform.SetParent(null);
            thisTransform.localPosition = position;
            Card.transform.SetParent(thisTransform);

            Card.RenderOrder = order;
        }

        public void MoveCardToPosition(float speed)
        {
            Card.MovementScript.MoveTo(Vector3.zero, Quaternion.identity, Vector3BezierCurve.Default, LerpCurve.Default, speed);
        }
        
        public Card Card { get; private set; }
        public int CardID { get; private set; } = -1;

        /// <summary>
        /// Sets the card to this slot and visually moves it there.
        /// </summary>
        /// <param name="card"></param>
        public void SetCard(Card card)
        {
            CardID = card.GetInstanceID();

            var cardTransform = card.transform;
            cardTransform.SetParent(thisTransform);

            this.Card = card;
        }
    }
}
