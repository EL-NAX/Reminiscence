using Godot;

public partial class tutorial : Node2D
{
	[Export] private ColorRect _blackScreen;
	[Export] private Node2D _player;

	public override async void _Ready()
	{
		RenderingServer.SetDefaultClearColor(Colors.Black);

		// 1. Matikan kontrol player sebentar & reset kamera agar tidak meluncur
		if (_player != null)
		{
			_player.SetProcess(false);
			_player.SetPhysicsProcess(false);

			var camera = _player.GetNodeOrNull<Camera2D>("Camera2D");
			if (camera != null)
			{
				camera.ResetSmoothing();
			}
		}

		// 2. Jalankan Fade-In dari Hitam Pekat ke Transparan
		if (_blackScreen != null)
		{
			_blackScreen.Show();
			_blackScreen.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			_blackScreen.Color = new Color(0, 0, 0, 1.0f); // Hitam Pekat

			Tween fadeIn = CreateTween();
			fadeIn.TweenProperty(_blackScreen, "color:a", 0.0f, 1.0f)
				  .SetTrans(Tween.TransitionType.Cubic)
				  .SetEase(Tween.EaseType.InOut);

			await ToSignal(fadeIn, Tween.SignalName.Finished);
			
			_blackScreen.Hide(); // Sembunyikan setelah Fade-In selesai
		}
		else
		{
			GD.PrintErr("⚠️ ERROR: Slot '_blackScreen' BELUM DIISI di Inspector Scene Tutorial!");
		}

		// 3. Kembalikan kontrol Player
		if (_player != null)
		{
			_player.SetProcess(true);
			_player.SetPhysicsProcess(true);
		}
	}
}
