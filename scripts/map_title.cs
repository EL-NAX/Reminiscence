using Godot;
using System.Threading.Tasks;

public partial class map_title : CanvasLayer
{
	private Label _titleLabel;
	private ColorRect _underline;

	public override void _Ready()
	{
		Layer = 95; 
		
		_titleLabel = GetNodeOrNull<Label>("TitleLabel");
		_underline = GetNodeOrNull<ColorRect>("Underline");

		// Sembunyiin di awal
		if (_titleLabel != null) 
		{
			_titleLabel.Modulate = new Color(1, 1, 1, 0);
			_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_titleLabel.VerticalAlignment = VerticalAlignment.Center;
			_titleLabel.AddThemeFontSizeOverride("font_size", 48);
			_titleLabel.AddThemeColorOverride("font_color", Colors.White);
			_titleLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f));
			_titleLabel.AddThemeConstantOverride("shadow_offset_x", 4);
			_titleLabel.AddThemeConstantOverride("shadow_offset_y", 4);
		}
		
		if (_underline != null) 
		{
			_underline.Modulate = new Color(1, 1, 1, 0);
			_underline.Color = Colors.White;
		}
	}

	public async void ShowTitle(string mapName)
	{
		if (_titleLabel == null || _underline == null) return;

		GD.Print($"[MapTitle] Munculin judul map: {mapName}");
		
		_titleLabel.Text = mapName;
		
		// =======================================================
		// PAKSA UKURAN & POSISI (ANTI GAGAL)
		// =======================================================
		Vector2 screenSize = GetViewport().GetVisibleRect().Size;
		float centerX = screenSize.X / 2;

		// Set Label jadi selebar layar
		_titleLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
		_titleLabel.OffsetLeft = 0;
		_titleLabel.OffsetRight = 0;
		_titleLabel.OffsetTop = 80;
		_titleLabel.OffsetBottom = 180;

		// Set Garis
		_underline.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		_underline.Size = new Vector2(200, 4);
		_underline.Position = new Vector2(centerX - 100, 160);
		// =======================================================

		// Tunggu 1 frame biar Godot selesai ngitung ukurannya
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		// Fade In
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_titleLabel, "modulate:a", 1.0f, 0.8f);
		tween.TweenProperty(_underline, "modulate:a", 1.0f, 0.8f);
		
		await ToSignal(tween, Tween.SignalName.Finished);
		
		// Tunggu 2 detik
		await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
		
		// Fade Out
		var fadeOutTween = CreateTween();
		fadeOutTween.SetParallel(true);
		fadeOutTween.TweenProperty(_titleLabel, "modulate:a", 0.0f, 0.8f);
		fadeOutTween.TweenProperty(_underline, "modulate:a", 0.0f, 0.8f);
		
		await ToSignal(fadeOutTween, Tween.SignalName.Finished);
	}
}
