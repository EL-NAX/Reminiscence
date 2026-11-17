using Godot;
using System.Threading.Tasks;

public partial class screen_fade : CanvasLayer
{
	public static screen_fade Instance { get; private set; }
	private ColorRect _rect;

	public override void _Ready()
	{
		Instance = this;
		Layer = 100;
		ProcessMode = ProcessModeEnum.Always;

		_rect = new ColorRect();
		_rect.Color = new Color(0, 0, 0, 0);
		_rect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_rect.MouseFilter = Control.MouseFilterEnum.Ignore;
		AddChild(_rect);
	}

	// Set alpha hitam langsung (buat fade black di awal scene)
	public void SetAlpha(float alpha)
	{
		_rect.Color = new Color(0, 0, 0, alpha);
	}

	public float GetAlpha() => _rect.Color.A;

	public async Task FadeOut(float duration = 0.3f)
	{
		_rect.MouseFilter = Control.MouseFilterEnum.Stop;
		var tween = CreateTween();
		tween.TweenProperty(_rect, "color:a", 1.0f, duration);
		await ToSignal(tween, "finished");
	}

	public async Task FadeIn(float duration = 0.3f)
	{
		var tween = CreateTween();
		tween.TweenProperty(_rect, "color:a", 0.0f, duration);
		await ToSignal(tween, "finished");
		_rect.MouseFilter = Control.MouseFilterEnum.Ignore;
	}

	public async Task TransitionTo(string scenePath, float fadeDuration = 0.4f)
	{
		await FadeOut(fadeDuration);
		GetTree().ChangeSceneToFile(scenePath);
		await ToSignal(GetTree(), "process_frame");
		// Fade in akan di-handle controller scene berikutnya
	}
}
