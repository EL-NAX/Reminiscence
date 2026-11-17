using Godot;
using System;

public partial class PlayerInteract : Node
{
	private Area2D interactionDetector;
	private AnimatedSprite2D interactIndicator;
	private Node2D currentTarget; 
	private bool _isShowing = false;

	public override void _Ready()
	{
		interactionDetector = GetParent().GetNodeOrNull<Area2D>("InteractionDetector");
		interactIndicator = GetParent().GetNodeOrNull<AnimatedSprite2D>("InteractIndicator");

		if (interactionDetector == null)
			GD.PrintErr("❌ InteractionDetector gak ketemu!");
		else
		{
			interactionDetector.AreaEntered += OnAreaEntered;
			interactionDetector.AreaExited += OnAreaExited;
		}

		if (interactIndicator != null) 
			interactIndicator.Visible = false;
	}

	public override void _Process(double delta)
	{
		if (currentTarget != null)
		{
			// Kalau itemnya ilang (udah diambil), sembunyiin logo F
			if (!IsInstanceValid(currentTarget))
			{
				HideIndicator();
				currentTarget = null;
				return;
			}

			// Cek apakah es udah di-collect
			if (currentTarget.HasMethod("PlayerCollect"))
			{
				var isCollected = currentTarget.Get("_collected").AsBool();
				if (isCollected)
				{
					HideIndicator();
					currentTarget = null;
					return;
				}
			}
		}

		if (Input.IsActionJustPressed("interact") && currentTarget != null)
		{
			// 1. Kalau ini ITEM (Es)
			if (currentTarget.HasMethod("PlayerCollect"))
			{
				currentTarget.Call("PlayerCollect");
				HideIndicator(); // Sembunyiin logo F pas dipencet
				currentTarget = null; // Reset target biar gak kepencet 2x
			}
			// 2. Kalau ini PEDANG
			else if (currentTarget.IsInGroup("IceSword"))
			{
				HideIndicator(); // Sembunyiin logo F pas dipencet
				
				// Panggil controller buat mulai QTE
				var controller = GetTree().GetFirstNodeInGroup("IceCave2Controller");
				if (controller != null && controller.HasMethod("StartSwordQTESequence"))
					controller.Call("StartSwordQTESequence");
					
				currentTarget = null; // Reset target biar gak manggil 2x
			}
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		var parent = area.GetParent();
		
		// Cek kalau ini ITEM (Es)
		if (parent != null && parent.HasMethod("PlayerCollect"))
		{
			var isCollected = parent.Get("_collected").AsBool();
			if (isCollected) return;

			currentTarget = parent as Node2D;
			ShowIndicator();
		}
		// Cek kalau ini PEDANG
		else if (parent != null && parent.IsInGroup("IceSword"))
		{
			currentTarget = parent as Node2D;
			ShowIndicator();
		}
	}

	private void OnAreaExited(Area2D area)
	{
		var parent = area.GetParent();

		if (parent == currentTarget)
		{
			HideIndicator();
			currentTarget = null;
		}
	}

	private void ShowIndicator()
	{
		if (interactIndicator != null && !_isShowing) 
		{
			_isShowing = true;
			interactIndicator.Visible = true;
			interactIndicator.Play("blink");
		}
	}

	private void HideIndicator()
	{
		if (interactIndicator != null && _isShowing) 
		{
			_isShowing = false;
			interactIndicator.Visible = false;
			interactIndicator.Stop();
		}
	}
}
