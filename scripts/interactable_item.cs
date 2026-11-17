using Godot;

public partial class interactable_item : Area2D
{
	private Label _label;
	private bool _playerInRange = false;
	public bool _used = false; 

	public override void _Ready()
	{
		AddToGroup("IceSword");
		_label = GetNodeOrNull<Label>("Label");
		if (_label != null) _label.Visible = false;
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	public override void _Process(double delta)
	{
		if (_used || !_playerInRange) return;
		if (Input.IsActionJustPressed("interact")) OnInteract();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!body.IsInGroup("Player")) return;
		_playerInRange = true;
		if (_label != null && !_used)
		{
			_label.Text = "Tekan F untuk menarik pedang";
			_label.Visible = true;
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (!body.IsInGroup("Player")) return;
		_playerInRange = false;
		if (_label != null) _label.Visible = false;
	}

	private void OnInteract()
	{
		_used = true;
		if (_label != null) _label.Visible = false;

		var controller = GetTree().GetFirstNodeInGroup("IceCave2Controller");
		if (controller != null && controller.HasMethod("StartSwordQTESequence"))
			controller.Call("StartSwordQTESequence");
	}
}
