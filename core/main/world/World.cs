using Casanova.core.content;
using Casanova.core.main.units;
using Godot;

namespace Casanova.core.main.world
{
    public class World : Node2D
    {
        public static World instance;
        public static SceneTree tree;

        public override void _Ready()
        {
            instance = this;
            tree = GetTree();
            // NetworkManager.CreateUnit(NetworkManager.loc.SERVER, UnitTypes.crimson, 0, new Vector2(0, 0), 35f);
        }

        // unit & body params which couldn't be included earlier
        public Unit AddUnit(Unit unit, float rotation)
        {
            GetNode<Node2D>("Units").AddChild(unit);
            unit.Body.GlobalRotation = rotation;
            
            return unit;
        }
    }
}