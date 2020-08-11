using System.Collections.Generic;
using UnityEngine;
using Uno.Game.Cards;
using Uno.RuleSets;
using Photon.Pun;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;
using Uno.Game.Display;
using Uno.Game.Players;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Uno.Game
{
    [System.Serializable]
    public class FlowManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        private const string BOT_NAME = "xanefeathers";

        //+ Serialize

        [SerializeField]
        private CardManager cardManager = new CardManager();

        // privates

        private readonly List<BaseController> players = new List<BaseController>();
        [ShowInInspector]
        [ReadOnly]
        private int NumberOfPlayers { get; set; }

        public BaseController LastPlayer { get; private set; }
        private bool LastPlayerSet { get; set; }

        private Card LastCard { get; set; }

        private bool WaitingForColorSelection { get; set; }

        [ShowInInspector]
        [ReadOnly]
        private PlayDirection playDirection;

        [ShowInInspector]
        [ReadOnly]

        private int turnIndex;

        private void Awake() => cardManager.Awake();

        private void Start()
        {
            var gm = GameManager.Instance;
            gm.ColorSelectorGUI.OnColorSelected += OnColorSelected;
            gm.UnoButtonGUI.OnUnoButton += OnUnoButton;
        }

        /// <summary>
        /// Entry point. Starts the game.
        /// </summary>
        public void StartGame(int numberOfBots)
        {
            var gm = GameManager.Instance;

            var handPrefabID = gm.HandPrefabGroup;
            
            var realPlayers = PhotonNetwork.PlayerList;
            var realPlayerCount = realPlayers.Length;

            NumberOfPlayers = realPlayerCount + numberOfBots;

            float rotationPerPlayer = 360f / (float)NumberOfPlayers;

            var playerHandTransform = gm.PlayerHandTransform;
            var otherHandTransform = gm.HandTransform;
            var anchor = gm.CenterAnchor;

            var localPlayer = PhotonNetwork.LocalPlayer;
            var myNumber = localPlayer.ActorNumber;
            
            // Set order for turns
            for (int i = 0; i < realPlayerCount; i++)
            {
                var player = realPlayers[i];
                var number = player.ActorNumber;

                if (myNumber == number) // Local player
                {
                    turnIndex = i;
                    players.Add(PlayerController.Instance);
                }
                else // Other player
                {
                    players.Add(gm.BaseControllerPool.GetInstance() as BaseController);
                }
            }

            if (numberOfBots > 0)
            {
                for (int i = 0; i < numberOfBots; i++) // Bots
                {
                    players.Add(gm.AIControllerPool.GetInstance() as AIController);
                }
            }

            // Create hands
            for (int i = 0; i < NumberOfPlayers; i++)
            {
                var index = GetNextPlayer(i);
                var player = players[index];
                var hand = gm.HandPrefabGroup.Spawn().GetComponent<Hand>();
                var handTransform = hand.transform;

                if (i == 0) // If local hand
                {
                    handTransform.position = playerHandTransform.position;
                    handTransform.rotation = playerHandTransform.rotation;
                    
                    player.Initialize(this, hand, myNumber, true, localPlayer.NickName);
                }
                else if (index < realPlayerCount) // If real player / not bot
                {
                    handTransform.position = otherHandTransform.position;
                    handTransform.rotation = otherHandTransform.rotation;

                    var multiplayer = realPlayers[index];
                    player.Initialize(this, hand, multiplayer.ActorNumber, false, multiplayer.NickName);
                }
                else // Bot
                {
                    handTransform.position = otherHandTransform.position;
                    handTransform.rotation = otherHandTransform.rotation;

                    player.Initialize(this, hand, -i, false, BOT_NAME);
                }

                anchor.Rotate(new Vector3(0, rotationPerPlayer, 0));
            }
            
            cardManager.StartGame();

            SetUpGame();
        }

        #region PUN
        
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (GameManager.Instance.CurrentGameState == GameManager.GameState.InGame)
            {
                // Switch player with AI
                var actorN = otherPlayer.ActorNumber;
                var index = GetPlayerIndex(actorN);
                var player = players[index];
                var hand = player.Hand;
                var isProtected = player.IsProtected;

                players.RemoveAt(index);
                player.PoolThis();

                var AI = GameManager.Instance.AIControllerPool.GetInstance() as AIController;
                AI.Initialize(this, hand, actorN, false, otherPlayer.NickName + "(Bot)");
                players.Insert(index, AI);

                // Check if it was their turn
                if (players[turnIndex].ActorNumber == actorN)
                {
                    if (WaitingForColorSelection)
                    {
                        AI.PickColor();
                    }
                    else
                    {
                        AI.YourTurn();
                    }
                }
            }
        }
        
        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case MyPhoton.UNO_BUTTON:

                    UnoButton(GetSender(photonEvent.Sender));

                    break;


                case MyPhoton.PLAYER_CARD:

                    var index = (int)(byte)photonEvent.CustomData;
                    var player2 = GetSender(photonEvent.Sender);
                    var card = player2.Hand.OwnedCards[index];

                    PlayCard(player2, card);

                    break;


                case MyPhoton.PICKUP_CARD:

                    PickupCard(GetSender(photonEvent.Sender));

                    break;


                case MyPhoton.COLOR_SELECTED:

                    ColorSelected((CardColor)photonEvent.CustomData);

                    break;


                case MyPhoton.RESET_GAME:

                    ResetGame();

                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BaseController GetSender(int ID)
        {
            if (PhotonNetwork.OfflineMode) ID = 1;
            return GetPlayer(ID);
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BaseController GetPlayer(int ActorNumber)
        {
            for (int i = 0; i < NumberOfPlayers; i++)
            {
                var p = players[i];
                if (p.ActorNumber == ActorNumber)
                {
                    return p;
                }
            }
            return null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetPlayerIndex(int ActorNumber)
        {
            for (int i = 0; i < NumberOfPlayers; i++)
            {
                if (players[i].ActorNumber == ActorNumber)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Restarts the game with same players
        /// </summary>
        public void ResetGame()
        {
            GameManager.Instance.DirectionIndicator.gameObject.SetActive(false);
            GameManager.Instance.ColorSelectorGUI.gameObject.SetActive(false);
            WaitingForColorSelection = false;
            LastPlayerSet = false;

            DisplayManager.Instance.Reset();

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                players[i].Reset();
            }

            cardManager.RestartGame();

            SetUpGame();
        }

        /// <summary>
        /// Resets the game completely.
        /// </summary>
        public void ResetGameFull()
        {
            GameManager.Instance.DirectionIndicator.gameObject.SetActive(false);
            GameManager.Instance.ColorSelectorGUI.gameObject.SetActive(false);

            DisplayManager.Instance.Reset();

            WaitingForColorSelection = false;
            LastPlayerSet = false;

            var handPool = GameManager.Instance.HandPrefabGroup;
            
            for (int i = 0; i < NumberOfPlayers; i++)
            {
                var p = players[i];

                p.Reset();
                handPool.Despawn(p.Hand.gameObject);
                
                if (!p.IsLocal) p.PoolThis();
            }

            players.Clear();

            cardManager.ResetFull();
        }

        /// <summary>
        /// Sets up a new game
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetUpGame()
        {
            playDirection = PlayDirection.Forward;

            turnIndex = UnityEngine.Random.Range(0, NumberOfPlayers);
            
            for (int j = 0; j < UnoGame.NUMBER_CARDS_DEALT; j++)
            {
                for (int i = 0; i < NumberOfPlayers; i++)
                {
                    var player = players[GetNextPlayer(i + 1)];

                    if (TryGiveCardToHand(player)) { continue; } else break;
                }
            }

            SwitchDirection();
            turnIndex = GetNextPlayer();
            SwitchDirection();

            LastCard = Garbage.TopCard;
            CardActions();

            SetPlayerTurn();
        }

        #region Multiplayer Layer

        // Uno Button

        private void OnUnoButton()
        {
            // Protect self
            var myID = PhotonNetwork.LocalPlayer.ActorNumber;
            var localPlayer = GetPlayer(myID);

            var amIAlreadyProtected = localPlayer.Hand.OwnedCards.Count > 2 || localPlayer.IsProtected ? true : false;

            // Attack others
            bool amIAttacking = false;

            var turnActor = players[turnIndex].ActorNumber;
            for (int i = 0; i < NumberOfPlayers; i++)
            {
                var player = players[i];
                var actor = player.ActorNumber;
                if (actor != turnActor && myID != actor && !player.IsProtected && player.Hand.OwnedCards.Count == 1)
                {
                    amIAttacking = true;
                    break;
                }
            }

            if (!amIAlreadyProtected || amIAttacking)
            {
                PhotonNetwork.RaiseEvent(MyPhoton.UNO_BUTTON, null, MyPhoton.EventOptions, SendOptions.SendReliable);
            }
        }

        public void UnoButton(BaseController player)
        {
            // Protect Self
            var cards = player.Hand.OwnedCards;
            if (cards.Count <= 2 && !player.IsProtected)
            {
                player.IsProtected = true;

                if (cards.Count == 1) cards[0].SetUno(true);
            }

            // Attack others
            List<BaseController> vulnerablePlayers = new List<BaseController>();
            var turnActor = players[turnIndex].ActorNumber;
            for (int i = 0; i < NumberOfPlayers; i++)
            {
                var p = players[i];
                if (!p.IsProtected && p.ActorNumber != turnActor && p.Hand.OwnedCards.Count == 1)
                {
                    vulnerablePlayers.Add(p);
                }
            }

            var length = vulnerablePlayers.Count;
            for (int i = 0; i < length; i++)
            {
                var vP = vulnerablePlayers[i];

                vP.IsProtected = true;
                TryGiveCardToHand(vP);
                TryGiveCardToHand(vP);
                TryGiveCardToHand(vP);
            }
        }

        // Color Selection

        private void OnColorSelected(CardColor color)
        {
            if (WaitingForColorSelection)
            {
                if (!LastPlayer.IsLocal) // Bot 
                {
                    ColorSelected(color);
                }
                else
                {
                    PhotonNetwork.RaiseEvent(MyPhoton.COLOR_SELECTED, color, MyPhoton.EventOptions, SendOptions.SendReliable);
                }
            }
        }

        private void ColorSelected(CardColor color)
        {
            if (WaitingForColorSelection)
            {
                WaitingForColorSelection = false;
                LastCard.CardColor = color;
                CardPlayed();
            }
        }

        // Pickup Card

        public bool OnPickupCard(BaseController player)
        {
            if (player.ActorNumber == players[turnIndex].ActorNumber &&
                !WaitingForColorSelection)
            {
                if (!player.IsLocal) // Bot 
                {
                    PickupCard(player);
                }
                else
                {
                    PlayerController.Instance.IsMyTurn = false;
                    PhotonNetwork.RaiseEvent(MyPhoton.PICKUP_CARD, null, MyPhoton.EventOptions, SendOptions.SendReliable);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void PickupCard(BaseController player)
        {
            if (TryGiveCardToHand(player, out Card card, out TakeCardDisplayAction action))
            {
                if (card.CanBePlayed)
                {
                    action.DontWaitForCardToReachHand = true;
                    PlayCard(player, card);
                    return;
                }
            }

            LastPlayerSet = true;
            LastPlayer = player;
            turnIndex = GetNextPlayer();
            SetPlayerTurn();
        }

        // Play Card

        public bool OnPlayCard(BaseController player, Card card)
        {
            if (player.ActorNumber == players[turnIndex].ActorNumber &&
                card.CanBePlayed &&
                !WaitingForColorSelection)
            {
                if (!player.IsLocal) // Bot
                {
                    PlayCard(player, card);
                }
                else
                {
                    PlayerController.Instance.IsMyTurn = false;
                    byte index = (byte)player.Hand.OwnedCards.IndexOf(card);
                    PhotonNetwork.RaiseEvent(MyPhoton.PLAYER_CARD, index, MyPhoton.EventOptions, SendOptions.SendReliable);

                    card.LocalPlayerCard = false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void PlayCard(BaseController player, Card card)
        {
            LastPlayerSet = true;
            LastPlayer = player;
            LastCard = card;

            var type = card.CardType;
            if (type == CardType.PlusFour || type == CardType.Wild)
            {
                WaitingForColorSelection = true;
                players[turnIndex].PickColor();
            }
            else
            {
                CardPlayed();
            }
        }

        #endregion

        #region Game Flow

        /// <summary>
        /// Switches to next turn
        /// </summary>
        private void SetPlayerTurn()
        {
            var controller = players[turnIndex];

            if (LastPlayerSet) (GameManager.Instance.ShowTurnDAPool.GetInstance() as ShowTurnDisplayAction).CreateAction(controller, LastPlayer);
            else (GameManager.Instance.ShowTurnDAPool.GetInstance() as ShowTurnDisplayAction).CreateAction(controller);
        }

        private int GetNextPlayer(int numberOfPlaces = 1)
        {
            switch (playDirection)
            {
                case PlayDirection.Forward:

                    var nextPlayer = turnIndex + numberOfPlaces;

                    while (nextPlayer >= NumberOfPlayers)
                    {
                        nextPlayer -= NumberOfPlayers;
                    }

                    return nextPlayer;

                case PlayDirection.Backward:

                    var nextPlayer2 = turnIndex - numberOfPlaces;

                    while (nextPlayer2 < 0)
                    {
                        nextPlayer2 += NumberOfPlayers;
                    }

                    return nextPlayer2;
            }
            throw new System.Exception("Invalid Play Direciton");
        }

        /// <summary>
        /// Uses cached values
        /// </summary>
        private void CardPlayed()
        {
            var hand = LastPlayer.Hand;

            hand.RemoveCard(LastCard);
            var action = GameManager.Instance.PlayCardDAPool.GetInstance() as PlayCardDisplayAction;
            action.Initialize(hand);
            cardManager.PlayCard(LastCard, action);

            (GameManager.Instance.DirectionColorDAPool.GetInstance() as DirectionColorDisplayAction).CreateAction(LastCard.CardColor);

            CardActions();

            SetPlayerTurn();
        }

        private void CardActions()
        {
            var type = LastCard.CardType;
            var nextPlayer = GetNextPlayer();

            switch (type)
            {
                case CardType.PlusFour:
                    var length4 = 4 * Garbage.NumberOfSameCards;
                    for (int i = 0; i < length4; i++)
                    {
                        TryGiveCardToHand(players[nextPlayer]);
                    }
                    break;

                case CardType.PlusTwo:
                    var length2 = 2 * Garbage.NumberOfSameCards;
                    for (int i = 0; i < length2; i++)
                    {
                        TryGiveCardToHand(players[nextPlayer]);
                    }
                    break;

                case CardType.Reverse:
                    SwitchDirection();
                    (GameManager.Instance.DirectionColorDAPool.GetInstance() as DirectionColorDisplayAction).CreateAction();
                    nextPlayer = GetNextPlayer();
                    break;

                case CardType.Skip:
                    nextPlayer = GetNextPlayer(2);
                    break;
            }

            turnIndex = nextPlayer;
        }

        private bool TryGiveCardToHand(BaseController player, out Card card, out TakeCardDisplayAction action)
        {
            action = GameManager.Instance.TakeCardDAPool.GetInstance() as TakeCardDisplayAction;
            action.Initialize(player);
            if (cardManager.TryDrawCard(out card, action))
            {
                player.GiveCard(card);
                return true;
            }

            return false;
        }
        private bool TryGiveCardToHand(BaseController player)
        {
            var action = GameManager.Instance.TakeCardDAPool.GetInstance() as TakeCardDisplayAction;
            action.Initialize(player);
            if (cardManager.TryDrawCard(out Card card, action))
            {
                player.GiveCard(card);
                return true;
            }

            return false;
        }

        private enum PlayDirection : byte
        {
            Forward,
            Backward
        }

        private void SwitchDirection() => playDirection = playDirection == PlayDirection.Forward ? PlayDirection.Backward : PlayDirection.Forward;

        #endregion



#if UNITY_EDITOR
        private void Update()
        {
            Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
        }
#endif
    }
}