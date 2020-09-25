using Casanova.core.main.units;

namespace Casanova.core.net.types
{
    public class Player
    {
        public readonly int Id;
        public readonly bool IsLocal;
        public readonly string Username;
        public PlayerUnit PlayerUnit;

        public Player(int id, string username, PlayerUnit playerUnit, bool isLocal = false)
        {
            Id = id;
            Username = username;
            PlayerUnit = playerUnit;
            IsLocal = isLocal;
        }
    }
}