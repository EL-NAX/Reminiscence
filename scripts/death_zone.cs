using Godot;

public partial class death_zone : Area2D
{
	[Export] public Marker2D RespawnPoint;
	[Export] public float FadeDuration = 0.3f;
	[Export] public float HoldBlackDuration = 0.2f;
	[Export] public float FallDamage = 100f; // Damage saat jatuh (default 100 biar langsung mati)

	private bool _respawning = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;

		if (RespawnPoint == null)
			GD.PrintErr("[DeathZone] RespawnPoint belum di-assign!");
	}

	private async void OnBodyEntered(Node2D body)
	{
		if (!body.IsInGroup("Player")) return;
		if (_respawning) return;

		_respawning = true;
		GD.Print("[DeathZone] Player jatuh! Respawn...");

		// =========================================================
		// TAMBAHAN BARU: KURANGIN DARAH PLAYER
		// =========================================================
		if (body is player p)
		{
			p.TakeDamage(FallDamage);
		}
		// =========================================================

		// Freeze player
		if (body is CharacterBody2D cb)
			cb.Velocity = Vector2.Zero;

		// 1. Fade ke hitam
		screen_fade.Instance.FadeOut(FadeDuration);
		await ToSignal(GetTree().CreateTimer(FadeDuration), "timeout");

		// 2. Teleport player ke RespawnPoint
		if (RespawnPoint != null)
		{
			body.GlobalPosition = RespawnPoint.GlobalPosition;
			GD.Print($"[DeathZone] Respawn di {RespawnPoint.GlobalPosition}");
		}

		if (body is CharacterBody2D cb2)
			cb2.Velocity = Vector2.Zero;

		// 3. Diam sebentar di hitam
		await ToSignal(GetTree().CreateTimer(HoldBlackDuration), "timeout");

		// 4. Fade kembali ke terang
		screen_fade.Instance.FadeIn(FadeDuration);
		await ToSignal(GetTree().CreateTimer(FadeDuration), "timeout");

		_respawning = false;
	}
}
