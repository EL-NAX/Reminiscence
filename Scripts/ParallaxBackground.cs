using Godot;
using System;

public partial class ParallaxBackground : Godot.ParallaxBackground
{
	[Export] public float Speed = 15.0f;

	public override void _Process(double delta)
	{
		Vector2 newOffset = ScrollOffset;
		newOffset.X += Speed * (float)delta;
		ScrollOffset = newOffset;
	}
}
