using Godot;

public partial class sword_qte : CanvasLayer
{
	[Signal] public delegate void QTEFinishedEventHandler(bool success);
	[Signal] public delegate void QTEProgressEventHandler(float progress);

	// BALIK KE SETELAN AWAL (LEBIH SULIT)
	[Export] public float BaseDecayRate = 10f;     // Turun lebih cepet
	[Export] public float DifficultyScale = 30f;  // Makin tinggi makin susah banget
	[Export] public float GainPerClick = 6f;      // Sekali spam cuma nambah dikit
	[Export] public float Duration = 15f;         // Waktu lebih mepet

	private Control _root;
	private ProgressBar _progressBar;
	private Label _hintLabel;
	private AnimatedSprite2D _spaceIndicator;

	private float _progress = 0f;
	private float _timer = 0f;
	private bool _isActive = false;

	public override void _Ready()
	{
		AddToGroup("SwordQTE");
		_root = GetNode<Control>("Root");
		_progressBar = GetNode<ProgressBar>("Root/CenterContainer/VBoxContainer/ProgressBar");
		_hintLabel = GetNode<Label>("Root/CenterContainer/VBoxContainer/HintLabel");
		_spaceIndicator = GetNodeOrNull<AnimatedSprite2D>("Root/CenterContainer/VBoxContainer/SpaceIndicator");

		_root.Visible = false;
		ProcessMode = ProcessModeEnum.Always;
		
		// Sembunyikan label dari awal
		if (_hintLabel != null) _hintLabel.Visible = false; 
	}

	public void StartQTE()
	{
		_progress = 0f;
		_timer = 0f;
		_isActive = true;
		_root.Visible = true;
		_progressBar.Value = 0;
		
		// Pastikan label tetap sembunyi
		if (_hintLabel != null) _hintLabel.Visible = false;
		
		if (_spaceIndicator != null) 
		{
			_spaceIndicator.Visible = true;
			_spaceIndicator.Play("space_blink"); 
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (!_isActive) return;

		if (Input.IsActionJustPressed("jump") || Input.IsActionJustPressed("ui_accept"))
		{
			_progress += GainPerClick;
		}
	}

	public override void _Process(double delta)
	{
		if (!_isActive) return;

		_timer += (float)delta;
		float currentDecay = BaseDecayRate + (_progress / 100f) * DifficultyScale;
		_progress -= currentDecay * (float)delta;
		_progress = Mathf.Clamp(_progress, 0, 100);

		_progressBar.Value = _progress;
		
		EmitSignal(SignalName.QTEProgress, _progress);

		if (_progress >= 100f)
		{
			_isActive = false;
			_root.Visible = false;
			if (_spaceIndicator != null) _spaceIndicator.Visible = false;
			EmitSignal(SignalName.QTEFinished, true);
			return;
		}

		if (_timer >= Duration)
		{
			_isActive = false;
			_root.Visible = false;
			if (_spaceIndicator != null) _spaceIndicator.Visible = false;
			EmitSignal(SignalName.QTEFinished, false);
		}
	}
}
