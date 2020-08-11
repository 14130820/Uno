using Uno.Game.Cards;
using UnityEngine;
using TMPro;
using ObjectPooling;

namespace Uno.Game.Players
{
    public class BaseController : ObjectPoolItem
    {
        protected FlowManager FlowManager { get; set; }

        public int ActorNumber { get; private set; }
        public bool IsLocal { get; private set; }
        public Hand Hand { get; private set; }
        public string Name { get; private set; }
        public TextMeshProUGUI NameComponent { get; private set; }

        public void Initialize(FlowManager flowManager, Hand hand, int actorNumber, bool isLocal, string name)
        {
            FlowManager = flowManager;
            Hand = hand;
            ActorNumber = actorNumber;
            IsLocal = isLocal;
            Name = name;

            Hand.IsLocal = isLocal;

            if (!isLocal) SetUpName();
        }

        public bool IsProtected { get; set; } = false;

        public void GiveCard(Card card) => Hand.OwnedCards.Add(card);

        public virtual void YourTurn() { }

        public virtual void PickColor() { }

        public virtual void Reset()
        {
            Hand.Reset();
            IsProtected = false;
            if (!IsLocal) NameComponent.color = CardColor.Green.GetColor();
        }

        protected bool TryPickupCard() => FlowManager.OnPickupCard(this);

        protected bool TryPlayCard(Card card)
        {
            if (Hand.OwnedCards.Count != 0)
            {
                if (FlowManager.OnPlayCard(this, card))
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector3 nameDistanceFromHand = new Vector3(0, -2.6f, 0);

        private void SetUpName()
        {
            NameComponent = GameManager.Instance.GetNameTextComponent();

            var handT = Hand.transform;

            var pos = handT.position + handT.TransformDirection(nameDistanceFromHand);
            
            // This basically moves all the names up (forward in world space) by a scale of 0 to 1 from the farthest name from the camera, to the closest name to the camera.
            pos -= new Vector3(0, 0, (pos.z + 5.75f) / 11.5f  - 1f);
            
            // Set alignment
            if (pos.z < 5.5f) // Is top
            {
                if (pos.x > 0) // On right side
                {
                    NameComponent.alignment = TextAlignmentOptions.Left;
                }
                else // On left side
                {
                    NameComponent.alignment = TextAlignmentOptions.Right;
                }
            }

            NameComponent.transform.position = Camera.main.WorldToScreenPoint(pos);
            NameComponent.text = Name;
        }
    }
}