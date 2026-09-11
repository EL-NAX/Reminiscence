using Godot;

public partial class interactableitem : Area2D
{
	private bool playerInRange = false;
	private Label interactionLabel;
	
	// ===== TAMBAHAN: Referensi ke Player =====
	private player currentPlayer = null;
	// ===== END TAMBAHAN =====

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
			
			// ===== TAMBAHAN: Simpan referensi player =====
			currentPlayer = body as player;
			// ===== END TAMBAHAN =====
			
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
			
			// ===== TAMBAHAN: Hapus referensi player =====
			currentPlayer = null;
			// ===== END TAMBAHAN =====
			
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

	// ===== DIUBAH: dari `private void Interact()` menjadi `private async void Interact()` =====
	private async void Interact()
	{
		GD.Print("ITEM DIINTERAKSI!");
		
		// Sembunyikan label dulu
		if (interactionLabel != null)
		{
			interactionLabel.Visible = false;
		}
		
		// Cegah spam interact
		playerInRange = false;
		
		// Panggil animasi interact di player
		if (currentPlayer != null)
		{
			currentPlayer.PlayInteractAnimation();
			
			// Tunggu animasi selesai (sesuaikan dengan InteractDuration di player)
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		}
		
		QueueFree();
	}
	// ===== END DIUBAH =====
}