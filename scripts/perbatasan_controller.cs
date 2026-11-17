using Godot;
using System.Linq;
using System.Threading.Tasks;

public partial class perbatasan_controller : Node
{
	[Export] private player _player;
	[Export] private amari_ai _amari;
	[Export] private dialogue_controller _dialogue;

	[Export] private Marker2D _markerAmariStop;
	[Export] private Marker2D _markerPlayerFollow; 
	[Export] private Marker2D _markerJatuh;
	[Export] private Marker2D _markerPlayerDekatAmari; 
	[Export] private Marker2D _markerExit;

	[Export] private Camera2D _cutsceneCamera;
	[Export] private Node2D _npcKsatria; 

	[Export] public float WalkSpeed = 100f; 
	[Export] public float DurasiPanKamera = 2.0f;      // lama panning ke CutsceneCamera
	[Export] public float DurasiPanBalik = 1.5f;       // lama panning balik ke kamera player
	[Export] public float TinggiLetterbox = 100f;      // tebal bar hitam atas & bawah

	[Export] public string[] DialogueAdegan3 = {
		"Amari: Tunggu... Krieger, kamu dengar itu?",
		"Krieger: Suara adu pedang? Datangnya dari ujung lorong ini...",
		"Amari: Ayo kita lihat! Siapa tahu ada yang butuh bantuan!",
		"Krieger: Amari! Jangan gegabah—"
	};

	[Export] public string[] DialoguePerbatasan = {
		"Krieger: T-tunggu... Ini kan... Garis Perbatasan Human Empire dan Dark Territory?!"
	};

	[Export] public string[] DialogueAdegan4 = {
		"Amari: Krieger... lihat di depan sana! Ada orang tergeletak!",
		"Krieger: Itu... zirah Ksatria Kerajaan?!",
		"Amari: Dia... dia tertancap pedang! Lukanya parah sekali, kita harus menolongnya!",
		"Krieger: AMARI, JANGAN! BERHENTI! JANGAN MENDEKATI GARIS PERBATASAN!"
	};

	[Export] public string[] DialogueJatuh = {
		"Krieger: AMARI!!"
	};

	[Export] public string[] DialogueNanya = {
		"Krieger: Amari! Kamu tidak apa-apa?!"
	};

	[Export] public string[] DialogueAdegan5_Part2 = {
		"Amari: A-aku... aku cuma tersandung...",
		"Krieger: Sudahlah, tempat ini sangat berbahaya! Ksatria itu sudah tidak bisa ditolong. Ayo kita langsung pulang sekarang!"
	};

	private bool _sudahMulai = false;
	private CanvasLayer _letterboxLayer;
	private ColorRect _topBar;
	private ColorRect _bottomBar;
	private AnimatedSprite2D _npcAnim;
	private Camera2D _playerCam;

