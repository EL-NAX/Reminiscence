using Godot;
using System.Threading.Tasks;

public partial class cave_cutscene : Node2D
{
	[Export] private player _player;
	[Export] private amari_ai _amari;
	[Export] private Camera2D _camera;
	[Export] private PointLight2D _magicLeaf;
	[Export] private CanvasModulate _darkness;
	[Export] private NinePatchRect _dialoguePanel;
	[Export] private Label _dialogueLabel;
	[Export] private Control _continueLabel; 
	
	[Export] private Control _subjectBox;
	[Export] private Label _subjectLabel;
	
	[Export] private ColorRect _blackScreen;
	[Export] private cave_path_selection _pathSelection;

	[Export] private Marker2D _amariCheckpoint;
	[Export] private Marker2D _playerCheckpoint;

	[Export] private float _startDelay = 1.5f;
	[Export] private float _lightFadeDuration = 1.5f;
	[Export] private float _amariWalkSpeed = 70f;
	[Export] private float _playerWalkSpeed = 70f;

	// TAMBAHAN UNTUK SFX
	[Export] public AudioStreamPlayer SfxTyping;
	[Export] public BlipVoice AmariVoice;
	[Export] public BlipVoice KriegerVoice;

	private Vector2 _originalZoom;
	private Tween _blinkTween;

	private readonly string[] _dialoguesPart1 = new string[]
	{
		"Amari: Gelap sekali di dalam gua ini...",
		"Krieger: Hati-hati. Kita harus tetap bersama.",
		"Amari: Tenang... aku punya daun ajaib ini. Tunggu sebentar..."
	};

	private readonly string[] _dialoguesPart2 = new string[]
	{
		"Krieger: (Melihat sekitar) Ini cukup untuk menerangi jalan kita.",
		"Amari: Ayo, kita jalan pelan-pelan. Aku duluan.",
        "Krieger: Oke, aku di belakangmu."
	};

	private void StartBlinking()
	{
		if (_continueLabel == null) return;
		_continueLabel.Visible = true;
		_continueLabel.Modulate = new Color(1, 1, 1, 1);
		if (_blinkTween != null && _blinkTween.IsValid()) _blinkTween.Kill();
		_blinkTween = CreateTween();
		_blinkTween.SetLoops();
		_blinkTween.TweenProperty(_continueLabel, "modulate:a", 0.2f, 0.5f);
		_blinkTween.TweenProperty(_continueLabel, "modulate:a", 1.0f, 0.5f);
	}

	private void StopBlinking()
	{
		if (_continueLabel == null) return;
		_continueLabel.Visible = false;
		if (_blinkTween != null && _blinkTween.IsValid()) _blinkTween.Kill();
	}

	public override async void _Ready()
	{
		var mapTitle = GetNodeOrNull<map_title>("/root/MapTitle");
		if (mapTitle != null)
		{
			mapTitle.ShowTitle("Cave");
		}

		GD.Print("=== [CaveCutscene] _Ready() START ===");

		RenderingServer.SetDefaultClearColor(Colors.Black);

		if (screen_fade.Instance != null)
			screen_fade.Instance.FadeIn(0.3f);

		bool skip = game_state.Instance != null && game_state.Instance.CaveCutsceneDone;
		GD.Print($"[Cutscene] Flag CaveCutsceneDone = {skip}");

		if (_camera == null && _player != null)
			_camera = _player.GetNodeOrNull<Camera2D>("Camera2D");

		if (_camera != null && _player != null)
		{
			_originalZoom = _camera.Zoom; 
			_camera.GlobalPosition = _player.GlobalPosition;
			_camera.ResetSmoothing();
			_camera.ForceUpdateScroll();
			_camera.Zoom = _originalZoom * 1.35f; 
		}

		StopBlinking();

		if (skip)
		{
			GD.Print("[Cutscene] SKIP - flag udah true");

			if (_magicLeaf != null)
			{
				_magicLeaf.Enabled = true;
				_magicLeaf.Energy = 1.5f;
			}

			if (_blackScreen != null)
				_blackScreen.Visible = false;

			if (_dialoguePanel != null)
				_dialoguePanel.Hide();
			
			StopBlinking();

			if (_amari != null)
				_amari.StartFollowing();

			if (_camera != null)
				_camera.Zoom = _originalZoom;

			if (_pathSelection != null)
			{
				if (screen_fade.Instance != null)
				{
					screen_fade.Instance.FadeOut(0.5f);
					await ToSignal(GetTree().CreateTimer(0.6f), SceneTreeTimer.SignalName.Timeout);
				}

				_pathSelection.ShowSelection();
				if (_player != null) _player.IsMovementLocked = true;

				if (screen_fade.Instance != null)
				{
					await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);
					screen_fade.Instance.FadeIn(0.6f);
				}
			}
			else if (_player != null)
			{
				_player.IsMovementLocked = false;
			}

			return;
		}

