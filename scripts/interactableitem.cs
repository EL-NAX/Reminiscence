using Godot;

public partial class interactableitem : Area2D
{
	private bool playerInRange = false;
	private Label interactionLabel;

	public override void _Ready()
	{
		Area2D interactionArea = GetNode<Area2D>("InteractionArea");
		interactionArea.BodyEntered += OnPlayerEntered;
		interactionArea.BodyExited += OnPlayerExited;
		
		interactionLabel = GetNode<Label>("Label");
		if (interactionLabel != null)
		{
			interactionLabel.Visible = false;
		}
	}

	private void OnPlayerEntered(Node2D body)
	{
		if (body is player)
		{
			playerInRange = true;
			GD.Print("Tekan F untuk berinteraksi");
			
			if (interactionLabel != null)
			{
				interactionLabel.Visible = true;
				interactionLabel.Text = "Tekan F untuk berinteraksi";
			}
		}
	}

	private void OnPlayerExited(Node2D body)
	{
		if (body is player)
		{
			playerInRange = false;
			GD.Print("Player keluar dari area item");
			
			if (interactionLabel != null)
			{
				interactionLabel.Visible = false;
			}
		}
	}

	public override void _Process(double delta)
	{
		// Menggunakan Input Map dengan action "interact"
		if (playerInRange && Input.IsActionJustPressed("interact"))
		{
			Interact();
		}
	}

	private void Interact()
	{
		GD.Print("ITEM DIINTERAKSI!");
		QueueFree();
	}
}
