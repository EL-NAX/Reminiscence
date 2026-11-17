using Godot;

public partial class game_state : Node
{
	public static game_state Instance { get; private set; }

	// GENERAL
	public bool CaveCutsceneDone { get; set; } = false;
	public bool TutorialDone { get; set; } = false;

	// CHAPTER 1 — ICE CAVE
	public bool AmariWentAhead { get; set; } = false;
	public bool IceSwordPulled { get; set; } = false;
	public bool Chapter1Done { get; set; } = false;
	public int TotalIceCollected { get; set; } = 0;

	public override void _Ready()
	{
		Instance = this;
		GD.Print("[GameState] Autoload ready!");
	}

	public void ResetAll()
	{
		CaveCutsceneDone = false;
		TutorialDone = false;
		AmariWentAhead = false;
		IceSwordPulled = false;
		Chapter1Done = false;
		TotalIceCollected = 0;
		GD.Print("[GameState] Reset all flags");
	}
}
