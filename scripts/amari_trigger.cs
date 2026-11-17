using Godot;

public partial class amari_trigger : Area2D
{
	private bool _triggered = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		GD.Print("[AmariTrigger] Ready");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_triggered) return;
		if (!body.IsInGroup("Player")) return;

		_triggered = true;
		GD.Print("[AmariTrigger] Player nyebrang! Panggil Controller...");

		// Cari Controller di scene
		var controller = GetTree().GetFirstNodeInGroup("GoToCaveController") as go_to_cave_controller;
		if (controller != null)
		{
			controller.StartAmariJumpSequence();
		}
		else
		{
			GD.PrintErr("[AmariTrigger] GoToCaveController gak ketemu!");
		}
	}
}
