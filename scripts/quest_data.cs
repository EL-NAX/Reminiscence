using Godot;

[GlobalClass]
public partial class quest_data : Resource
{
	public enum QuestType { Sequence, Collect, QTE, Reach }

	[Export] public string QuestTitle = "";
	[Export] public QuestType Type = QuestType.Sequence;

	// Sequence
	[Export] public Godot.Collections.Array<string> SubQuests = new();
	[Export] public Godot.Collections.Array<Texture2D> KeyIcons = new();
	[Export] public Godot.Collections.Array<string> SubActions = new();

	// Collect
	[Export] public int TargetCount = 10;
	[Export] public bool UsePercent = true;
	[Export] public Texture2D CollectIcon;

	// QTE
	[Export] public string QTEPrompt = "Spam klik kanan!";
}
