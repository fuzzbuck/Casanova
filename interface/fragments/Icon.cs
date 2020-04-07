using Godot;
using System;

public class Icon : TextureRect
{
  public override void _Process(float delta)
  {
	  if ((int) delta % 2 < 1)
	  {
		  SetFlipH(!FlipH);
	  }
  }
}
