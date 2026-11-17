using Godot;
using System.Threading.Tasks;

public partial class dialogue_controller : CanvasLayer
{
	[Export] public float TypeDurationPerChar = 0.03f;
	[Export] public float ZoomAmount = 1.35f;
	[Export] public float ZoomDuration = 0.8f;
	[Export] public bool AutoZoom = true;
	[Export] public bool AutoAdvance = false;          
	[Export] public float AutoAdvanceDelay = 1.5f;     

	private Control _panel;
	private Label _label;
	private Control _continueLabel; 
	private Label _subjectLabel;
	private Control _subjectBox;
	
	private bool _isActive = false;
	private Camera2D _camera;
	private Vector2 _originalZoom;
	
	private Tween _blinkTween; // Simpan tween biar bisa di-kill kalau perlu

	public override void _Ready()
	{
		AddToGroup("DialogUI");
		Layer = 50;
		ProcessMode = ProcessModeEnum.Always;

		_panel = GetNodeOrNull<Control>("Panel");
		_label = GetNodeOrNull<Label>("Panel/Label");
		_continueLabel = GetNodeOrNull<Control>("Panel/ContinueLabel");
		
		_subjectBox = GetNodeOrNull<Control>("Panel/SubjectBox");
		_subjectLabel = GetNodeOrNull<Label>("Panel/SubjectBox/SubjectLabel");

		if (_panel == null) GD.PrintErr("[Dialogue] Panel tidak ditemukan!");
		if (_label == null) GD.PrintErr("[Dialogue] Label tidak ditemukan!");
		if (_continueLabel == null) GD.PrintErr("[Dialogue] ContinueLabel tidak ditemukan!");
		if (_subjectBox == null) GD.PrintErr("[Dialogue] SubjectBox tidak ditemukan!");
		if (_subjectLabel == null) GD.PrintErr("[Dialogue] SubjectLabel tidak ditemukan!");

		if (_panel != null) _panel.Visible = false;
		if (_continueLabel != null) _continueLabel.Visible = false;
	}

	// Fungsi baru untuk mulai berkedip
	private void StartBlinking()
	{
		if (_continueLabel == null) return;
		
		_continueLabel.Visible = true;
		
		// Hapus tween lama kalau masih jalan
		if (_blinkTween != null && _blinkTween.IsValid())
			_blinkTween.Kill();

		_blinkTween = CreateTween();
		_blinkTween.SetLoops(); 
		_blinkTween.TweenProperty(_continueLabel, "modulate:a", 0.2f, 0.5f);
		_blinkTween.TweenProperty(_continueLabel, "modulate:a", 1.0f, 0.5f);
	}

	private void StopBlinking()
	{
		if (_continueLabel == null) return;
		
		_continueLabel.Visible = false;
		_continueLabel.Modulate = new Color(1, 1, 1, 1); // Kembalikan ke terang
		
		if (_blinkTween != null && _blinkTween.IsValid())
			_blinkTween.Kill();
	}

	private void FindCamera()
	{
		if (_camera != null) return;

		var player = GetTree().GetFirstNodeInGroup("Player");
		if (player != null)
		{
			_camera = player.GetNodeOrNull<Camera2D>("Camera2D");
			if (_camera != null)
				_originalZoom = _camera.Zoom;
		}
	}

	public async Task ShowDialogue(string[] lines, bool skipZoomOut = false)
	{
		if (lines == null || lines.Length == 0) return;
		if (_label == null || _panel == null) return;

		_isActive = true;
		_panel.Visible = true;
		_panel.Modulate = new Color(1, 1, 1, 1);

		StopBlinking();

		// ZOOM IN KAMERA
		if (AutoZoom)
		{
			FindCamera();
			if (_camera != null)
			{
				_originalZoom = _camera.Zoom;
				Vector2 targetZoom = _originalZoom * ZoomAmount;

				Tween zoomIn = CreateTween();
				zoomIn.TweenProperty(_camera, "zoom", targetZoom, ZoomDuration)
					  .SetTrans(Tween.TransitionType.Cubic)
					  .SetEase(Tween.EaseType.InOut);

				await ToSignal(zoomIn, Tween.SignalName.Finished);
			}
		}

		// LOOPING DIALOG
		for (int i = 0; i < lines.Length; i++)
		{
			string speaker = "";
			string text = lines[i];

			if (lines[i].Contains(":"))
			{
				var parts = lines[i].Split(new[] { ':' }, 2);
				speaker = parts[0].Trim();
				text = parts[1].Trim();
			}

			if (_subjectLabel != null) _subjectLabel.Text = speaker;
			if (_subjectBox != null) _subjectBox.Visible = !string.IsNullOrEmpty(speaker);

			_label.Text = text;
			_label.VisibleRatio = 0.0f; 

			StopBlinking(); // Sembunyikan panah saat mulai ngetik

			float duration = text.Length * TypeDurationPerChar;

			Tween textTween = CreateTween();
			textTween.TweenProperty(_label, "visible_ratio", 1.0f, duration);

			bool isTyping = true;
			
			// Begitu ketikan selesai, panggil StartBlinking
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
						_label.VisibleRatio = 1.0f;
						isTyping = false;
						StartBlinking(); // Panggil StartBlinking kalau di-skip
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

		_panel.Visible = false;
		StopBlinking();

		// ZOOM OUT KAMERA
		if (AutoZoom && _camera != null && !skipZoomOut)
		{
			Tween zoomOut = CreateTween();
			zoomOut.TweenProperty(_camera, "zoom", _originalZoom, ZoomDuration)
				   .SetTrans(Tween.TransitionType.Cubic)
				   .SetEase(Tween.EaseType.InOut);

			await ToSignal(zoomOut, Tween.SignalName.Finished);
		}

		_isActive = false;
		GD.Print("[Dialogue] Selesai");
	}

	public bool IsActive() => _isActive;
}
