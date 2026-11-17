using Godot;
using System.Threading.Tasks;

public partial class prologue : Node2D
{
	[Export] private Node2D _player;
	[Export] private Node2D _amari;
	[Export] private NinePatchRect _dialoguePanel;
	[Export] private Label _dialogueLabel;
	[Export] private Control _continueLabel; 
	
	[Export] private Control _subjectBox;
	[Export] private Label _subjectLabel;
	
	// VARIABEL BUAT LABEL F
	[Export] private Label _fInstruction;

	[Export] private ColorRect _blackScreen;

	[Export] private string _tutorialScenePath = "res://scenes/tutorial/tutorial.tscn";

	private Tween _blinkTween;

	private readonly string[] _dialogues = new string[]
	{
		"Amari: Hei, Krieger! Pantas saja dicari dari tadi tidak ada, ternyata malah main di sini!",
		"Krieger: Eh, Amari? Ada apa? Tumben menyusul ke sini?",
		"Amari: Lihat nih, aku bawa roti! Kita makan bersama, yuk. Kamu belum makan, kan?",
		"Krieger: Wah, pas sekali! Kebetulan perutku sudah mulai lapar dari tadi. Terima kasih, Amari!",
		"Amari: (Penasaran) Bagaimana? Enak, kan?",
		"Krieger: (Mengunyah pelan, ragu-ragu) Hmm... Agak beda dari yang biasa kamu bawa. Agak keras dan rasanya sedikit hambar...",
		"Amari: Ah! Iya juga... Roti ini sudah tersimpan cukup lama di rumah. Pantas saja rasanya sudah tidak seenak biasanya...",
		"Amari: Tunggu, aku punya ide bagus!",
		"Krieger: Ide apa?",
		"Amari: Bagaimana kalau kita pakai Es dari Goa Es untuk mendinginkan dan mengembalikan kesegaran rotinya? Katanya kalau diberi Es dari sana, teksturnya bisa segar lagi!",
		"Amari: Iya! Kita ke sana sebentar untuk mengambil beberapa potong es, setelah itu rotinya pasti jadi jauh lebih lezat!",
		"Krieger: Sebenarnya dari dulu aku penasaran sekali ingin melihat Goa Es itu secara langsung... Baiklah, ayo kita ke sana!",
		"Amari: Nah, begitu dong! Ayo cepat sebelum harinya makin gelap!"
	};

	// Fungsi baru untuk berkedip
	private void StartBlinking()
	{
		if (_continueLabel == null) return;
		
		// Munculin panah & F
		_continueLabel.Visible = true;
		_continueLabel.Modulate = new Color(1, 1, 1, 1);
		if (_fInstruction != null) 
		{
			_fInstruction.Visible = true;
			_fInstruction.Modulate = new Color(1, 1, 1, 1);
		}

		if (_blinkTween != null && _blinkTween.IsValid()) _blinkTween.Kill();
		_blinkTween = CreateTween();
		_blinkTween.SetLoops(); 
		
		// Kedipin panah
		_blinkTween.TweenProperty(_continueLabel, "modulate:a", 0.2f, 0.5f);
		// Kedipin F barengan (paralel)
		if (_fInstruction != null)
			_blinkTween.Parallel().TweenProperty(_fInstruction, "modulate:a", 0.2f, 0.5f);
			
		// Balikin ke terang
		_blinkTween.TweenProperty(_continueLabel, "modulate:a", 1.0f, 0.5f);
		if (_fInstruction != null)
			_blinkTween.Parallel().TweenProperty(_fInstruction, "modulate:a", 1.0f, 0.5f);
	}

	private void StopBlinking()
	{
		if (_continueLabel == null) return;
		_continueLabel.Visible = false;
		
		if (_fInstruction != null) _fInstruction.Visible = false; 

		if (_blinkTween != null && _blinkTween.IsValid()) _blinkTween.Kill();
	}

	public override async void _Ready()
	{
		RenderingServer.SetDefaultClearColor(Colors.Black);

		if (_dialoguePanel == null || _blackScreen == null || _dialogueLabel == null)
		{
			GD.PrintErr("⚠️ ISI SEMUA SLOT INSPECTOR DI PROLOGUE!");
			return;
		}

		_dialoguePanel.Hide();
		StopBlinking();

		_blackScreen.Show();
		_blackScreen.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		_blackScreen.Color = new Color(0, 0, 0, 1.0f);

		if (_player != null)
		{
			_player.SetProcess(false);
			_player.SetPhysicsProcess(false);
		}
		if (_amari != null)
		{
			_amari.SetProcess(false);
			_amari.SetPhysicsProcess(false);
		}

		var playerSprite = _player?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") 
						   ?? _player?.GetNodeOrNull<AnimatedSprite2D>("Sprite2D");

		if (playerSprite != null)
		{
			playerSprite.FlipH = true;
			if (playerSprite.SpriteFrames != null && playerSprite.SpriteFrames.HasAnimation("attack"))
			{
				playerSprite.Play("attack");
			}
		}

		Tween fadeInTween = CreateTween();
		fadeInTween.TweenProperty(_blackScreen, "color:a", 0.0f, 1.0f)
				   .SetTrans(Tween.TransitionType.Cubic)
				   .SetEase(Tween.EaseType.InOut);

		await ToSignal(fadeInTween, Tween.SignalName.Finished);

		await RunPrologueSequence();
	}

