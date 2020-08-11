using System.Runtime.CompilerServices;
using UnityEngine;
using Uno.Game.Cards;
using static Uno.Game.LerpMovement;
using Rewired;

namespace Uno.Game.Players
{
    public class PlayerController : BaseController
    {
        public static PlayerController Instance = new PlayerController();

        private const float MAX_RAY_DISTANCE = 25;

        public void Awake()
        {
            mainCamera = Camera.main;
        }

        public override void Reset()
        {
            IsMyTurn = false;
            ResetState();

            base.Reset();
        }

        private Camera mainCamera;

        public bool IsMyTurn { get => _isMyTurn;
            set
            {
                _isMyTurn = value;
                
                //ResetState();
                //cachedHitCard = null;
            }
        }
        private State currentState = State.None;
        private enum State : byte
        {
            None,
            MouseOver,
            Selected,
            Dragging
        }

        // Raycast caching
        private readonly RaycastHit[] cachedHits = new RaycastHit[5];
        private int cachedHitCount = 0;
        private Ray cachedRay;

        private Card cachedHitCard;
        private Vector3 cachedHitPoint;

        // Saved vars

        private Transform selectedCardTransform;
        private Vector3 selectedCardOffset;

        // Input

        private bool ButtonDown => ReInput.controllers.Mouse.GetAnyButtonDown();
        private bool ButtonUp => ReInput.controllers.Mouse.GetAnyButtonUp();

        /// <summary>
        /// 1. Check for card hit.
        /// 1.1 Check if my card.
        /// 1. else Check for deckc hit.
        /// 
        /// </summary>
        public void Update()
        {
            const int CARD_LAYER = 1 << 8;
            const int DECK_LAYER = 1 << 9;
            const int PLANE_LAYER = 1 << 10;
            
            const float MAX_DISTANCE_UNTIL_DRAG = 0.5f;

            var actionPos = ReInput.controllers.Mouse.screenPosition;
            
            switch (currentState)
            {
                case State.None:

                    cachedRay = mainCamera.ScreenPointToRay(actionPos);

                    if (PerformRaycast(CARD_LAYER) && HitValidCard())  // If hit valid card
                    {
                        currentState = State.MouseOver;
                    }
                    else if (IsMyTurn && PerformRaycast(DECK_LAYER)) // If hit deck.
                    {
                        HighlightDeck(true);

                        currentState = State.MouseOver;
                    }

                    break;


                case State.MouseOver:

                    cachedRay = mainCamera.ScreenPointToRay(actionPos);

                    if (PerformRaycast(CARD_LAYER) && HitValidCard())  // If hit valid card.
                    {
                        if (ButtonDown)
                        {
                            selectedCardTransform = CachedHitCard.transform;
                            selectedCardOffset = selectedCardTransform.position - cachedHitPoint;

                            currentState = State.Selected;
                        }
                    }
                    else if (IsMyTurn && PerformRaycast(DECK_LAYER)) // If hit deck.
                    {
                        if (ButtonUp)
                        {
                            ResetState();
                            TryPickupCard();
                        }
                    }
                    else // No ray hits.
                    {
                        ResetState();
                    }

                    break;


                case State.Selected: // Pseudo mouse held down

                    if (ButtonUp) // Play card
                    {
                        ResetState();
                        if (IsMyTurn)
                        {
                            TryPlayCard(CachedHitCard);
                        }
                    }
                    else // Check for dragging
                    {
                        cachedRay = mainCamera.ScreenPointToRay(actionPos);
                        PerformRaycast(PLANE_LAYER);

                        var distance = Vector3.Distance(cachedHitPoint, cachedHits[0].point);
                        if (distance > MAX_DISTANCE_UNTIL_DRAG)
                        {
                            selectedCardTransform.position = cachedHitPoint;
                            currentState = State.Dragging;
                        }
                    }

                    break;


                case State.Dragging:

                    if (ButtonUp) // Reset
                    {
                        ResetState();
                    }
                    else // move card around
                    {
                        cachedRay = mainCamera.ScreenPointToRay(actionPos);
                        PerformRaycast(PLANE_LAYER);

                        selectedCardTransform.position = cachedHits[0].point + selectedCardOffset;

                        PerformRaycast(CARD_LAYER);

                        if (GetBottomCard(out Card bottomCard))
                        {
                            Hand.SwitchPlaceHolderCard(CachedHitCard, bottomCard);
                        }
                    }

                    break;
            }

        }

