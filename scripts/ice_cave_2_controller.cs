using Godot;

public partial class ice_cave_2_controller : Node
{
	[Export] public string[] AmariCallDialogue = {
		"Amari: Krieger! Kesini!",
		"Amari: Aku menemukan pedang es raksasa!",
		"Krieger: ...Pedang?",
        "Amari: Coba tarik! Mungkin kamu bisa."
	};

	[Export] public string[] SwordDialogue = {
		"Krieger: Pedang ini... sangat berat.",
        "Amari: Tarik dengan sekuat tenaga!"
	};

	[Export] public string[] EndingDialogue = {
		"Krieger: Akhirnya... terlepas juga.",
        "Amari: Hebat! Ayo keluar dari sini."
	};

	[Export] public AudioStreamPlayer SFXGedubrak; 

	private player _player;
	private amari_ai _amari;
	private dialogue_controller _dialogue;
	private sword_qte _qte;
	private Camera2D _playerCamera;
	
	private ColorRect _autoBlackScreen;

	private bool _qteStarted = false;
	private float _currentQTEProgress = 0f;

	private Label _hintLabel;
	private Tween _hintBlinkTween;

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

		AddToGroup("IceCave2Controller");

		_player = GetTree().GetFirstNodeInGroup("Player") as player;
		_amari = GetTree().GetFirstNodeInGroup("Amari") as amari_ai;
		_dialogue = GetTree().GetFirstNodeInGroup("DialogUI") as dialogue_controller;
		_playerCamera = _player?.GetNodeOrNull<Camera2D>("Camera2D");

		// Bikin layar hitam otomatis
		var canvasLayer = new CanvasLayer();
		canvasLayer.Layer = 100; 
		AddChild(canvasLayer);

		_autoBlackScreen = new ColorRect();
		_autoBlackScreen.Color = new Color(0, 0, 0, 0); 
		_autoBlackScreen.SetAnchorsPreset(Control.LayoutPreset.FullRect); 
		_autoBlackScreen.MouseFilter = Control.MouseFilterEnum.Ignore; 
		_autoBlackScreen.Visible = false;
		canvasLayer.AddChild(_autoBlackScreen);

		CreateHintUI();

		if (_player != null) _player.IsMovementLocked = true;
		if (_amari != null)
		{
			_amari.FollowPlayer = false;
			_amari.AutoWalkMode = false;
			_amari.SetIdle();
		}

