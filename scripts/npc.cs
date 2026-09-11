using Godot;

public partial class Npc : CharacterBody2D
{
	[Export] public float Speed = 60f;          // Kecepatan jalan (atur di Inspector kalau mau lebih pelan/lambat)
	[Export] public float StopDistance = 20f;   // Berhenti kalau sudah sedekat ini dari target
	[Export] public float SideOffset = 50f;     // Jarak bersebelahan dari player (50 pixel ke kiri)

	// Ambil gravitasi default dari project settings
	private float _gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
	private CharacterBody2D _player;

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null) return;

		// 1. TERAPKAN GRAVITASI (Biar nggak terbang, nempel tanah)
		if (!IsOnFloor())
		{
			Velocity += new Vector2(0, _gravity * (float)delta);
		}

		// 2. HITUNG TARGET POSISI SAMPING PLAYER
		// Target X = posisi player di kiri (bisa diubah ke kanan dengan + SideOffset)
		Vector2 targetPosition = _player.GlobalPosition + new Vector2(-SideOffset, 0);

		// 3. HITUNG JARAK HORIZONTAL SAJA (Sumbu X)
		float distanceX = targetPosition.X - GlobalPosition.X;

		if (Mathf.Abs(distanceX) > StopDistance)
		{
			// Bergerak horizontal ke arah target, biarkan Velocity.Y mengikuti gravitasi
			Velocity = new Vector2(Mathf.Sign(distanceX) * Speed, Velocity.Y);
		}
		else
		{
			// Berhenti bergerak horizontal, biarkan gravitasi yang menahannya di tanah
			Velocity = new Vector2(0, Velocity.Y);
		}

		MoveAndSlide();
	}
}
