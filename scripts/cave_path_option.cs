using Godot;

public partial class cave_path_option : Control
{
	[Export] public string TargetScenePath = "res://scenes/ice_cave/ice_cave_1.tscn";
	[Export] public Sprite2D PathImage;
	[Export] public Color NormalColor = new Color(0.15f, 0.15f, 0.15f);
	[Export] public Color HoverColor = new Color(1f, 1f, 1f);
	[Export] public float TransitionDuration = 0.2f;

	private Tween _tween;
	private bool _clicked = false;

	public override void _Ready()
	{
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		MouseDefaultCursorShape = CursorShape.PointingHand;

		if (PathImage != null)
			PathImage.Modulate = NormalColor;
	}

	private void OnMouseEntered()
	{
		GD.Print($"[Path] Hover: {Name}");
		AnimateColor(HoverColor);
	}

	private void OnMouseExited()
	{
		AnimateColor(NormalColor);
	}

	private void AnimateColor(Color target)
	{
		if (PathImage == null) return;

		if (_tween != null && _tween.IsValid())
			_tween.Kill();

		_tween = CreateTween();
		_tween.TweenProperty(PathImage, "modulate", target, TransitionDuration)
			  .SetTrans(Tween.TransitionType.Sine)
			  .SetEase(Tween.EaseType.InOut);
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (_clicked) return;
		if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
		{
			_clicked = true;
			GD.Print($"[Path] Klik: {Name} -> {TargetScenePath}");
			GetTree().ChangeSceneToFile(TargetScenePath);
		}
	}
}
