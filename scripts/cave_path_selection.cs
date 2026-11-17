using Godot;

public partial class cave_path_selection : CanvasLayer
{
	[Export] public Label TitleLabel;
	[Export] public Control PathContainer;
	[Export] public ColorRect BlackOverlay;

	public override void _Ready()
	{
		// Sembunyikan dulu
		Visible = false;
		if (PathContainer != null)
			PathContainer.Modulate = new Color(1, 1, 1, 0);

		GD.Print("[PathSelection] Ready");
	}

	public void ShowSelection()
	{
		GD.Print("[PathSelection] Munculkan UI");
		Visible = true;

		// Fade in container
		if (PathContainer != null)
		{
			PathContainer.Modulate = new Color(1, 1, 1, 0);
			var tween = CreateTween();
			tween.TweenProperty(PathContainer, "modulate:a", 1.0f, 0.6f)
				  .SetTrans(Tween.TransitionType.Sine)
				  .SetEase(Tween.EaseType.InOut);
		}

		// Fade in title
		if (TitleLabel != null)
		{
			TitleLabel.Modulate = new Color(1, 1, 1, 0);
			var tween2 = CreateTween();
			tween2.TweenProperty(TitleLabel, "modulate:a", 1.0f, 0.8f)
				  .SetDelay(0.3f);
		}
	}
}
