using Godot;

public partial class tutorial_controller : Node
{
	private amari_ai _amari;

	public override void _Ready()
	{
		// Daftarin ke group biar gampang dicari
		AddToGroup("TutorialController");

		_amari = GetTree().GetFirstNodeInGroup("Amari") as amari_ai;

		if (_amari != null)
		{
			_amari.FollowPlayer = false;
			GD.Print("[TutorialController] Amari STOP");
		}
	}

	public void OnTutorialComplete()
	{
		if (_amari != null)
		{
			_amari.FollowPlayer = true;
			GD.Print("[TutorialController] Amari FOLLOW");
		}

		// Hapus TutorialActions biar input gak bentrok
		var actions = GetNodeOrNull<Node>("../TutorialActions");
		if (actions != null)
		{
			actions.QueueFree();
			GD.Print("[TutorialController] TutorialActions dihapus");
		}

		GD.Print("[TutorialController] ✅ Tutorial selesai!");
	}
}
