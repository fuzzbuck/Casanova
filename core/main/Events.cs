using System;
using System.Net;
using Casanova.core.net.types;
using Godot;
using static Casanova.core.main.world.NetworkManager;

namespace Casanova.core.main
{
    public class Events : Node
    {

        /* Events in this region are fired both on the server and on the client
         * Because of this, each delegate must have a parameter "loc" which
         * determines who called this event (server or client)
         */

        # region Shared
        
        public static event Action<loc, short, string, EndPoint> PlayerConnect;
        public static void RaisePlayerConnect(loc loc, short id, string username, EndPoint ip = null) => PlayerConnect?.Invoke(loc, id, username, ip);
        
        public static event Action<loc, Player> PlayerJoin;
        public static void RaisePlayerJoin(loc loc, Player player) => PlayerJoin?.Invoke(loc, player);
        
        public static event Action<loc, Player> PlayerDisconnect;
        public static void RaisePlayerDisconnect(loc loc, Player player) => PlayerDisconnect?.Invoke(loc, player);

        # endregion


        /* Events in this region are only fired on the server */

        # region Server

        # endregion


        /* Events in this region are only fired on the client */

        # region Client
        
        # endregion
    }
}
