using Godot;

public partial class keybind_hud : CanvasLayer
{
	private Label _keyLeftRight;
	private Label _keySpace;
	private Label _keyF;
	private bool _isVisible = false;

	public override void _Ready()
	{
		// Taruh di layer tinggi biar selalu di atas
		Layer = 80; 
		
		// Cari node Label
		_keyLeftRight = GetNodeOrNull<Label>("KeyLeftRight");
		_keySpace = GetNodeOrNull<Label>("KeySpace");
		_keyF = GetNodeOrNull<Label>("KeyF");

		// Sembunyiin di awal
		SetVisible(false);
	}

	public override void _Process(double delta)
	{
		// Cek apakah sedang ada dialog aktif
		var dialogUI = GetTree().GetFirstNodeInGroup("DialogUI") as CanvasLayer;
		bool isDialogActive = false;

		if (dialogUI != null)
		{
			// Cek apakah panel dialognya lagi kelihatan
			var panel = dialogUI.GetNodeOrNull<Control>("Panel");
			if (panel != null && panel.Visible)
				isDialogActive = true;
		}

		// Kalau lagi dialog, sembunyiin. Kalau gak, munculin (sesuai status _isVisible)
		if (isDialogActive && IsVisible())
		{
			SetVisible(false, false); // Sembunyiin tanpa ngubah status _isVisible asli
		}
		else if (!isDialogActive && _isVisible && !IsVisible())
		{
			SetVisible(true, false); // Munculin lagi
		}
	}

	public void SetVisible(bool visible, bool updateState = true)
	{
		if (updateState) _isVisible = visible;

		if (_keyLeftRight != null) _keyLeftRight.Visible = visible;
		if (_keySpace != null) _keySpace.Visible = visible;
		if (_keyF != null) _keyF.Visible = visible;
	}

	// ===== FUNGSI BARU BUAT BUKA HUD =====
	public void UnlockHUD()
	{
		SetVisible(true);
		GD.Print("[KeybindHUD] HUD Keybind di-unlock!");
	}
	// ======================================
}
