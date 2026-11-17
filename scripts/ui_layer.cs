using Godot;

public partial class ui_layer : CanvasLayer
{
	private Control _menuUI;
	private Control _settingsPopup;
	private Control _diaryPopup;

	public override void _Ready()
	{
		_menuUI = GetNode<Control>("MenuUI");
		_settingsPopup = GetNode<Control>("SettingsPopup");
		_diaryPopup = GetNode<Control>("DiaryPopup");

		_settingsPopup.Visible = false;
		_diaryPopup.Visible = false;

		// MenuBos (satu s)
		GetNode<BaseButton>("MenuUI/MenuBos/NewGame").Pressed += OnNewGamePressed;
		GetNode<BaseButton>("MenuUI/MenuBos/Setting").Pressed += OnSettingPressed;
		GetNode<BaseButton>("SettingsPopup/Panel/CloseButton").Pressed += OnCloseSettingsPressed;
		
		GetNode<BaseButton>("MenuUI/MenuBos/Diary").Pressed += OnDiaryPressed;
		GetNode<BaseButton>("DiaryPopup/Panel/CloseButton").Pressed += OnCloseDiaryPressed;
	}

	private void OnNewGamePressed()
	{
		if (game_state.Instance != null)
		{
			game_state.Instance.ResetAll();
			GD.Print("[UiLayer] New Game - flag direset");
		}

		GetTree().ChangeSceneToFile("res://scenes/prologue/prologue.tscn");
	}

	private void OnSettingPressed()
	{
		_settingsPopup.Visible = true;
		_menuUI.MouseFilter = Control.MouseFilterEnum.Ignore;
	}

	private void OnCloseSettingsPressed()
	{
		_settingsPopup.Visible = false;
		_menuUI.MouseFilter = Control.MouseFilterEnum.Stop;
	}

	private void OnDiaryPressed()
	{
		_diaryPopup.Visible = true;
		_menuUI.MouseFilter = Control.MouseFilterEnum.Ignore;
	}

	private void OnCloseDiaryPressed()
	{
		_diaryPopup.Visible = false;
		_menuUI.MouseFilter = Control.MouseFilterEnum.Stop;
	}
}
