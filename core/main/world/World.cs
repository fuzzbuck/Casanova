using Casanova.core.content;
using Casanova.core.main.units;
using Casanova.core.types;
using Godot;

namespace Casanova.core.main.world
{
    public class World : Node2D
    {
        public static World instance;
        public static Rules rules;
        public static SceneTree tree;

        public override void _Ready()
        {
            if (rules == null)
            {
                rules = new Rules();
            }
            
            instance = this;
            tree = GetTree();
            
            /* Initialize PlayerController */
            PlayerController.Init();
        }

        public void SetRules(Rules newRules)
        {
            rules = newRules;
        }
        
        public Unit AddUnit(Unit unit, float rotation)
        {
            GetNode<Node2D>("Units").AddChild(unit);
            unit.Body.GlobalRotation = rotation;
            
            return unit;
        }
    }
}