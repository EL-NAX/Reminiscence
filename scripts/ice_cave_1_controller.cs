using Godot;
using System.Threading.Tasks;

public partial class ice_cave_1_controller : Node
{
	[Export] public int TotalIce = 9;
	[Export] public Marker2D PlayerExitMarker;
	[Export] public Marker2D AmariExitMarker;
	[Export] public float AmariSpeed = 140f;

	[Export] public string[] IntroDialogue = {
		"Amari: Kita harus melewati gua ini.",
		"Krieger: Hati-hati, banyak es di sekitar.",
		"Amari: Aku jalan duluan. Ambil serpihan es-nya ya.",
        "Krieger: Baik, hati-hati."
	};

	[Export] public string[] TransitionDialogue = {
		"Krieger: Amari... tunggu aku!",
        "Krieger: Aku harus segera menyusulnya."
	};

	private player _player;
	private amari_ai _amari;
	private dialogue_controller _dialogue;
	private int _collected = 0;
	private bool _amariExited = false;
	private bool _playerAtExit = false;
	private bool _transitionStarted = false;

	private Label _notifLabel;
	private CanvasLayer _notifLayer;

	private Label _hintLabel;
	private Tween _hintBlinkTween;
	private bool _sudahTampilHint = false;

	public override async void _Ready()
	{
	// =======================================================
	// MUNCULIN JUDUL MAP DI SINI
	// =======================================================
	var mapTitle = GetNodeOrNull<map_title>("/root/MapTitle");
	if (mapTitle != null)
	{
		mapTitle.ShowTitle("Ice Cave"); // <-- Ganti teksnya
	}

		AddToGroup("IceCave1Controller");

		_player = GetTree().GetFirstNodeInGroup("Player") as player;
		_amari = GetTree().GetFirstNodeInGroup("Amari") as amari_ai;
		_dialogue = GetTree().GetFirstNodeInGroup("DialogUI") as dialogue_controller;

		if (PlayerExitMarker != null)
			GD.Print($"[IceCave1] PlayerExitMarker: {PlayerExitMarker.GlobalPosition}");
		if (AmariExitMarker != null)
			GD.Print($"[IceCave1] AmariExitMarker: {AmariExitMarker.GlobalPosition}");

		CreateNotifUI();
		CreateHintUI();

		if (_player != null) _player.IsMovementLocked = true;
		if (_amari != null) _amari.FollowPlayer = false;

		if (screen_fade.Instance != null)
		{
			screen_fade.Instance.SetAlpha(1.0f);
			await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
			await screen_fade.Instance.FadeIn(1.2f);
		}

		await ToSignal(GetTree().CreateTimer(0.4f), "timeout");
		GetTree().CreateTimer(0.5f).Timeout += StartIntro;
	}

