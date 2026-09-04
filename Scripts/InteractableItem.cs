using Godot;

public partial class InteractableItem : Area2D
{
	private bool playerInRange = false;
	private Label interactionLabel;

	public override void _Ready()
	{
		Area2D interactionArea = GetNode<Area2D>("InteractionArea");

		interactionArea.BodyEntered += OnPlayerEntered;
		interactionArea.BodyExited += OnPlayerExited;
		
		// Ambil label - nama node harus "Label"
		interactionLabel = GetNode<Label>("Label");
		if (interactionLabel != null)
		{
			interactionLabel.Visible = false;
		}
	}

	private void OnPlayerEntered(Node2D body)
	{
		if (body is Player)
		{
			playerInRange = true;
			GD.Print("Tekan E untuk berinteraksi");
			
			// Tampilkan label
			if (interactionLabel != null)
			{
				interactionLabel.Visible = true;
				interactionLabel.Text = "Tekan E untuk berinteraksi";
			}
		}
	}

	private void OnPlayerExited(Node2D body)
	{
		if (body is Player)
		{
			playerInRange = false;
			GD.Print("Player keluar dari area item");
			
			// Sembunyikan label
			if (interactionLabel != null)
			{
				interactionLabel.Visible = false;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (playerInRange && Input.IsKeyPressed(Key.E))
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