		if (screen_fade.Instance != null)
		{
			screen_fade.Instance.SetAlpha(1.0f);
			await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
			await screen_fade.Instance.FadeIn(1.2f);
		}

		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		StartCutscene();
	}

	private void CreateHintUI()
	{
		var hintLayer = new CanvasLayer();
		hintLayer.Layer = 90; 
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

	private async void TampilkanHint(string text)
	{
		if (_hintLabel == null) return;

		_hintLabel.Text = text;
		GD.Print($"[IceCave2] Nampilin hint: {text}");
		
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

	public override void _Process(double delta)
	{
		if (_qteStarted && _playerCamera != null)
		{
			float shakeIntensity = (_currentQTEProgress / 100f) * 15f; 
			_playerCamera.Offset = new Vector2(
				GD.Randf() * shakeIntensity - (shakeIntensity / 2), 
				GD.Randf() * shakeIntensity - (shakeIntensity / 2)
			);
		}
	}

	private async void StartCutscene()
	{
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

		if (_dialogue != null)
			await _dialogue.ShowDialogue(AmariCallDialogue);

		TampilkanHint("Tarik pedang yang tertancap!");

		if (_player != null) _player.IsMovementLocked = false;
	}

	public async void StartSwordQTESequence()
	{
		if (_qteStarted) return;
		_qteStarted = true;
		_currentQTEProgress = 0f;

		if (_player != null) _player.IsMovementLocked = true;

		if (_dialogue != null)
			await _dialogue.ShowDialogue(SwordDialogue);

		if (_player != null)
		{
			var anim = _player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (anim != null) anim.Play("pullSword");
		}

		if (_qte == null) _qte = GetTree().GetFirstNodeInGroup("SwordQTE") as sword_qte;
		
		if (_qte != null)
		{
			_qte.QTEFinished -= OnQTEFinished;
			_qte.QTEFinished += OnQTEFinished;
			_qte.QTEProgress -= OnQTEProgress; 
			_qte.QTEProgress += OnQTEProgress;
			_qte.StartQTE();
		}
	}

	private void OnQTEProgress(float progress)
	{
		_currentQTEProgress = progress;
	}

	private async void OnQTEFinished(bool success)
	{
		_qteStarted = false;
		if (_playerCamera != null) _playerCamera.Offset = Vector2.Zero; 

		if (!success)
		{
			var sword = GetTree().GetFirstNodeInGroup("IceSword") as interactable_item;
			if (sword != null) 
			{
				sword._used = false;
				var label = sword.GetNodeOrNull<Label>("Label");
				if (label != null) label.Visible = true;
			}

			if (_player != null) 
			{
				_player.IsMovementLocked = false;
				var anim = _player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
				if (anim != null) anim.Play("idle");
			}
			return;
		}

		// QTE BERHASIL
		var swordToRemove = GetTree().GetFirstNodeInGroup("IceSword") as interactable_item;
		if (swordToRemove != null) swordToRemove.QueueFree();

		if (_player != null)
		{
			_player.IsMovementLocked = true; 
			_player.IsFalling = true; 

			var anim = _player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (anim != null) anim.Play("fall"); 
		}

		// Getar kamera
		if (_playerCamera != null)
		{
			var tween = CreateTween();
			for (int i = 0; i < 10; i++)
				tween.TweenProperty(_playerCamera, "offset",
					new Vector2(GD.Randf() * 20 - 10, GD.Randf() * 20 - 10), 0.05f);
			tween.TweenProperty(_playerCamera, "offset", Vector2.Zero, 0.1f);
		}

		await ToSignal(GetTree().CreateTimer(1.2f), "timeout");

		if (SFXGedubrak != null) 
			SFXGedubrak.Play();

		if (_autoBlackScreen != null) 
		{
			_autoBlackScreen.Visible = true;
			var fadeTween = CreateTween();
			fadeTween.TweenProperty(_autoBlackScreen, "color:a", 1.0f, 0.2f); 
		}

		await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

		// UBAH STATUS PLAYER SAAT LAYAR MASIH HITAM PEKAT
		if (_player != null)
		{
			_player.IsFalling = false;
			_player.IsDraggingSword = false; 
			
			var anim = _player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (anim != null) anim.Play("idleWithSword");
		}

		if (_autoBlackScreen != null) 
		{
			var fadeOutTween = CreateTween();
			fadeOutTween.TweenProperty(_autoBlackScreen, "color:a", 0.0f, 0.5f);
			await ToSignal(fadeOutTween, "finished");
			_autoBlackScreen.Visible = false;
		}

		if (game_state.Instance != null)
			game_state.Instance.IceSwordPulled = true;

		// MUNCULIN DIALOG ENDING
		if (_dialogue != null)
			await _dialogue.ShowDialogue(EndingDialogue);

		// =========================================================
		// JEDA SEBENTAR SEBELUM MUNCULIN HINT
		// =========================================================
		await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
		
		TampilkanHint("Pergi ke ujung gua >");
		// =========================================================

		// SETELAH DIALOG SELESAI, PLAYER BEBAS JALAN
		if (_player != null)
		{
			_player.IsDraggingSword = true; 
			_player.IsMovementLocked = false; 
		}

		if (game_state.Instance != null)
			game_state.Instance.Chapter1Done = true;

		if (_amari != null) _amari.FollowPlayer = true;
	}
}
