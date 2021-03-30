using Godot;

namespace Casanova.core.types
{
    public class Rules
    {
        public Vars.Gamemode Mode = Vars.Gamemode.Freeplay;

        public Rules()
        {
        }

        public override string ToString()
        {
            return $"Mode: {Mode.ToString()}";
        }
    }
}