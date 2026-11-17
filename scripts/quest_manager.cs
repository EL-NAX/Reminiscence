using Godot;

public partial class quest_manager : Node
{
	public static quest_manager Instance { get; private set; }

	[Signal] public delegate void QuestStartedEventHandler(string title);
	[Signal] public delegate void SubQuestCompletedEventHandler(int current, int total);
	[Signal] public delegate void QuestProgressEventHandler(int current, int target, bool usePercent);
	[Signal] public delegate void QuestCompletedEventHandler(string title);
	[Signal] public delegate void AllQuestsCompletedEventHandler();

	private Godot.Collections.Array<quest_data> _quests = new();
	private int _currentQuestIndex = 0;
	private int _currentSubIndex = 0;
	private int _currentProgress = 0;
	private bool _isActive = false;
	private quest_data _lastCompletedQuest;

	public override void _Ready()
	{
		Instance = this;
		GD.Print("[QuestManager] Autoload ready!");
	}

	public void StartQuestSequence(Godot.Collections.Array<quest_data> quests)
	{
		GD.Print($"[QuestManager] Start with {quests.Count} quests");
		_quests = quests;
		_currentQuestIndex = 0;
		_currentSubIndex = 0;
		_currentProgress = 0;
		_isActive = true;
		StartCurrentQuest();
	}

	private void StartCurrentQuest()
	{
		if (_currentQuestIndex >= _quests.Count)
		{
			_isActive = false;
			EmitSignal(SignalName.AllQuestsCompleted);
			return;
		}

		var quest = _quests[_currentQuestIndex];
		if (quest == null) { _currentQuestIndex++; StartCurrentQuest(); return; }

		_currentProgress = 0;
		_currentSubIndex = 0;

		GD.Print($"[QuestManager] Start: {quest.QuestTitle} ({quest.Type})");
		EmitSignal(SignalName.QuestStarted, quest.QuestTitle);

		// Tampilkan sub quest & key icon di awal (untuk Collect juga)
		if (quest.SubQuests.Count > 0)
		{
			EmitSignal(SignalName.SubQuestCompleted, 0, quest.SubQuests.Count);
		}

		// Untuk Collect, emit progress 0%
		if (quest.Type == quest_data.QuestType.Collect)
		{
			EmitSignal(SignalName.QuestProgress, 0, quest.TargetCount, quest.UsePercent);
		}
	}

	public void ReportAction(string action)
	{
		if (!_isActive) return;
		var quest = _quests[_currentQuestIndex];
		if (quest == null || quest.Type != quest_data.QuestType.Sequence) return;
		if (_currentSubIndex >= quest.SubActions.Count) return;

		if (quest.SubActions[_currentSubIndex] != action) return;

		_currentSubIndex++;
		EmitSignal(SignalName.SubQuestCompleted, _currentSubIndex, quest.SubQuests.Count);

		if (_currentSubIndex >= quest.SubQuests.Count)
			CompleteCurrentQuest();
	}

	public void ReportProgress(int amount = 1)
	{
		if (!_isActive) return;
		var quest = _quests[_currentQuestIndex];
		if (quest == null || quest.Type != quest_data.QuestType.Collect) return;

		_currentProgress = Mathf.Min(_currentProgress + amount, quest.TargetCount);
		EmitSignal(SignalName.QuestProgress, _currentProgress, quest.TargetCount, quest.UsePercent);

		if (_currentProgress >= quest.TargetCount)
			CompleteCurrentQuest();
	}

	public void CompleteQTE()
	{
		if (!_isActive) return;
		var quest = _quests[_currentQuestIndex];
		if (quest == null || quest.Type != quest_data.QuestType.QTE) return;
		CompleteCurrentQuest();
	}

	private void CompleteCurrentQuest()
	{
		var quest = _quests[_currentQuestIndex];
		_lastCompletedQuest = quest;
		GD.Print($"[QuestManager] Complete: {quest.QuestTitle}");
		EmitSignal(SignalName.QuestCompleted, quest.QuestTitle);
		GetTree().CreateTimer(2.0).Timeout += NextQuest;
	}

	private void NextQuest()
	{
		_currentQuestIndex++;
		_currentSubIndex = 0;
		_currentProgress = 0;
		StartCurrentQuest();
	}

	public quest_data GetCurrentQuest()
	{
		if (!_isActive || _currentQuestIndex >= _quests.Count) return null;
		return _quests[_currentQuestIndex];
	}

	public quest_data GetLastCompletedQuest() => _lastCompletedQuest;
	public bool IsActive() => _isActive;

	public bool IsActionAllowed(string inputAction)
	{
		if (!_isActive) return false;
		var quest = _quests[_currentQuestIndex];
		if (quest == null || quest.Type != quest_data.QuestType.Sequence) return false;
		if (_currentSubIndex >= quest.SubActions.Count) return false;

		string expected = quest.SubActions[_currentSubIndex];
		return expected switch
		{
			"left" => inputAction == "move_left",
			"right" => inputAction == "move_right",
			"jump" => inputAction == "jump",
			"hold" => inputAction == "jump",
			_ => false
		};
	}

	public string GetCurrentSubAction()
	{
		if (!_isActive || _currentQuestIndex >= _quests.Count) return "";
		var quest = _quests[_currentQuestIndex];
		if (quest == null || quest.Type != quest_data.QuestType.Sequence) return "";
		if (_currentSubIndex >= quest.SubActions.Count) return "";
		return quest.SubActions[_currentSubIndex];
	}
}