		if (_dialoguePanel == null || _blackScreen == null || _dialogueLabel == null ||
			_player == null || _amari == null || _magicLeaf == null ||
			_amariCheckpoint == null || _playerCheckpoint == null)
		{
			GD.PrintErr("⚠️ ISI SEMUA SLOT INSPECTOR DI CAVE CUTSCENE!");
			return;
		}
		GD.Print("[Cutscene] Validasi OK");

		if (_camera == null)
		{
			GD.PrintErr("[Cutscene] Camera gak ketemu!");
			return;
		}

		_dialoguePanel.Hide();
		_blackScreen.Show();
		_blackScreen.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		_blackScreen.Color = new Color(0, 0, 0, 1.0f);

		_magicLeaf.Enabled = true; 
		_magicLeaf.Energy = 0.3f;  
		
		if (_darkness != null)
			_darkness.Color = new Color(0f, 0f, 0f, 1f); 

		_player.IsMovementLocked = true;
		_amari.SetIdle();

		Tween fadeInTween = CreateTween();
		fadeInTween.TweenProperty(_blackScreen, "color:a", 0.0f, 1.0f)
				   .SetTrans(Tween.TransitionType.Cubic)
				   .SetEase(Tween.EaseType.InOut);
		await ToSignal(fadeInTween, Tween.SignalName.Finished);