	private async Task RunPrologueSequence()
	{
		var playerSprite = _player?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") 
						   ?? _player?.GetNodeOrNull<AnimatedSprite2D>("Sprite2D");

		var amariSprite = _amari?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") 
						  ?? _amari?.GetNodeOrNull<AnimatedSprite2D>("Sprite2D");

		Camera2D camera = _player?.GetNodeOrNull<Camera2D>("Camera2D");

		if (_amari != null && _player != null)
		{
			if (amariSprite != null)
			{
				amariSprite.FlipH = true;
				if (amariSprite.SpriteFrames != null && amariSprite.SpriteFrames.HasAnimation("walkWithKeranjang"))
				{
					amariSprite.Play("walkWithKeranjang");
				}
			}

			Vector2 targetPos = _player.GlobalPosition + new Vector2(50, 0);
			Tween moveTween = CreateTween();
			moveTween.TweenProperty(_amari, "global_position", targetPos, 2.0f)
					 .SetTrans(Tween.TransitionType.Quad)
					 .SetEase(Tween.EaseType.Out);

			await ToSignal(moveTween, Tween.SignalName.Finished);

			if (amariSprite != null && amariSprite.SpriteFrames != null && amariSprite.SpriteFrames.HasAnimation("idleWithKeranjang"))
			{
				amariSprite.Play("idleWithKeranjang");
			}
		}

		await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);

		if (playerSprite != null)
		{
			if (playerSprite.SpriteFrames != null && playerSprite.SpriteFrames.HasAnimation("idle"))
			{
				playerSprite.Play("idle");
			}
			playerSprite.FlipH = false;
		}

		Vector2 originalZoom = new Vector2(1.0f, 1.0f);
		if (camera != null)
		{
			originalZoom = camera.Zoom;
			Vector2 targetZoom = originalZoom * 1.35f;

			Tween zoomIn = CreateTween();
			zoomIn.TweenProperty(camera, "zoom", targetZoom, 0.8f)
				  .SetTrans(Tween.TransitionType.Cubic)
				  .SetEase(Tween.EaseType.InOut);

			await ToSignal(zoomIn, Tween.SignalName.Finished);
		}

		_dialoguePanel.Show();
		StopBlinking();

		for (int i = 0; i < _dialogues.Length; i++)
		{
			string speaker = "";
			string text = _dialogues[i];

			if (_dialogues[i].Contains(":"))
			{
				var parts = _dialogues[i].Split(new[] { ':' }, 2);
				speaker = parts[0].Trim();
				text = parts[1].Trim();
			}

			if (_subjectLabel != null) _subjectLabel.Text = speaker;
			if (_subjectBox != null) _subjectBox.Visible = !string.IsNullOrEmpty(speaker);

			_dialogueLabel.Text = text;
			_dialogueLabel.VisibleRatio = 0.0f; 

			StopBlinking();

			float duration = text.Length * 0.03f;

			Tween textTween = CreateTween();
			textTween.TweenProperty(_dialogueLabel, "visible_ratio", 1.0f, duration);

			bool isTyping = true;
			
			textTween.Finished += () => 
			{
				isTyping = false;
				StartBlinking();
			};

			while (true)
			{
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

			await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		}

		_dialoguePanel.Hide();
		StopBlinking();

		if (camera != null)
		{
			Tween zoomOut = CreateTween();
			zoomOut.TweenProperty(camera, "zoom", originalZoom, 0.8f)
				   .SetTrans(Tween.TransitionType.Cubic)
				   .SetEase(Tween.EaseType.InOut);

			await ToSignal(zoomOut, Tween.SignalName.Finished);
		}

		Tween fadeOutTween = CreateTween();
		fadeOutTween.TweenProperty(_blackScreen, "color:a", 1.0f, 0.8f)
					.SetTrans(Tween.TransitionType.Cubic)
					.SetEase(Tween.EaseType.InOut);

		await ToSignal(fadeOutTween, Tween.SignalName.Finished);

		GetTree().ChangeSceneToFile(_tutorialScenePath);
	}
}
