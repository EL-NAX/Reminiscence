using Godot;
using System.Threading.Tasks;

public partial class tutorial_actions : Node
{
	[Export] private AnimatedSprite2D KeybindAnimasi;
	[Export] private Label TeksPerintah;

	private float _jumpHoldTime = 0f;
	private const float HOLD_THRESHOLD = 0.5f;

	private int _tutorialStep = 0; 
	private bool _sudahLoncat = false;
	private bool _sedangHold = false;
	private bool _sudahTekanA = false;
	private bool _sudahTekanD = false;

	public override async void _Ready()
	{
		if (KeybindAnimasi != null) KeybindAnimasi.Visible = false;
		if (TeksPerintah != null) TeksPerintah.Visible = false;
		
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
		
		_tutorialStep = 0;
		_sudahTekanA = false;
		_sudahTekanD = false;

		if (KeybindAnimasi != null)
		{
			KeybindAnimasi.Visible = true;
			KeybindAnimasi.Play("anim_A_D");
		}
		if (TeksPerintah != null)
		{
			TeksPerintah.Text = "Tekan A dan D untuk bergerak";
			TeksPerintah.Visible = true;
		}
		GD.Print("[Tutorial] Step 0: Tekan A dan D");
	}

	public override void _Input(InputEvent @event)
	{
		if (_tutorialStep == 0)
		{
			if (@event.IsActionPressed("move_left")) _sudahTekanA = true;
			if (@event.IsActionPressed("move_right")) _sudahTekanD = true;

			if (_sudahTekanA && _sudahTekanD) LanjutKeStepLoncat();
		}
		else if (_tutorialStep == 1)
		{
			if (@event.IsActionPressed("jump") && !_sudahLoncat)
			{
				_sudahLoncat = true;
				LanjutKeStepHold();
			}
		}
		else if (_tutorialStep == 2)
		{
			if (@event.IsActionPressed("jump"))
			{
				_sedangHold = true;
				_jumpHoldTime = 0f;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (_tutorialStep == 2 && _sedangHold)
		{
			if (Input.IsActionPressed("jump"))
			{
				_jumpHoldTime += (float)delta;
				if (_jumpHoldTime >= HOLD_THRESHOLD)
				{
					_sedangHold = false;
					LanjutKeStepSelesai();
				}
			}
			else
			{
				_sedangHold = false;
				_jumpHoldTime = 0f;
			}
		}

		if (Input.IsActionJustReleased("jump"))
		{
			_sedangHold = false;
			_jumpHoldTime = 0f;
		}
	}

	private async void LanjutKeStepLoncat()
	{
		_tutorialStep = 1;
		GD.Print("[Tutorial] Step 1: Loncat Biasa");
		
		if (KeybindAnimasi != null) KeybindAnimasi.Play("anim_Space_Normal");
		if (TeksPerintah != null) TeksPerintah.Text = "Tekan Space untuk lompat";
		
		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
	}

	private void LanjutKeStepHold()
	{
		_tutorialStep = 2;
		GD.Print("[Tutorial] Step 2: Hold Space");
		
		if (KeybindAnimasi != null) KeybindAnimasi.Play("anim_Space_Hold");
		if (TeksPerintah != null) TeksPerintah.Text = "Tahan Space untuk lompat tinggi";
	}

	private async void LanjutKeStepSelesai()
	{
		_tutorialStep = 3;
		GD.Print("[Tutorial] Step 3: Selesai!");
		
		// Sembunyiin UI keybind
		if (KeybindAnimasi != null) KeybindAnimasi.Visible = false;
		if (TeksPerintah != null) TeksPerintah.Visible = false;
		
		// =======================================================
		// PANGGIL TUTORIAL CONTROLLER (Amari Follow)
		// =======================================================
		foreach (var node in GetTree().GetNodesInGroup("TutorialController"))
		{
			if (node.HasMethod("OnTutorialComplete"))
			{
				node.Call("OnTutorialComplete");
				GD.Print("[Tutorial] TutorialController dipanggil!");
			}
		}

		// =======================================================
		// PANGGIL TUTORIAL.CS (Tulisan & HUD)
		// =======================================================
		var allNodes = GetTree().Root.FindChildren("*", "Node2D", true, false);
		bool found = false;
		
		foreach (var node in allNodes)
		{
			if (node.HasMethod("OnTutorialComplete") && node != this)
			{
				node.Call("OnTutorialComplete");
				GD.Print($"[Tutorial] Script tutorial.cs dipanggil di node: {node.Name}");
				found = true;
				break;
			}
		}

		if (!found)
		{
			GD.PrintErr("[Tutorial] Gagal nemuin node yang punya OnTutorialComplete!");
		}
		// =======================================================
	}
}
