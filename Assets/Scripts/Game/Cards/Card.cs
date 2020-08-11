using FeatherWorks.Pooling;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Uno.Game.Cards
{
    public class Card : MonoBehaviour
    {
        [SerializeField]
        private CardShaderController frontShader = null;
        [SerializeField]
        private CardShaderController backShader = null;
        
        private void Awake()
        {
            MovementScript = GetComponent<LerpMovement>();
            Collider = GetComponent<BoxCollider>();
        }

        public CardData cardData { get; private set; }

        private Collider Collider;
        private CardColor _cardColor;
        private bool localPlayerCard;

        public LerpMovement MovementScript { get; private set; }
        
        public bool CanBePlayed
        {
            get
            {
                var otherData = Garbage.TopCard;
                var otherColor = otherData.CardColor;
                var otherType = otherData.CardType;

                var color = CardColor;
                var type = CardType;

                if (type == CardType.Wild ||
                    type == CardType.PlusFour ||
                    type == otherType ||

                    otherColor == CardColor.Any ||
                    color == otherColor)
                    return true;

                return false;
            }
        }
        
        public CardType CardType => cardData.Type;
        public CardColor CardColor
        {
            get => _cardColor;
            set
            {
                _cardColor = value;

                frontShader.SetEdgeColor(value);
            }
        }

        /// <summary>
        /// For local player controlling
        /// </summary>
        public bool LocalPlayerCard
        {
            get => localPlayerCard;
            set
            {
                if (value && !localPlayerCard)
                {
                    Collider.enabled = true;
                    localPlayerCard = true;
                }
                else if (!value && localPlayerCard)
                {
                    Collider.enabled = false;
                    localPlayerCard = false;
                }
            }
        }


        /// <summary>
        /// Initialization
        /// Sets the base card type and shader values to render it.
        /// </summary>
        public void SetCard(CardData card)
        {
            cardData = card;
            _cardColor = card.Color;

            frontShader.SetCard(card);
        }

        public void SetUno(bool on)
        {
            backShader.SetUno(on);
            frontShader.SetUno(on);
        }

        public void HighLightBack(bool highLight)
        {
            if (highLight) backShader.Highlight(); else backShader.UnHighlight();
        }

        public void ResetParent() => this.transform.parent = GameManager.Instance.CardPoolGroup;

        public void Resetcard()
        {
            // Reset Wild if wild
            if (CardType == CardType.PlusFour || CardType == CardType.Wild)
            {
                CardColor = CardColor.Any;
            }

            backShader.UnHighlight();
            SetUno(false);

            LocalPlayerCard = false;
            SetDefaultCard();
        }

        #region Card Sort/Order Rendering

        public int RenderOrder
        {
            set
            {
                frontShader.RenderOrder = value;
                backShader.RenderOrder = value;
            }
            get
            {
                return backShader.RenderOrder;
            }
        }
        
        /// <summary>
        /// Keeps card sorting layer in front
        /// </summary>
        public void SetLocalPlayerCard() => SetShadersID(GameManager.Instance.PlayerLayerID);
        /// <summary>
        /// deck/garbage sorting layer
        /// </summary>
        public void SetDefaultCard() => SetShadersID(GameManager.Instance.DefaultLayerID);
        /// <summary>
        /// Keeps card above deck/garbage but under local player
        /// </summary>
        public void SetNonLocalHandCard() => SetShadersID(GameManager.Instance.HandLayerID);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetShadersID(int ID)
        {
            frontShader.RenderLayer = ID;
            backShader.RenderLayer = ID;
        }

        #endregion
    }
}