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

	// TAMBAHAN: Audio Player & Resource
	[Export] public AudioStreamPlayer SfxTyping; 
	[Export] public BlipVoice AmariVoice;
	[Export] public BlipVoice KriegerVoice;

	private Control _panel;
	private Label _label;
	private Control _continueLabel; 
	private Label _subjectLabel;
	private Control _subjectBox;
	
	private bool _isActive = false;
	private Camera2D _camera;
	private Vector2 _originalZoom;
	
	private Tween _blinkTween; 

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

	private void StartBlinking()
	{
		if (_continueLabel == null) return;
		
		_continueLabel.Visible = true;
		
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
		_continueLabel.Modulate = new Color(1, 1, 1, 1); 
		
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

			// ========================================================
			// TENTUKAN SUARA SIAPA YANG MUNCUL
			// ========================================================
			BlipVoice currentVoice = AmariVoice; // Default
			if (!string.IsNullOrEmpty(speaker))
			{
				string speakerLower = speaker.ToLower();
				if (speakerLower.Contains("amari")) 
					currentVoice = AmariVoice;
				else if (speakerLower.Contains("krieger")) 
					currentVoice = KriegerVoice;
			}
			// ========================================================

			_label.Text = text;
			_label.VisibleRatio = 0.0f; 

			StopBlinking(); 

			float duration = text.Length * TypeDurationPerChar;

			Tween textTween = CreateTween();
			textTween.TweenProperty(_label, "visible_ratio", 1.0f, duration);

			bool isTyping = true;
			float sfxTimer = 0f;
			int lastBlipIndex = 0;
			
			textTween.Finished += () => 
			{
				isTyping = false;
				StartBlinking();
				if (SfxTyping != null) SfxTyping.Stop(); // Matikan suara saat selesai
			};

			while (true)
			{
				// ========================================================
				// LOGIKA MEMUTAR SFX BERDASARKAN HURUF YANG MUNCUL
				// ========================================================
				if (isTyping && SfxTyping != null && currentVoice != null)
				{
					float visibleRatio = _label.VisibleRatio;
					int currentVisibleChars = (int)(visibleRatio * text.Length);

					if (currentVisibleChars > lastBlipIndex)
					{
						lastBlipIndex = currentVisibleChars; // Update index
						sfxTimer += (float)GetProcessDeltaTime();

						// Interval antar suara (bisa disesuaikan)
						if (sfxTimer >= 0.06f) 
						{
							// Ambil huruf terakhir
							string lastLetter = text.Substring(currentVisibleChars - 1, 1);
							
							// Abaikan spasi atau enter
							if (lastLetter != " " && lastLetter != "\n") 
							{
								// Panggil fungsi Pick dari BlipVoice
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
				// ========================================================

				if (Input.IsActionJustPressed("ui_accept") || 
					Input.IsActionJustPressed("jump") || 
					Input.IsActionJustPressed("interact"))
				{
					if (isTyping)
					{
						textTween.Kill();
						_label.VisibleRatio = 1.0f;
						isTyping = false;
						StartBlinking(); 
						
						// Matikan suara jika di-skip
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

			await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		}

		_panel.Visible = false;
		StopBlinking();
		
		// Pastikan suara mati total saat dialog selesai
		if (SfxTyping != null) SfxTyping.Stop();

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
