using System;
using Godot;

namespace Casanova.core.main.units
{
	public abstract class PlayerUnit : Unit
	{
		private Tag _tagNode;
		public string Tag
		{
			set => _tagNode.UpdateTag(value);
			get => _tagNode.TagFakeLabel.Text;
		}
		public override void _Ready()
		{
			base._Ready();
			_tagNode = GetNode<Tag>("Tag");
		}
	}
}
