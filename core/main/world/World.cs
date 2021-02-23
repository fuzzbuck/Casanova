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
            
            /* Initialize PlayerController */
            PlayerController.Init();
        }
        
        public Unit AddUnit(Unit unit, float rotation)
        {
            GetNode<Node2D>("Units").AddChild(unit);
            unit.Body.GlobalRotation = rotation;
            
            return unit;
        }
    }
}