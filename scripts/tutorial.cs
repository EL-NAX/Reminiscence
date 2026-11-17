using Godot;

public partial class tutorial : Node2D
{
	[Export] private ColorRect _blackScreen;
	[Export] private Node2D _player;
	[Export] private Label _hintLabel;

	private Tween _blinkTween;

	public override async void _Ready()
	{
		RenderingServer.SetDefaultClearColor(Colors.Black);

		if (_player != null)
		{
			_player.SetProcess(false);
			_player.SetPhysicsProcess(false);

			var camera = _player.GetNodeOrNull<Camera2D>("Camera2D");
			if (camera != null) camera.ResetSmoothing();
		}

		if (_blackScreen != null)
		{
			_blackScreen.Show();
			_blackScreen.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			_blackScreen.Color = new Color(0, 0, 0, 1.0f);

			Tween fadeIn = CreateTween();
			fadeIn.TweenProperty(_blackScreen, "color:a", 0.0f, 1.0f)
				  .SetTrans(Tween.TransitionType.Cubic)
				  .SetEase(Tween.EaseType.InOut);

			await ToSignal(fadeIn, Tween.SignalName.Finished);
			_blackScreen.Hide();
		}

		if (_player != null)
		{
			_player.SetProcess(true);
			_player.SetPhysicsProcess(true);
		}
		
		// Siapin Label Hint
		if (_hintLabel == null)
		{
			BikinHintLabel();
		}
	}

	private void BikinHintLabel()
	{
		var canvasLayer = new CanvasLayer();
		canvasLayer.Layer = 100; 
		AddChild(canvasLayer);

		_hintLabel = new Label();
		_hintLabel.Text = "Pergi ke ujung map >";
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
		
		canvasLayer.AddChild(_hintLabel);
	}

	// FUNGSI INI HARUS PUBLIC BIAR BISA DIPANGGIL DARI LUAR
	public void OnTutorialComplete()
	{
		TampilkanPetunjuk();
		
		// Panggil HUD Keybind dari Autoload
		var hud = GetNodeOrNull<keybind_hud>("/root/KeybindHUD");
		if (hud != null)
		{
			hud.UnlockHUD();
		}
	}

	private async void TampilkanPetunjuk()
	{
		if (_hintLabel == null) return;

		GD.Print("[Tutorial] Nampilin petunjuk di tengah layar!");
		
		_hintLabel.Visible = true;
		_hintLabel.Modulate = new Color(1, 1, 1, 1);

		if (_blinkTween != null && _blinkTween.IsValid()) _blinkTween.Kill();
		_blinkTween = CreateTween();
		_blinkTween.SetLoops();
		_blinkTween.TweenProperty(_hintLabel, "modulate:a", 0.2f, 0.4f);
		_blinkTween.TweenProperty(_hintLabel, "modulate:a", 1.0f, 0.4f);

		await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout);

		if (_blinkTween != null && _blinkTween.IsValid()) _blinkTween.Kill();
		
		var fadeOutTween = CreateTween();
		fadeOutTween.TweenProperty(_hintLabel, "modulate:a", 0.0f, 0.5f);
		await ToSignal(fadeOutTween, Tween.SignalName.Finished);

		_hintLabel.Visible = false;
	}
}
