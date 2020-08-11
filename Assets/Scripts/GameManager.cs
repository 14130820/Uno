using UnityEngine;
using Uno.Game.Cards;
using Uno.RuleSets;
using Uno.Game;
using Photon.Pun;
using FeatherWorks.Pooling;
using Sirenix.OdinInspector;
using Uno.GUI;
using TMPro;
using ObjectPooling;
using Uno.Game.Display;
using Uno.Game.Players;

namespace Uno
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField]
        private GUIManager guiManager = null;
        [SerializeField]
        private FlowManager flowManager = null;

        [SerializeField]
        [Header("Prefabs")]
        [AssetsOnly]
        private GameObject cardPrefab = null;
        [SerializeField]
        [AssetsOnly]
        private GameObject handPrefab = null;
        [SerializeField]
        [AssetsOnly]
        private GameObject placeHolderPrefab = null;

        [SerializeField]
        [Header("Controls")]
        private Transform playerHandTransform = null;
        [SerializeField]
        private Transform centerAnchor = null;
        [SerializeField]
        private Transform handTransform = null;
        [SerializeField]
        private SpriteRenderer directionIndicator = null;
        [SerializeField]
        private Transform cardPoolGroup = null;

        // Feather Pools
        public FeatherPool CardPrefabGroup { get; private set; }
        public FeatherPool HandPrefabGroup { get; private set; }
        public FeatherPool PlaceHolderPrefabGroup { get; private set; }

        // Object Pools
            // Display Actions
        public ObjectPool CreateDeckDAPool { get; private set; }
        public ObjectPool DirectionColorDAPool { get; private set; }
        public ObjectPool PlayCardDAPool { get; private set; }
        public ObjectPool TakeCardDAPool { get; private set; }
        public ObjectPool ShowTurnDAPool { get; private set; }
        public ObjectPool ShuffleGarbageDAPool { get; private set; }
        public ObjectPool PauseDAPool { get; private set; }
            // Controllers
        public ObjectPool AIControllerPool { get; private set; }
        public ObjectPool BaseControllerPool { get; private set; }

        // Sprite Sorting Layer IDs
        public int DefaultLayerID { get; private set; }
        public int PlayerLayerID { get; private set; }
        public int HandLayerID { get; private set; }

        // Transforms
        public Transform PlayerHandTransform => playerHandTransform;
        public Transform HandTransform => handTransform;
        public Transform CenterAnchor => centerAnchor;
        public Transform CardPoolGroup => cardPoolGroup;
        public SpriteRenderer DirectionIndicator => directionIndicator;

        // Game State

        public GameState CurrentGameState { get; private set; } = GameState.InLobby;

        public enum GameState
        {
            InLobby,
            InGame
        }

        // GUI Access

        public ColorSelectorGUI ColorSelectorGUI => guiManager.ColorSelectorGUI;
        public UnoButtonGUI UnoButtonGUI => guiManager.UnoButtonGUI;
        public bool IsMenuOpen => guiManager.IsMenuOpen;

        public TextMeshProUGUI GetNameTextComponent() => guiManager.GetNameTextComponent();
        public void SetWinner(string winnerName) => guiManager.SetGameWinner(winnerName);

        private void Awake()
        {
            Instance = this;

            directionPropertyBlock = new MaterialPropertyBlock();

            // Get feather pools
            var FP = FeatherPoolManager.Instance;
            CardPrefabGroup = FP.GetPool(cardPrefab.GetInstanceID());
            HandPrefabGroup = FP.GetPool(handPrefab.GetInstanceID());
            PlaceHolderPrefabGroup = FP.GetPool(placeHolderPrefab.GetInstanceID());

            // Create and get object pools.
            CreateDeckDAPool = ObjectPoolManager.GetPool<CreateDeckDisplayAction>(1);
            ShuffleGarbageDAPool = ObjectPoolManager.GetPool<ShuffleGarbageDisplayAction>(1);
            DirectionColorDAPool = ObjectPoolManager.GetPool<DirectionColorDisplayAction>(15);
            PlayCardDAPool = ObjectPoolManager.GetPool<PlayCardDisplayAction>(15);
            ShowTurnDAPool = ObjectPoolManager.GetPool<ShowTurnDisplayAction>(15);
            PauseDAPool = ObjectPoolManager.GetPool<PauseDisplayAction>(15);
            TakeCardDAPool = ObjectPoolManager.GetPool<TakeCardDisplayAction>(55);
            BaseControllerPool = ObjectPoolManager.GetPool<BaseController>(7);
            AIControllerPool = ObjectPoolManager.GetPool<AIController>(6);
            
            // Get layer ID's
            DefaultLayerID = SortingLayer.NameToID("Default");
            HandLayerID = SortingLayer.NameToID("HandCard");
            PlayerLayerID = SortingLayer.NameToID("PlayerCard");
        }

        public void StartGame(int numberOfBots)
        {
            flowManager.StartGame(numberOfBots);

            CurrentGameState = GameState.InGame;
        }

        public void ResetGame() => flowManager.ResetGame();

        public void EndGame()
        {
            flowManager.ResetGameFull();

            CurrentGameState = GameState.InLobby;
        }

        #region Turn Indicator

        private CardColor currentCardColor;
        private MaterialPropertyBlock directionPropertyBlock;

        public void SetDirectionColor(CardColor color)
        {
            if (currentCardColor != color)
            {
                directionIndicator.GetPropertyBlock(directionPropertyBlock);

                directionPropertyBlock.SetColor("_FillColor_Color_1", color.GetColor()); // Sides

                directionIndicator.SetPropertyBlock(directionPropertyBlock);

                currentCardColor = color;
            }
        }

        public void ChangeDirection() => directionIndicator.flipX = !directionIndicator.flipX;

        #endregion

        #region Physics Update

        private bool updatePhysics = false;

        /// <summary>
        /// Flags for the physics to be updated next Fixed Update.
        /// </summary>
        public void UpdatePhysics() => updatePhysics = true;

        private void FixedUpdate()
        {
            if (updatePhysics)
            {
                Physics.Simulate(Time.deltaTime);
                updatePhysics = false;
            }
        }

        #endregion
    }
}