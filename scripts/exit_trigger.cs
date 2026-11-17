using Godot;

public partial class exit_trigger : Area2D
{
	// Tambahkan variabel ini biar bisa diatur di Inspector
	[Export] public string TargetScenePath = ""; 
	
	private bool _hasTriggered = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private async void OnBodyEntered(Node2D body)
	{
		if (_hasTriggered) return;
		if (!body.IsInGroup("Player")) return;

		// Cek apakah path-nya udah diisi di Inspector
		if (string.IsNullOrEmpty(TargetScenePath))
		{
			GD.PrintErr("❌ ExitTrigger: TargetScenePath belum diisi di Inspector!");
			return;
		}

		_hasTriggered = true;

		var player = body as player;
		if (player != null) player.IsMovementLocked = true;

		if (screen_fade.Instance != null)
		{
			await screen_fade.Instance.FadeOut(1.0f);
		}

		// Pindah ke scene sesuai yang diatur di Inspector
		GetTree().ChangeSceneToFile(TargetScenePath);
	}
}
