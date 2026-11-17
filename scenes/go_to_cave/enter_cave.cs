using Godot;

public partial class enter_cave : Area2D
{
	[Export] public string TargetScenePath = "res://scenes/cave/cave.tscn";
	[Export] public bool Triggered = false;
	[Export] public string AmariGroupName = "Amari";
	[Export] public float FadeDuration = 0.4f;

	// Hint
	[Export] public Label HintLabel;
	[Export] public string HintTextLocked = "Amari belum nyusul!";
	[Export] public string HintTextReady = "Amari siap! Ayo masuk goa...";
	[Export] public float HintDuration = 2.0f;

	private amari_ai _amari;
	private bool _hintShowing = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		_amari = GetTree().GetFirstNodeInGroup(AmariGroupName) as amari_ai;
		if (_amari == null)
			GD.PrintErr("[EnterCave] Amari gak ketemu di group!");

		if (HintLabel != null)
			HintLabel.Visible = false;

		GD.Print($"[EnterCave] Ready. Target={TargetScenePath}");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (Triggered) return;
		if (!body.IsInGroup("Player")) return;

		// =======================================================
		// UBAH DI SINI: HAPUS TANDA SERU (!)
		// =======================================================
		bool amariReady = _amari != null && _amari.IsWalkingDone; 
		// Artinya: Kalau Amari udah selesai AutoWalk (lagi diem), berarti udah siap masuk.

		if (!amariReady)
		{
			GD.Print("[EnterCave] Amari belum sampai di depan gua!");
			ShowHint(HintTextLocked);
			return;
		}

		Triggered = true;
		GD.Print($"[EnterCave] Amari udah siap. Player masuk goa!");
		ShowHint(HintTextReady);

		CallDeferred(nameof(FadeAndChangeScene));
	}

	private void OnBodyExited(Node2D body)
	{
		if (!body.IsInGroup("Player")) return;

		if (HintLabel != null)
			HintLabel.Visible = false;
	}

	private async void FadeAndChangeScene()
	{
		// Fade out dulu
		if (screen_fade.Instance != null)
		{
			GD.Print("[EnterCave] Fade out...");
			screen_fade.Instance.FadeOut(FadeDuration);
			await ToSignal(GetTree().CreateTimer(FadeDuration), SceneTreeTimer.SignalName.Timeout);
		}

		// Baru pindah scene
		GD.Print($"[EnterCave] Pindah ke: {TargetScenePath}");
		GetTree().ChangeSceneToFile(TargetScenePath);
	}

	private async void ShowHint(string text)
	{
		if (HintLabel == null) return;
		if (_hintShowing && HintLabel.Text == text) return;

		_hintShowing = true;
		HintLabel.Text = text;
		HintLabel.Visible = true;

		await ToSignal(GetTree().CreateTimer(HintDuration), SceneTreeTimer.SignalName.Timeout);

		if (HintLabel != null)
			HintLabel.Visible = false;

		_hintShowing = false;
	}
}
