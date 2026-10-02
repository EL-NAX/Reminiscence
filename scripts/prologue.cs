using Godot;
using System.Threading.Tasks;

public partial class prologue : Node2D
{
	[Export] private Node2D _player;
	[Export] private Node2D _amari;
	[Export] private Panel _dialoguePanel;
	[Export] private Label _dialogueLabel;
	[Export] private ColorRect _blackScreen;

	[Export] private string _tutorialScenePath = "res://scenes/tutorial/tutorial.tscn";

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

	public override async void _Ready()
	{
		// 0. Kunci warna background jeda frame jadi hitam saat game jalan
		RenderingServer.SetDefaultClearColor(Colors.Black);

		if (_dialoguePanel == null || _blackScreen == null || _dialogueLabel == null)
		{
			GD.PrintErr("⚠️ ISI SEMUA SLOT INSPECTOR DI PROLOGUE!");
			return;
		}

		_dialoguePanel.Hide();

		// 1. Layar Hitam Pekat di Awal
		_blackScreen.Show();
		_blackScreen.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		_blackScreen.Color = new Color(0, 0, 0, 1.0f);

		// 2. Nonaktifkan Pergerakan Karakter
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

		// 3. Animasi Serang Player saat Masih Hitam
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

		// 4. Fade In Mulus
		Tween fadeInTween = CreateTween();
		fadeInTween.TweenProperty(_blackScreen, "color:a", 0.0f, 1.0f)
				   .SetTrans(Tween.TransitionType.Cubic)
				   .SetEase(Tween.EaseType.InOut);

		await ToSignal(fadeInTween, Tween.SignalName.Finished);

		// 5. Jalankan Urutan Cutscene
		await RunPrologueSequence();
	}

	private async Task RunPrologueSequence()
	{
		var playerSprite = _player?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") 
						   ?? _player?.GetNodeOrNull<AnimatedSprite2D>("Sprite2D");

		var amariSprite = _amari?.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") 
						  ?? _amari?.GetNodeOrNull<AnimatedSprite2D>("Sprite2D");

		Camera2D camera = _player?.GetNodeOrNull<Camera2D>("Camera2D");

		// STEP 1: Amari Berjalan Mendekat
		if (_amari != null && _player != null)
		{
			if (amariSprite != null)
			{
				amariSprite.FlipH = true;
				if (amariSprite.SpriteFrames != null && amariSprite.SpriteFrames.HasAnimation("walk"))
				{
					amariSprite.Play("walk");
				}
			}

			Vector2 targetPos = _player.GlobalPosition + new Vector2(50, 0);
			Tween moveTween = CreateTween();
			moveTween.TweenProperty(_amari, "global_position", targetPos, 2.0f)
					 .SetTrans(Tween.TransitionType.Quad)
					 .SetEase(Tween.EaseType.Out);

			await ToSignal(moveTween, Tween.SignalName.Finished);

			if (amariSprite != null && amariSprite.SpriteFrames != null && amariSprite.SpriteFrames.HasAnimation("idle"))
			{
				amariSprite.Play("idle");
			}
		}

		await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);

		// STEP 2: Player Berhenti Attack -> Balik Badan
		if (playerSprite != null)
		{
			if (playerSprite.SpriteFrames != null && playerSprite.SpriteFrames.HasAnimation("idle"))
			{
				playerSprite.Play("idle");
			}
			playerSprite.FlipH = false;
		}

		// STEP 3: ZOOM IN KAMERA (Sebelum Dialog Dimulai)
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

		// =========================================================================
		// STEP 4: PERCAKAPAN DIALOG DENGAN EFEK KETIKAN (TYPEWRITER)
		// =========================================================================
		_dialoguePanel.Show();

		for (int i = 0; i < _dialogues.Length; i++)
		{
			_dialogueLabel.Text = _dialogues[i];
			_dialogueLabel.VisibleRatio = 0.0f; // Sembunyikan teks di awal

			// Durasi ketikan disesuaikan dengan panjang kalimat (0.03f detik per huruf)
			float duration = _dialogues[i].Length * 0.03f;

			Tween textTween = CreateTween();
			textTween.TweenProperty(_dialogueLabel, "visible_ratio", 1.0f, duration);

			bool isTyping = true;
			textTween.Finished += () => isTyping = false;

			// Loop menunggu tombol dipencet
			while (true)
			{
				if (Input.IsActionJustPressed("ui_accept"))
				{
					if (isTyping)
					{
						// Jika sedang mengetik lalu dipencet -> Tampilkan SEMUA teks langsung (Skip animasi ketik)
						textTween.Kill();
						_dialogueLabel.VisibleRatio = 1.0f;
						isTyping = false;
						await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);
					}
					else
					{
						// Jika teks sudah muncul semua lalu dipencet -> Lanjut dialog berikutnya
						break;
					}
				}
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}

			await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		}

		_dialoguePanel.Hide();

		// STEP 5: ZOOM OUT KAMERA (Kembali Normal)
		if (camera != null)
		{
			Tween zoomOut = CreateTween();
			zoomOut.TweenProperty(camera, "zoom", originalZoom, 0.8f)
				   .SetTrans(Tween.TransitionType.Cubic)
				   .SetEase(Tween.EaseType.InOut);

			await ToSignal(zoomOut, Tween.SignalName.Finished);
		}

		// STEP 6: Fade Out ke Hitam & Pindah Scene
		Tween fadeOutTween = CreateTween();
		fadeOutTween.TweenProperty(_blackScreen, "color:a", 1.0f, 0.8f)
					.SetTrans(Tween.TransitionType.Cubic)
					.SetEase(Tween.EaseType.InOut);

		await ToSignal(fadeOutTween, Tween.SignalName.Finished);

		GetTree().ChangeSceneToFile(_tutorialScenePath);
	}
}
