using System;
using Unity.Services.Lobbies.Models;

namespace Assets.Classes
{
    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }
    public class PlayerCountEventArgs : EventArgs
    {
        public int originalCount;
        public int newTotalCount;
    }
}

