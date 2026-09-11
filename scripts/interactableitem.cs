using Godot;

public partial class interactableitem : Area2D
{
	private Label interactionLabel;
	private bool playerInRange = false;
	private player currentPlayer; // <-- INI YANG DIPERBAIKI: tipe player (huruf kecil)

	public override void _Ready()
	{
		// Ambil label di dalam scene (nama node harus "Label")
		interactionLabel = GetNode<Label>("Label");
		if (interactionLabel != null)
		{
			interactionLabel.Visible = false;
		}

		// Hubungkan signal dari InteractionArea
		Area2D interactionArea = GetNode<Area2D>("InteractionArea");
		interactionArea.BodyEntered += OnPlayerEntered;
		interactionArea.BodyExited += OnPlayerExited;
	}

	private void OnPlayerEntered(Node2D body)
	{
		// Cek apakah yang masuk adalah player (huruf kecil, sesuai nama class)
		if (body is player playerNode)
		{
			playerInRange = true;
			currentPlayer = playerNode; // Simpan referensi
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
			currentPlayer = null;

			if (interactionLabel != null)
			{
				interactionLabel.Visible = false;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (playerInRange && Input.IsActionJustPressed("interact"))
		{
			Interact();
		}
	}

	private async void Interact()
	{
		if (currentPlayer == null) return;

		// Cegah spam interact
		playerInRange = false;

		// Sembunyikan label
		if (interactionLabel != null)
		{
			interactionLabel.Visible = false;
		}

		// Panggil animasi interact di player
		currentPlayer.PlayInteractAnimation();

		// Tunggu animasi selesai (sesuaikan dengan InteractDuration di player)
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

		// Hapus item setelah diambil
		QueueFree();
	}
}
