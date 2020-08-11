using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun
{
    /// <summary>
    /// Helper class for Photon Network.
    /// </summary>
    public static class MyPhoton
    {
        // Room vars
        public const string NUMBER_OF_PLAYERS = "a";
        public const string NUMBER_OF_BOTS = "b";
        public const string RANDOM_SEED = "c";

        // Player vars
        public const string PLAYER_READY = "0a";

        // Events
        public const byte PLAYER_CARD = 0;
        public const byte PICKUP_CARD = 1;
        public const byte COLOR_SELECTED = 2;
        public const byte UNO_BUTTON = 3;
        public const byte RESET_GAME = 4;
        public const byte START_GAME = 5;
        public const byte RESTART_GAME = 6;
        public const byte BACK_TO_ROOM = 7;

        // Saved Data
        private static readonly RaiseEventOptions eventOptions = new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.All
        };

        public static RaiseEventOptions EventOptions => eventOptions;

        /// <summary>
        /// Custom Method for setting up a room.
        /// </summary>
        public static bool CreateRoom(string roomName, bool isPrivate, int maxPlayers, int numberOfBots)
        {
            var _maxPlayers = (byte)maxPlayers;
            var _numberOfBots = (byte)numberOfBots;

            // Set room options
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = _maxPlayers,
                IsVisible = !isPrivate
            };

            // Create Properties
            var maxValue = int.MaxValue;
            int seed = Random.Range(-maxValue, maxValue);

            var props = new Hashtable() {
                { MyPhoton.RANDOM_SEED, seed },
                { MyPhoton.NUMBER_OF_PLAYERS, _maxPlayers },
                { MyPhoton.NUMBER_OF_BOTS, _numberOfBots }
            };

            roomOptions.CustomRoomProperties = props;

            return PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
    }
}