	public override async void _Ready()
	{
	// =======================================================
	// MUNCULIN JUDUL MAP DI SINI
	// =======================================================
	var mapTitle = GetNodeOrNull<map_title>("/root/MapTitle");
	if (mapTitle != null)
	{
		mapTitle.ShowTitle("Perbatasan"); // <-- Ganti teksnya
	}

		AddToGroup("PerbatasanController");
		GD.Print("[Perbatasan] _Ready() mulai.");

		if (_player == null) _player = GetTree().GetFirstNodeInGroup("Player") as player;
		if (_amari == null) _amari = GetTree().GetFirstNodeInGroup("Amari") as amari_ai;
		if (_dialogue == null) _dialogue = GetTree().GetFirstNodeInGroup("DialogUI") as dialogue_controller;

		if (_player != null) 
		{
			_player.IsMovementLocked = true;
			_player.IsDraggingSword = true; 
			
			var pAnim = _player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (pAnim != null) pAnim.Play("idleWithSword");

			_playerCam = _player.GetNodeOrNull<Camera2D>("Camera2D");
		}

		if (_amari != null) 
		{
			_amari.FollowPlayer = false;
			_amari.SetIdle();
		}

		if (_npcKsatria != null)
		{
			_npcAnim = _npcKsatria.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (_npcAnim != null)
			{
				_npcAnim.SpriteFrames.SetAnimationLoop("fall", false);
				_npcAnim.SpriteFrames.SetAnimationLoop("default", false);
				
				if (_npcAnim.SpriteFrames.HasAnimation("default"))
					_npcAnim.Play("default");
				_npcAnim.Stop();
				_npcAnim.Frame = 0; 
			}
		}

		BikinLetterbox();

		if (screen_fade.Instance != null)
		{
			screen_fade.Instance.SetAlpha(1.0f);
			await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
			await screen_fade.Instance.FadeIn(1.0f);
		}

		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		
		if (_dialogue != null)
			await _dialogue.ShowDialogue(DialogueAdegan3);

		GD.Print("[Perbatasan] Mulai AutoWalk Amari & Player.");
		if (_player != null) 
			_player.IsMovementLocked = false;

		if (_amari != null && _markerAmariStop != null)
			_amari.WalkToX(_markerAmariStop.GlobalPosition.X, WalkSpeed);

		if (_player != null && _markerPlayerFollow != null)
			_player.AutoWalkToX(_markerPlayerFollow.GlobalPosition.X, WalkSpeed); 

		await ToSignal(GetTree().CreateTimer(1.0f), "timeout"); 

		// ===== PANNING SMOOTH KE CUTSCENE CAMERA + LETTERBOX MASUK BARENGAN =====
		TampilkanLetterbox(DurasiPanKamera);
		await PanKeCutsceneCamera(DurasiPanKamera);

		if (_npcAnim != null)
		{
			GD.Print("[Perbatasan] NPC Ksatria Jatuh!");
			if (_npcAnim.SpriteFrames.HasAnimation("fall"))
			{
				_npcAnim.Play("fall");
				await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
				_npcAnim.Stop();
				_npcAnim.Frame = _npcAnim.SpriteFrames.GetFrameCount("fall") - 1;
			}
		}

		while ((_amari != null && !_amari.IsWalkingDone) || (_player != null && _player.IsAutoWalking))
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		if (_amari != null) _amari.SetIdle();
		if (_player != null) _player.IsMovementLocked = true;

		// Kamera TETAP di CutsceneCamera (tidak dibalikin di sini),
		// dialog Perbatasan sekarang digabung ke PlayerSampaiDiAmari().
		PlayerSampaiDiAmari(); 
	}

	// ======================== LETTERBOX ========================
	private void BikinLetterbox()
	{
		_letterboxLayer = new CanvasLayer();
		_letterboxLayer.Layer = 10; // kalau menutupi dialog box, kecilkan; kalau ketutup UI lain, besarkan
		AddChild(_letterboxLayer);

		_topBar = new ColorRect();
		_topBar.Color = Colors.Black;
		_topBar.MouseFilter = Control.MouseFilterEnum.Ignore;
		_letterboxLayer.AddChild(_topBar);
		_topBar.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide);
		_topBar.OffsetBottom = 0; // mulai dari tinggi 0

