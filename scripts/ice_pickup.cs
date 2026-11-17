using Godot;

public partial class ice_pickup : Area2D
{
	[Export] public float PlayerCollectDelay = 0.6f;

	public bool _collected = false;
	private bool _playerInRange = false;
	private Label _label;
	private Sprite2D _sprite;

	public override void _Ready()
	{
		AddToGroup("IcePickup");
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		Monitoring = true;

		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		_label = GetNodeOrNull<Label>("Label");
		if (_label != null) _label.Visible = false;
	}

	public override void _Process(double delta)
	{
		if (_collected) return;

		if (_playerInRange && Input.IsActionJustPressed("interact"))
		{
			PlayerCollect();
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_collected) return;
		if (!body.IsInGroup("Player")) return;

		_playerInRange = true;
		if (_label != null)
		{
			_label.Text = "Tekan F untuk ambil serpihan";
			_label.Visible = true;
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (!body.IsInGroup("Player")) return;
		_playerInRange = false;
		if (_label != null) _label.Visible = false;
	}

	private async void PlayerCollect()
	{
		if (_collected) return;
		_collected = true;
		if (_label != null) _label.Visible = false;

		// Animasi interact player
		var playerNode = GetTree().GetFirstNodeInGroup("Player") as player;
		if (playerNode != null) playerNode.PlayInteractAnimation();

		// Delay
		await ToSignal(GetTree().CreateTimer(PlayerCollectDelay), SceneTreeTimer.SignalName.Timeout);
		if (!IsInstanceValid(this)) return;

		// Warna es berubah (jadi biru gelap / abu)
		if (_sprite != null)
		{
			var tween = CreateTween();
			tween.TweenProperty(_sprite, "modulate", new Color(0.35f, 0.45f, 0.7f, 1f), 0.4f);
		}

		// Notif ke controller
		var controller = GetTree().GetFirstNodeInGroup("IceCave1Controller");
		if (controller != null)
		{
			if (controller.HasMethod("OnIceCollected"))
				controller.Call("OnIceCollected");
			if (controller.HasMethod("CheckAllCollected"))
				controller.Call("CheckAllCollected");
		}

		var currentQuest = quest_manager.Instance.GetCurrentQuest();
		if (currentQuest != null && currentQuest.Type == quest_data.QuestType.Collect)
			quest_manager.Instance.ReportProgress(1);
	}
}