		await RunCaveSequence();
	}

	private async Task RunCaveSequence()
	{
		await ToSignal(GetTree().CreateTimer(_startDelay), SceneTreeTimer.SignalName.Timeout);

		var amariSprite = _amari?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") 
						  ?? _amari?.GetNodeOrNull<AnimatedSprite2D>("Sprite2D");

		_dialoguePanel.Show();
		StopBlinking();

		// ==========================================
		// DIALOG PART 1
		// ==========================================
		for (int i = 0; i < _dialoguesPart1.Length; i++)
		{
			string speaker = "";
			string text = _dialoguesPart1[i];

			if (_dialoguesPart1[i].Contains(":"))
			{
				var parts = _dialoguesPart1[i].Split(new[] { ':' }, 2);
				speaker = parts[0].Trim();
				text = parts[1].Trim();
			}

			if (_subjectLabel != null) _subjectLabel.Text = speaker;
			if (_subjectBox != null) _subjectBox.Visible = !string.IsNullOrEmpty(speaker);

			BlipVoice currentVoice = AmariVoice;
			if (!string.IsNullOrEmpty(speaker))
			{
				string speakerLower = speaker.ToLower();
				if (speakerLower.Contains("amari")) currentVoice = AmariVoice;
				else if (speakerLower.Contains("krieger")) currentVoice = KriegerVoice;
			}

			_dialogueLabel.Text = text;
			_dialogueLabel.VisibleRatio = 0.0f;

			StopBlinking();

			float duration = text.Length * 0.03f; 

			Tween textTween = CreateTween();
			textTween.TweenProperty(_dialogueLabel, "visible_ratio", 1.0f, duration);

			bool isTyping = true;
			float sfxTimer = 0f;
			int lastBlipIndex = 0;
			
			textTween.Finished += () => 
			{
				isTyping = false;
				StartBlinking();
				if (SfxTyping != null) SfxTyping.Stop();
			};

			while (true)
			{
				if (isTyping && SfxTyping != null && currentVoice != null)
				{
					float visibleRatio = _dialogueLabel.VisibleRatio;
					int currentVisibleChars = (int)(visibleRatio * text.Length);

					if (currentVisibleChars > lastBlipIndex)
					{
						lastBlipIndex = currentVisibleChars;
						sfxTimer += (float)GetProcessDeltaTime();

						if (sfxTimer >= 0.06f) 
						{
							string lastLetter = text.Substring(currentVisibleChars - 1, 1);
							if (lastLetter != " " && lastLetter != "\n") 
							{
								var blipData = currentVoice.Pick(lastLetter);
								if (blipData.stream != null)
								{
									SfxTyping.Stream = blipData.stream;
									SfxTyping.PitchScale = blipData.pitchScale;
									SfxTyping.VolumeDb = currentVoice.VolumeDb;
									SfxTyping.Play();
								}
							}
							sfxTimer = 0f;
						}
					}
				}

				if (Input.IsActionJustPressed("ui_accept") || 
					Input.IsActionJustPressed("jump") || 
					Input.IsActionJustPressed("interact"))
				{
					if (isTyping)
					{
						textTween.Kill();
						_dialogueLabel.VisibleRatio = 1.0f;
						isTyping = false;
						StartBlinking();
						if (SfxTyping != null) SfxTyping.Stop();
						await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);
					}
					else 
					{
						StopBlinking();
						break;
					}
				}
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
			if (SfxTyping != null) SfxTyping.Stop();
			await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		}
		
		_dialoguePanel.Hide();
		StopBlinking();
		
		await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);

		_amari.IsInCutscene = true; 

		if (amariSprite != null && amariSprite.SpriteFrames != null && amariSprite.SpriteFrames.HasAnimation("magicLeaf"))
		{
			amariSprite.Play("magicLeaf");
			amariSprite.Offset = new Vector2(0, 10); 

			await ToSignal(GetTree().CreateTimer(1.5f), SceneTreeTimer.SignalName.Timeout);
			amariSprite.Stop(); 
			amariSprite.Frame = 12; 
		}
		else
		{
			await ToSignal(GetTree().CreateTimer(1.5f), SceneTreeTimer.SignalName.Timeout);
		}

		GD.Print("[Cutscene] Daun mulai menyala...");
		Tween lightTween = CreateTween();
		lightTween.TweenProperty(_magicLeaf, "energy", 1.5f, _lightFadeDuration)
				  .SetTrans(Tween.TransitionType.Sine)
				  .SetEase(Tween.EaseType.InOut);
		await ToSignal(lightTween, Tween.SignalName.Finished);

		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

		if (amariSprite != null && amariSprite.SpriteFrames != null && amariSprite.SpriteFrames.HasAnimation("idleWithKeranjang"))
		{
			amariSprite.Play("idleWithKeranjang");
			amariSprite.Offset = new Vector2(0, 0);
		}

		_dialoguePanel.Show();
		StopBlinking();

		// ==========================================
		// DIALOG PART 2
		// ==========================================
		for (int i = 0; i < _dialoguesPart2.Length; i++)
		{
			string speaker = "";
			string text = _dialoguesPart2[i];

			if (_dialoguesPart2[i].Contains(":"))
			{
				var parts = _dialoguesPart2[i].Split(new[] { ':' }, 2);
				speaker = parts[0].Trim();
				text = parts[1].Trim();
			}

			if (_subjectLabel != null) _subjectLabel.Text = speaker;
			if (_subjectBox != null) _subjectBox.Visible = !string.IsNullOrEmpty(speaker);

			BlipVoice currentVoice = AmariVoice;
			if (!string.IsNullOrEmpty(speaker))
			{
				string speakerLower = speaker.ToLower();
				if (speakerLower.Contains("amari")) currentVoice = AmariVoice;
				else if (speakerLower.Contains("krieger")) currentVoice = KriegerVoice;
			}

			_dialogueLabel.Text = text;
			_dialogueLabel.VisibleRatio = 0.0f;

			StopBlinking();

			float duration = text.Length * 0.03f; 

			Tween textTween = CreateTween();
			textTween.TweenProperty(_dialogueLabel, "visible_ratio", 1.0f, duration);

			bool isTyping = true;
			float sfxTimer = 0f;
			int lastBlipIndex = 0;
			
			textTween.Finished += () => 
			{
				isTyping = false;
				StartBlinking();
				if (SfxTyping != null) SfxTyping.Stop();
			};

			while (true)
			{
				if (isTyping && SfxTyping != null && currentVoice != null)
				{
					float visibleRatio = _dialogueLabel.VisibleRatio;
					int currentVisibleChars = (int)(visibleRatio * text.Length);

					if (currentVisibleChars > lastBlipIndex)
					{
						lastBlipIndex = currentVisibleChars;
						sfxTimer += (float)GetProcessDeltaTime();

						if (sfxTimer >= 0.06f) 
						{
							string lastLetter = text.Substring(currentVisibleChars - 1, 1);
							if (lastLetter != " " && lastLetter != "\n") 
							{
								var blipData = currentVoice.Pick(lastLetter);
								if (blipData.stream != null)
								{
									SfxTyping.Stream = blipData.stream;
									SfxTyping.PitchScale = blipData.pitchScale;
									SfxTyping.VolumeDb = currentVoice.VolumeDb;
									SfxTyping.Play();
								}
							}
							sfxTimer = 0f;
						}
					}
				}

				if (Input.IsActionJustPressed("ui_accept") || 
					Input.IsActionJustPressed("jump") || 
					Input.IsActionJustPressed("interact"))
				{
					if (isTyping)
					{
						textTween.Kill();
						_dialogueLabel.VisibleRatio = 1.0f;
						isTyping = false;
						StartBlinking();
						if (SfxTyping != null) SfxTyping.Stop();
						await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);
					}
					else 
					{
						StopBlinking();
						break;
					}
				}
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
			if (SfxTyping != null) SfxTyping.Stop();
			await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		}
		_dialoguePanel.Hide();
		StopBlinking();

		Tween zoomBack = CreateTween();
		zoomBack.TweenProperty(_camera, "zoom", _originalZoom, 0.8f)
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.InOut);
		await ToSignal(zoomBack, Tween.SignalName.Finished);

		GD.Print("[Cutscene] Amari & Player jalan barengan");
		
		_amari.IsInCutscene = false; 

		if (_player != null) 
			_player.IsMovementLocked = false; 

		_amari.WalkToX(_amariCheckpoint.GlobalPosition.X, _amariWalkSpeed);
		_player.AutoWalkToX(_playerCheckpoint.GlobalPosition.X, _playerWalkSpeed);

		while (!_amari.IsWalkingDone || !_player.IsAutoWalking)
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		if (_player != null) 
			_player.IsMovementLocked = true;

		GD.Print("[Cutscene] Sampai di checkpoint");

		if (game_state.Instance != null)
		{
			game_state.Instance.CaveCutsceneDone = true;
			GD.Print("[Cutscene] Flag CaveCutsceneDone = TRUE");
		}

		if (_blackScreen != null)
		{
			_blackScreen.Visible = false;
			GD.Print("[Cutscene] BlackScreen di-hide");
		}

		if (_pathSelection != null && screen_fade.Instance != null)
		{
			GD.Print("[Cutscene] Fade out sebelum Path Selection...");
			screen_fade.Instance.FadeOut(0.5f);
			await ToSignal(GetTree().CreateTimer(0.6f), SceneTreeTimer.SignalName.Timeout);
		}

		if (_pathSelection != null)
		{
			GD.Print("[Cutscene] Munculkan Path Selection");
			_pathSelection.ShowSelection();
			_player.IsMovementLocked = true;

			if (screen_fade.Instance != null)
			{
				await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);
				screen_fade.Instance.FadeIn(0.6f);
				GD.Print("[Cutscene] Fade in setelah Path Selection muncul");
			}
		}
		else
		{
			GD.Print("[Cutscene] PathSelection NULL - Player unlock");
			_player.IsMovementLocked = false;
		}

		GD.Print("[Cutscene] Selesai!");
	}
}
