using Godot;

namespace Casanova.core.types.bodies
{
	public class Shadow : Sprite
	{
		private Node2D Parent;
		private Sprite Sprite;
		public float Heigth = 15f;
		public float Blur = 0f;

		public override void _Ready()
		{
			Parent = GetParent<Node2D>();
			Sprite = Parent.GetNode<Sprite>("Sprite");
			
			Texture = Sprite.Texture;

			if(Heigth <= 0f)
				QueueFree();
			
			// todo: handle Blur
		}

		public override void _Process(float delta)
		{
			GlobalPosition = Parent.GlobalPosition + new Vector2(-15, 15);
		}
	}
}
