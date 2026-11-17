using Godot;

public partial class trigger_jatuh : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			var controller = GetTree().GetFirstNodeInGroup("PerbatasanController") as perbatasan_controller;
			if (controller != null)
			{
				controller.PlayerSampaiDiAmari();
			}
			// Matikan trigger biar gak kepanggil 2x
			SetDeferred("monitoring", false);
		}
	}
}