		_bottomBar = new ColorRect();
		_bottomBar.Color = Colors.Black;
		_bottomBar.MouseFilter = Control.MouseFilterEnum.Ignore;
		_letterboxLayer.AddChild(_bottomBar);
		_bottomBar.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
		_bottomBar.OffsetTop = 0; // mulai dari tinggi 0
	}

	private Tween TampilkanLetterbox(float durasi)
	{
		var t = CreateTween().SetParallel(true);
		t.TweenProperty(_topBar, "offset_bottom", TinggiLetterbox, durasi)
		 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		t.TweenProperty(_bottomBar, "offset_top", -TinggiLetterbox, durasi)
		 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		return t;
	}

	private Tween SembunyikanLetterbox(float durasi)
	{
		var t = CreateTween().SetParallel(true);
		t.TweenProperty(_topBar, "offset_bottom", 0f, durasi)
		 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		t.TweenProperty(_bottomBar, "offset_top", 0f, durasi)
		 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		return t;
	}

	// ======================== KAMERA ========================
	private async Task PanKeCutsceneCamera(float durasi)
	{
		if (_cutsceneCamera == null) return;

		// Simpan posisi & zoom tujuan (sesuai posisi CutsceneCamera di scene)
		Vector2 targetPos = _cutsceneCamera.GlobalPosition;
		Vector2 targetZoom = _cutsceneCamera.Zoom;

		// Mulai dari posisi kamera player supaya tidak ada lompatan
		if (_playerCam != null)
		{
			_cutsceneCamera.GlobalPosition = _playerCam.GetScreenCenterPosition();
			_cutsceneCamera.Zoom = _playerCam.Zoom;
		}

		_cutsceneCamera.PositionSmoothingEnabled = false;
		_cutsceneCamera.Enabled = true;
		_cutsceneCamera.MakeCurrent();

		var tween = CreateTween().SetParallel(true);
		tween.TweenProperty(_cutsceneCamera, "global_position", targetPos, durasi)
			 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		// zoom ikut disamakan ke tujuan (kalau zoom-nya sama dengan kamera player, tidak terlihat)
		tween.TweenProperty(_cutsceneCamera, "zoom", targetZoom, durasi)
			 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private async Task PanBalikKePlayer(float durasi)
	{
		if (_cutsceneCamera == null || _playerCam == null) return;

		var tween = CreateTween().SetParallel(true);
		tween.TweenProperty(_cutsceneCamera, "global_position", _playerCam.GetScreenCenterPosition(), durasi)
			 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(_cutsceneCamera, "zoom", _playerCam.Zoom, durasi)
			 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

		await ToSignal(tween, Tween.SignalName.Finished);

		// Serahkan kamera ke player lagi
		_cutsceneCamera.Enabled = false;
		_playerCam.Enabled = true;
		_playerCam.MakeCurrent();
	}

	// ======================== CUTSCENE UTAMA ========================
	public async void PlayerSampaiDiAmari()
	{
		if (_sudahMulai) return;
		_sudahMulai = true;

		if (_player != null) 
		{
			_player.StopAutoWalk(); 
			_player.IsMovementLocked = true;
			var pAnim = _player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (pAnim != null) pAnim.Play("idleWithSword"); 
		}

		// DIALOG DIGABUNG: "T-tunggu... Garis Perbatasan" + Adegan 4 (tanpa pindah kamera di tengah)
		if (_dialogue != null)
			await _dialogue.ShowDialogue(DialoguePerbatasan.Concat(DialogueAdegan4).ToArray());

		if (_amari != null && _markerJatuh != null)
		{
			_amari.WalkToX(_markerJatuh.GlobalPosition.X, WalkSpeed);
			while (!_amari.IsWalkingDone)
				await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		}

		if (_amari != null)
		{
			_amari.IsInCutscene = true; 
			var amariAnim = _amari.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (amariAnim != null)
			{
				amariAnim.Play("fall");
				amariAnim.Offset = new Vector2(0, 10); 

				await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
				
				amariAnim.Stop();
				int totalFrame = amariAnim.SpriteFrames.GetFrameCount("fall");
				amariAnim.Frame = totalFrame - 1;
			}
		}

		if (_dialogue != null)
			await _dialogue.ShowDialogue(DialogueJatuh);

		if (_player != null && _markerPlayerDekatAmari != null)
		{
			_player.IsMovementLocked = false;
			_player.AutoWalkToX(_markerPlayerDekatAmari.GlobalPosition.X, WalkSpeed);

			while (_player.IsAutoWalking)
				await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

			_player.IsMovementLocked = true;
			
			var pAnim = _player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (pAnim != null) pAnim.Play("idleWithSword");
		}

		// Amari bangun (dibantu Krieger) sebelum dialog digabung
		if (_amari != null)
		{
			var amariAnim = _amari.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (amariAnim != null) 
			{
				amariAnim.Play("idle");
				amariAnim.Offset = new Vector2(0, 0); 
			}
			_amari.IsInCutscene = false;
		}

		// DIALOG DIGABUNG: "Amari! Kamu tidak apa-apa?!" + "A-aku cuma tersandung..." dst.
		if (_dialogue != null)
			await _dialogue.ShowDialogue(DialogueNanya.Concat(DialogueAdegan5_Part2).ToArray());

		SembunyikanLetterbox(DurasiPanBalik);
		await PanBalikKePlayer(DurasiPanBalik);

		// ================= JALAN PULANG BARENGAN =================
		if (_amari != null && _player != null && _markerExit != null)
		{
			_player.IsMovementLocked = false;
			
			var pAnim = _player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (pAnim != null)
			{
				if (_player.IsDraggingSword)
					pAnim.Play("nyeretPedang");
				else
					pAnim.Play("walk");
			}

			_amari.WalkToX(_markerExit.GlobalPosition.X, WalkSpeed);
			_player.AutoWalkToX(_markerExit.GlobalPosition.X, WalkSpeed);

			while ((_amari != null && !_amari.IsWalkingDone) || (_player != null && _player.IsAutoWalking))
				await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

			_player.IsMovementLocked = true;
			if (pAnim != null) pAnim.Play("idleWithSword");
		}

		// ===== SETELAH SAMPAI MarkerExit: KAMERA BALIK KE SEMULA + LETTERBOX HILANG =====
		

		if (screen_fade.Instance != null)
		{
			await screen_fade.Instance.FadeOut(1.0f);
		}

		GetTree().ChangeSceneToFile("res://scenes/desa/desa.tscn"); 
	}
}
