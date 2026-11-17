using Godot;
using System.Threading.Tasks;

public partial class exit_tutorial : Area2D
{
	[Export] public string NextScenePath = "res://scenes/go_to_cave/go_to_cave.tscn";
	[Export] public Label HintLabel;
	[Export] public string HintTextLocked = "Selesaikan quest dulu...";
	[Export] public float TransitionDelay = 0.5f;  // jeda sebelum pindah (biar ada feel)

	private bool _playerInside = false;
	private bool _transitioning = false;
	private float _timer = 0f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		if (HintLabel != null)
			HintLabel.Visible = false;

		GD.Print("[ExitZone] Ready");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!body.IsInGroup("Player")) return;
		if (_transitioning) return;

		_playerInside = true;
		_timer = 0f;

		bool allQuestsDone = !quest_manager.Instance.IsActive();
		GD.Print($"[ExitZone] Player entered. Quests active = {quest_manager.Instance.IsActive()}");

		if (!allQuestsDone)
		{
			// Quest belum selesai - tampilkan hint
			if (HintLabel != null)
			{
				HintLabel.Text = HintTextLocked;
				HintLabel.Visible = true;
			}
		}
		else
		{
			// Quest selesai - pindah otomatis setelah delay
			if (HintLabel != null)
				HintLabel.Visible = false;
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (!body.IsInGroup("Player")) return;

		_playerInside = false;
		_timer = 0f;

		if (HintLabel != null)
			HintLabel.Visible = false;

		GD.Print("[ExitZone] Player exited");
	}

	public override void _Process(double delta)
	{
		if (!_playerInside || _transitioning) return;

		bool allQuestsDone = !quest_manager.Instance.IsActive();
		if (!allQuestsDone) return;

		// Auto transition
		_timer += (float)delta;
		if (_timer >= TransitionDelay)
		{
			Transition();
		}
	}

	private void Transition()
	{
		_transitioning = true;
		GD.Print($"[ExitZone] Mulai transisi ke: {NextScenePath}");
		
		// Panggil Autoload ScreenFade menggunakan Instance (Singleton)
		if (screen_fade.Instance != null)
		{
			// Menggunakan _ = untuk menjalankan async Task tanpa perlu await di sini
			_ = screen_fade.Instance.TransitionTo(NextScenePath);
			GD.Print("[ExitZone] ScreenFade.TransitionTo dipanggil.");
		}
		else
		{
			// Fallback jika ScreenFade entah kenapa null
			GD.PrintErr("[ExitZone] ScreenFade tidak ditemukan! Pindah langsung tanpa fade.");
			GetTree().ChangeSceneToFile(NextScenePath);
		}
	}
}
