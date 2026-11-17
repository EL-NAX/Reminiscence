using Godot;

public partial class quest_box : CanvasLayer
{
	private Control _container;
	private Label _titleLabel;
	private Label _counterLabel;
	private Label _hintLabel;
	private HBoxContainer _keybindHint;
	private TextureRect _keyIcon;

	public override void _Ready()
	{
		_container = GetNode<Control>("Container");
		_titleLabel = GetNode<Label>("Container/TitleLabel");
		_counterLabel = GetNode<Label>("Container/CounterLabel");
		_hintLabel = GetNode<Label>("Container/HintLabel");
		_keybindHint = GetNode<HBoxContainer>("Container/KeybindHint");
		_keyIcon = GetNode<TextureRect>("Container/KeybindHint/KeyIcon");

		_container.Modulate = new Color(1, 1, 1, 0);
		_container.Visible = false;

		quest_manager.Instance.QuestStarted += OnQuestStarted;
		quest_manager.Instance.SubQuestCompleted += OnSubCompleted;
		quest_manager.Instance.QuestProgress += OnQuestProgress;
		quest_manager.Instance.QuestCompleted += OnQuestCompleted;
		quest_manager.Instance.AllQuestsCompleted += OnAllComplete;
	}

	private void OnQuestStarted(string title)
	{
		GD.Print($"[QuestBox] Quest started: {title}");
		_titleLabel.Text = title;
		_counterLabel.Text = "0/0";
		_hintLabel.Text = "";
		_keybindHint.Visible = false;
		FadeIn();
	}

	private void OnSubCompleted(int current, int total)
	{
		GD.Print($"[QuestBox] Sub completed: {current}/{total}");
		_counterLabel.Text = $"{current}/{total}";

		var quest = quest_manager.Instance.GetCurrentQuest();
		if (quest == null) return;

		if (current < quest.SubQuests.Count)
		{
			_hintLabel.Text = quest.SubQuests[current];

			if (quest.KeyIcons.Count > current && quest.KeyIcons[current] != null)
			{
				_keyIcon.Texture = quest.KeyIcons[current];
				_keybindHint.Visible = true;
			}
			else _keybindHint.Visible = false;
		}
		else
		{
			_hintLabel.Text = "✓ Done";
			_keybindHint.Visible = false;
		}
	}

	private void OnQuestProgress(int current, int target, bool usePercent)
	{
		if (usePercent)
			_counterLabel.Text = $"{Mathf.RoundToInt((float)current / target * 100)}%";
		else
			_counterLabel.Text = $"{current}/{target}";

		var quest = quest_manager.Instance.GetCurrentQuest();
		if (quest == null) return;

		// Tampilkan sub quest pertama (index 0) untuk Collect
		if (quest.SubQuests.Count > 0)
		{
			_hintLabel.Text = quest.SubQuests[0];
		}

		// Tampilkan key icon pertama (index 0)
		if (quest.KeyIcons.Count > 0 && quest.KeyIcons[0] != null)
		{
			_keyIcon.Texture = quest.KeyIcons[0];
			_keybindHint.Visible = true;
		}
		else
		{
			_keybindHint.Visible = false;
		}

		// Kalau udah selesai
		if (current >= target)
		{
			_hintLabel.Text = "✓ Done";
			_keybindHint.Visible = false;
		}
	}

	private void OnQuestCompleted(string title)
	{
		GD.Print($"[QuestBox] Quest completed: {title}");
		FadeOut();
	}

	private void OnAllComplete() => _container.Visible = false;

	private async void FadeIn()
	{
		_container.Visible = true;
		_container.Modulate = new Color(1, 1, 1, 0);
		var tween = CreateTween();
		tween.TweenProperty(_container, "modulate:a", 1.0f, 0.4f);
		await ToSignal(tween, "finished");
	}

	private async void FadeOut()
	{
		var tween = CreateTween();
		tween.TweenProperty(_container, "modulate:a", 0.0f, 0.4f);
		await ToSignal(tween, "finished");
		_container.Visible = false;
	}
}
