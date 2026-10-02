using Godot;

public partial class npc : Node2D
{
	private CanvasLayer _canvasLayer;
	private Label _dialogueLabel;
	private Label _interactLabel;

	private readonly string[] _dialogues = new string[]
	{
		"NPC : Hei kalian mau apa disini",
		"Krieger: Hemm kami ingin mencari es!",
		"NPC : Sebaiknya kalian hati-hati",
		"Krieger: Baik terimakasih",
		"Krieger: Emang kamu siapa disini",
		"NPC : Saya penghuni goa ini",
		"Krieger: Oh oke, kalo begitu aku pergi dulu ya",
		"NPC : Ya"
	};

	private int _currentIndex = 0;
	private bool _isPlayerInRange = false;
	private bool _isDialogueActive = false;
	
	// Variabel untuk Efek Ketik (Typewriter)
	private bool _isTyping = false;
	private Tween _textTween;
	[Export] private float _typeSpeed = 0.04f;

	private Node2D _targetPlayer = null;

	public override void _Ready()
	{
		_canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
		_dialogueLabel = GetNode<Label>("CanvasLayer/Panel/Label");
		_interactLabel = GetNode<Label>("InteractLabel");

		_canvasLayer.Hide();
		_interactLabel.Hide();
	}

	public override void _Process(double delta)
	{
		if (_isPlayerInRange && !_isDialogueActive && Input.IsActionJustPressed("interact"))
		{
			StartDialogue();
		}
		else if (_isDialogueActive && Input.IsActionJustPressed("ui_accept"))
		{
			if (_isTyping)
			{
				SkipTypewriter();
			}
			else
			{
				AdvanceDialogue();
			}
		}
	}

	private void StartDialogue()
	{
		_isDialogueActive = true;
		_currentIndex = 0;

		_interactLabel.Hide();
		_canvasLayer.Show();

		TogglePlayerMovement(_targetPlayer, false);
		ShowCurrentDialogue();
	}

	private void ShowCurrentDialogue()
	{
		string currentText = _dialogues[_currentIndex];
		_dialogueLabel.Text = currentText;
		_dialogueLabel.VisibleRatio = 0.0f;
		_isTyping = true;

		if (_textTween != null && _textTween.IsValid())
		{
			_textTween.Kill();
		}

		float duration = currentText.Length * _typeSpeed;
		_textTween = CreateTween();
		_textTween.TweenProperty(_dialogueLabel, "visible_ratio", 1.0f, duration);
		_textTween.Finished += () => _isTyping = false;
	}

	private void SkipTypewriter()
	{
		if (_textTween != null && _textTween.IsValid())
		{
			_textTween.Kill();
		}
		_dialogueLabel.VisibleRatio = 1.0f;
		_isTyping = false;
	}

	private void AdvanceDialogue()
	{
		_currentIndex++;
		if (_currentIndex < _dialogues.Length)
		{
			ShowCurrentDialogue();
		}
		else
		{
			EndDialogue();
		}
	}

	private void EndDialogue()
	{
		_canvasLayer.Hide();
		_isDialogueActive = false;
		_isTyping = false;

		TogglePlayerMovement(_targetPlayer, true);

		if (_isPlayerInRange)
		{
			_interactLabel.Show();
		}
	}

	private void TogglePlayerMovement(Node2D player, bool enable)
	{
		if (player != null)
		{
			player.SetPhysicsProcess(enable);
			player.SetProcess(enable);
		}
	}

	private void OnDetectAreaBodyEntered(Node2D body)
	{
		// Hanya deteksi Player (Amari diabaikan)
		if (body.IsInGroup("player") || body.Name == "Player")
		{
			_isPlayerInRange = true;
			_targetPlayer = body;

			if (!_isDialogueActive)
			{
				_interactLabel.Show();
			}
		}
	}

	private void OnDetectAreaBodyExited(Node2D body)
	{
		// Hanya merespons jika yang keluar adalah Player
		if (body == _targetPlayer)
		{
			if (_isDialogueActive)
			{
				EndDialogue();
			}

			_isPlayerInRange = false;
			_interactLabel.Hide();
			_targetPlayer = null;
		}
	}
}
