using Godot;

public partial class Interactable : Node2D
{
	[Signal]
	public delegate void InteractedEventHandler();

	public override void _Ready()
	{
		AddToGroup("Interactable");
	}

	public virtual void Interact()
	{
		GD.Print($"[Interactable] {Name} diinteraksi!");
		EmitSignal(SignalName.Interacted);
	}
}