        private void ResetState()
        {
            currentState = State.None;
            HighlightCard(false);
            HighlightDeck(false);
        }

        /// <summary>
        /// Uses cached ray.
        /// Caches hits, hit count,
        /// </summary>
        /// <returns>Returns if ray hit something</returns>
        private bool PerformRaycast(int layersToCheck)
        {
            cachedHitCount = Physics.RaycastNonAlloc(cachedRay, cachedHits, MAX_RAY_DISTANCE, layersToCheck);
            return cachedHitCount != 0;
        }

        /// <summary>
        /// Caches: Top-most ordered card, hit point
        /// </summary>
        /// <returns>If a valid card was hit</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HitValidCard()
        {
            int index = -1;
            int order = -1;
            Card savedCard = null;

            for (int i = 0; i < cachedHitCount; i++)
            {
                var hit = cachedHits[i];

                var card = hit.collider.GetComponent<Card>();

                if (card.LocalPlayerCard)
                {
                    var renderOrder = card.RenderOrder;
                    if (renderOrder > order)
                    {
                        order = renderOrder;
                        index = i;
                        savedCard = card;
                    }
                }
            }

            if (index >= 0) // Got Card
            {
                CachedHitCard = savedCard;
                cachedHitPoint = cachedHits[index].point;
                return true;
            }
            else
            {
                return false;
            }
        }


        public Card CachedHitCard
        {
            get => cachedHitCard;

            set
            {
                if (!selectingCard)
                {
                    cachedHitCard = value;

                    HighlightCard(true);
                }
                else if (cachedHitCard.gameObject.GetInstanceID() != value.gameObject.GetInstanceID()) // if cards are different
                {
                    HighlightCard(false);

                    cachedHitCard = value;

                    HighlightCard(true);
                }
            }
        }

        private static readonly LerpCurve lerp = new LerpCurve(5, LerpCurve.Curve.SmoothStart);
        private bool selectingCard = false;
        private void HighlightCard(bool doHighlight)
        {
            if (doHighlight && !selectingCard) // Do highlight
            {
                CachedHitCard.MovementScript.MoveTo(Vector3.up * 0.4f, Quaternion.identity, Vector3BezierCurve.Default, LerpCurve.Default, 0.01f);
                selectingCard = true;
            }
            else if (!doHighlight && selectingCard) // Stop highlight
            {
                CachedHitCard.MovementScript.MoveTo(Vector3.zero, Quaternion.identity, Vector3BezierCurve.Default, lerp, 0.2f);
                selectingCard = false;
            }
        }

        private bool selectingDeck = false;
        private bool _isMyTurn = false;

        private void HighlightDeck(bool doHighlight)
        {
            if (Deck.HasTopCard)
            {
                if (doHighlight && !selectingDeck) // Do highlight
                {
                    Deck.TopCard.HighLightBack(true);
                    selectingDeck = true;
                }
                else if (!doHighlight && selectingDeck) // Stop highlight
                {
                    Deck.TopCard.HighLightBack(false);
                    selectingDeck = false;
                }
            }
        }

        /// <summary>
        /// Checks for the lowest valid card != the current selected card.
        /// </summary>
        /// <returns>If a valid card was found</returns>
        private bool GetBottomCard(out Card bottomCard)
        {
            var selectedCardOrder = CachedHitCard.RenderOrder;
            int index = -1;
            int order = 1000;
            Card savedCard = null;

            for (int i = 0; i < cachedHitCount; i++)
            {
                var hit = cachedHits[i];
                var card = hit.collider.GetComponent<Card>();

                if (card.LocalPlayerCard)
                {
                    var renderOrder = card.RenderOrder;
                    if (renderOrder != selectedCardOrder && renderOrder < order)
                    {
                        order = renderOrder;
                        index = i;
                        savedCard = card;
                    }
                }
            }

            bottomCard = savedCard;

            return index >= 0 ? true : false;
        }

        // Real time, call by display action

        public override void YourTurn() => IsMyTurn = true;

        public override void PickColor() => GameManager.Instance.ColorSelectorGUI.OpenColorSelector();
    }
}