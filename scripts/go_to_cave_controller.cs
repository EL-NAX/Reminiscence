using Godot;
using System.Threading.Tasks;

public partial class go_to_cave_controller : Node
{
	[Export] public Marker2D JumpPoint;
	[Export] public Marker2D LandingPoint;
	[Export] public Marker2D AmariFinalPoint; 
	
	[Export] public float JumpForce = 450f;
	[Export] public float DelayBeforeWalk = 0.2f;
	[Export] public float DelayBeforeJump = 0.4f;
	
	// TAMBAHAN: Kecepatan AutoWalk Amari (dipercepat biar gak lama)
	[Export] public float AmariAutoWalkSpeed = 300f; 

	private amari_ai _amari;
	private bool _sequenceStarted = false;

	// UBAH DI SINI: Tambahkan async agar bisa await FadeIn
	public override async void _Ready()
	{
		var mapTitle = GetNodeOrNull<map_title>("/root/MapTitle");
		if (mapTitle != null)
		{
			mapTitle.ShowTitle("Cave Entry"); 
		}

		AddToGroup("GoToCaveController");
		CallDeferred(nameof(InitAmari));
		GD.Print("[GoToCaveController] Ready");

		// =======================================================
		// TAMBAHAN: Fade In Layar Hitam
		// =======================================================
		if (screen_fade.Instance != null)
		{
			// Tunggu sebentar agar judul map muncul dulu (opsional, biar lebih mulus)
			await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
			
			await screen_fade.Instance.FadeIn(0.5f);
			GD.Print("[GoToCaveController] Layar berhasil di-FadeIn!");
		}
		// =======================================================
	}

	private void InitAmari()
	{
		_amari = GetTree().GetFirstNodeInGroup("Amari") as amari_ai;
		if (_amari == null)
		{
			GD.PrintErr("[GoToCaveController] Amari gak ketemu!");
			return;
		}
		_amari.SetIdle();
		GD.Print("[GoToCaveController] Amari IDLE di awal");
	}

	public async void StartAmariJumpSequence()
	{
		if (_sequenceStarted) return;
		_sequenceStarted = true;
		
		GD.Print("[GoToCaveController] Sequence mulai...");

		if (_amari == null) return;
		if (JumpPoint == null || LandingPoint == null || AmariFinalPoint == null)
		{
			GD.PrintErr("[GoToCaveController] JumpPoint/LandingPoint/FinalPoint belum di-assign!");
			return;
		}

		// STEP 1: Delay, lalu WALK ke JumpPoint (pakai speed cepat)
		await ToSignal(GetTree().CreateTimer(DelayBeforeWalk), SceneTreeTimer.SignalName.Timeout);

		GD.Print($"[GoToCaveController] Amari WALK ke X={JumpPoint.GlobalPosition.X}");
		_amari.WalkToX(JumpPoint.GlobalPosition.X, AmariAutoWalkSpeed);

		while (!_amari.IsWalkingDone)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		}
		GD.Print("[GoToCaveController] Amari sampai di JumpPoint");

		// STEP 2: Delay, lalu JUMP
		await ToSignal(GetTree().CreateTimer(DelayBeforeJump), SceneTreeTimer.SignalName.Timeout);

		GD.Print("[GoToCaveController] Amari JUMP ke LandingPoint");
		_amari.JumpToPosition(LandingPoint.GlobalPosition, JumpForce);

		while (_amari.IsJumping)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		}
		GD.Print("[GoToCaveController] Amari mendarat!");

		// STEP 3: AUTOWALK KE FINAL POINT (PAKAI SPEED CEPAT)
		GD.Print($"[GoToCaveController] Amari AUTOWALK ke gua (X={AmariFinalPoint.GlobalPosition.X})");
		_amari.WalkToPosition(AmariFinalPoint.GlobalPosition, AmariAutoWalkSpeed);

		while (!_amari.IsWalkingDone)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		}

		// =======================================================
		// UBAH DI SINI: JANGAN FOLLOW, TAPI DIEM (IDLE)
		// =======================================================
		_amari.SetIdle();
		GD.Print("[GoToCaveController] Amari berhenti di depan gua (IDLE)");
	}
}