	private void CreateNotifUI()
	{
		_notifLayer = new CanvasLayer();
		_notifLayer.Layer = 10;
		AddChild(_notifLayer);

		_notifLabel = new Label();
		_notifLabel.Text = "";
		_notifLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_notifLabel.VerticalAlignment = VerticalAlignment.Center;
		_notifLabel.AddThemeFontSizeOverride("font_size", 24);
		_notifLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.95f, 1f));
		_notifLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f));
		_notifLabel.AddThemeConstantOverride("shadow_offset_x", 2);
		_notifLabel.AddThemeConstantOverride("shadow_offset_y", 2);

		_notifLabel.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
		_notifLabel.Position = new Vector2(0, 100);
		_notifLabel.Size = new Vector2(800, 60);
		_notifLabel.OffsetLeft = -400;
		_notifLabel.OffsetRight = 400;

		_notifLabel.Modulate = new Color(1, 1, 1, 0);
		_notifLabel.Visible = false;

		_notifLayer.AddChild(_notifLabel);
	}

	private void CreateHintUI()
	{
		var hintLayer = new CanvasLayer();
		hintLayer.Layer = 100;
		AddChild(hintLayer);

		_hintLabel = new Label();
		_hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_hintLabel.VerticalAlignment = VerticalAlignment.Center;
		_hintLabel.AddThemeFontSizeOverride("font_size", 36);
		_hintLabel.AddThemeColorOverride("font_color", Colors.White);
		_hintLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f));
		_hintLabel.AddThemeConstantOverride("shadow_offset_x", 4);
		_hintLabel.AddThemeConstantOverride("shadow_offset_y", 4);

		_hintLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
		_hintLabel.Size = new Vector2(800, 100);
		_hintLabel.OffsetLeft = -400;
		_hintLabel.OffsetRight = 400;
		_hintLabel.OffsetTop = -50;
		_hintLabel.OffsetBottom = 50;

		_hintLabel.Modulate = new Color(1, 1, 1, 0);
		_hintLabel.Visible = false;

		hintLayer.AddChild(_hintLabel);
	}

	// ===== FUNGSI BARU UNTUK NAMPILIN HINT (BISA GANTI TEKS) =====
	private async void TampilkanHint(string text, bool kunciPlayerPindah = false)
	{
		if (_hintLabel == null) return;
		
		if (kunciPlayerPindah)
			_sudahTampilHint = true;

		_hintLabel.Text = text;
		GD.Print($"[IceCave1] Nampilin hint: {text}");
		
		_hintLabel.Visible = true;
		_hintLabel.Modulate = new Color(1, 1, 1, 1);

		if (_hintBlinkTween != null && _hintBlinkTween.IsValid()) _hintBlinkTween.Kill();
		_hintBlinkTween = CreateTween();
		_hintBlinkTween.SetLoops();
		_hintBlinkTween.TweenProperty(_hintLabel, "modulate:a", 0.2f, 0.4f);
		_hintBlinkTween.TweenProperty(_hintLabel, "modulate:a", 1.0f, 0.4f);

		await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout);

		if (_hintBlinkTween != null && _hintBlinkTween.IsValid()) _hintBlinkTween.Kill();
		
		var fadeOutTween = CreateTween();
		fadeOutTween.TweenProperty(_hintLabel, "modulate:a", 0.0f, 0.5f);
		await ToSignal(fadeOutTween, Tween.SignalName.Finished);

		_hintLabel.Visible = false;
	}

	private void CekDanTampilkanPetunjuk()
	{
		if (_sudahTampilHint) return;

		if (_collected >= TotalIce && _amariExited)
		{
			TampilkanHint("Susul Amari ke ujung gua! >", true); 
		}
	}

	private async void StartIntro()
	{
		if (_dialogue != null)
			await _dialogue.ShowDialogue(IntroDialogue);

		// =======================================================
		// MUNCULIN HINT PERTAMA SETELAH DIALOG INTRO SELESAI
		// =======================================================
		TampilkanHint("Kumpulkan semua es! ❄", false); 
		// =======================================================

		OnIntroComplete();
	}

	private void OnIntroComplete()
	{
		if (_player != null)
		{
			_player.IsMovementLocked = false;
			_player.UnlockJump();
			GD.Print("[IceCave1] Jump unlocked");
		}

		if (_amari != null)
		{
			Vector2 target;
			if (AmariExitMarker != null)
				target = AmariExitMarker.GlobalPosition;
			else
				target = new Vector2(3200, _amari.GlobalPosition.Y);

			_amari.AutoWalkMode = true;
			_amari.AutoWalkTarget = target;
			_amari.AutoWalkSpeed = AmariSpeed;
			GD.Print($"[IceCave1] Amari target: {target}, speed: {AmariSpeed}");
		}

		var quest = GD.Load<quest_data>("res://data/quests/ch1_collect_ice.tres");
		if (quest != null)
		{
			var arr = new Godot.Collections.Array<quest_data>();
			arr.Add(quest);
			quest_manager.Instance.StartQuestSequence(arr);
		}
	}

	public void OnIceCollected()
	{
		_collected++;
		GD.Print($"[IceCave1] Ice: {_collected}/{TotalIce}");

		if (_collected >= TotalIce)
		{
			GD.Print("[IceCave1] Semua es terkumpul!");
			CekDanTampilkanPetunjuk(); 
		}
	}

	public async void OnAmariReachedEnd()
	{
		if (_amariExited) return;
		_amariExited = true;

		GD.Print("[IceCave1] Amari keluar map");

		if (game_state.Instance != null)
			game_state.Instance.AmariWentAhead = true;

		if (_amari != null)
			_amari.FadeOutAndExit();

		await ToSignal(GetTree().CreateTimer(0.8f), "timeout");

		CekDanTampilkanPetunjuk(); 
	}

	private async void ShowNotification(string text)
	{
		if (_notifLabel == null) return;

		_notifLabel.Text = text;
		_notifLabel.Visible = true;

		var tween = CreateTween();
		tween.TweenProperty(_notifLabel, "modulate:a", 1.0f, 0.5f);
		await ToSignal(tween, "finished");

		await ToSignal(GetTree().CreateTimer(4.0f), "timeout");

		var tween2 = CreateTween();
		tween2.TweenProperty(_notifLabel, "modulate:a", 0.0f, 0.5f);
		await ToSignal(tween2, "finished");

		_notifLabel.Visible = false;
	}

	public override void _Process(double delta)
	{
		if (_playerAtExit || _transitionStarted) return;
		if (_player == null) return;
		if (PlayerExitMarker == null) return;

		float jarakKeMarker = _player.GlobalPosition.DistanceTo(PlayerExitMarker.GlobalPosition);

		if (!_sudahTampilHint)
		{
			if (jarakKeMarker < 30f)
			{
				if (_notifLabel != null && !_notifLabel.Visible)
					ShowNotification("Kumpulkan es dan tunggu Amari dulu!");
			}
			return; 
		}

		if (jarakKeMarker < 30f)
		{
			bool amariDone = _amariExited;
			bool questDone = _collected >= TotalIce;

			if (!amariDone)
			{
				if (_notifLabel != null && !_notifLabel.Visible)
					ShowNotification("Amari belum sampai. Tunggu dulu...");
				return;
			}

			if (!questDone)
			{
				if (_notifLabel != null && !_notifLabel.Visible)
					ShowNotification($"Kumpulkan semua es dulu! ({_collected}/{TotalIce})");
				return;
			}

			GD.Print("[IceCave1] Player nyampe marker + syarat terpenuhi!");
			_playerAtExit = true;
			_ = StartTransitionToMap2();
		}
	}

	private async System.Threading.Tasks.Task StartTransitionToMap2()
	{
		if (_transitionStarted) return;
		_transitionStarted = true;

		GD.Print("[IceCave1] Transition ke Map 2 (langsung, tanpa delay)");

		if (_player != null) _player.IsMovementLocked = true;

		if (_dialogue != null)
			await _dialogue.ShowDialogue(TransitionDialogue, true);

		if (game_state.Instance != null)
			game_state.Instance.TotalIceCollected = _collected;

		if (screen_fade.Instance != null)
		{
			await screen_fade.Instance.FadeOut(0.5f);
		}

		GetTree().ChangeSceneToFile("res://scenes/ice_cave/ice_cave_2.tscn");
	}
}
