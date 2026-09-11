using Godot;
using System;

public partial class amari_ai : CharacterBody2D
{
	private CharacterBody2D player;
	private AnimatedSprite2D animatedSprite;
	
	[Export] private float followDistance = 80.0f;
	[Export] private float speed = 150.0f;
	[Export] public float Gravity = 1000.0f;
	
	private float facingDirection = 1.0f;
	
	public override void _Ready()
	{
		GD.Print("🔵 Amari: Script mulai!");
		
		player = GetParent().GetNode<CharacterBody2D>("Player");
		
		if (player == null)
		{
			GD.PrintErr("❌ Amari: Player TIDAK ditemukan!");
		}
		else
		{
			GD.Print($"✅ Amari: Player ditemukan! Nama={player.Name}");
		}
		
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (animatedSprite == null)
		{
			GD.PrintErr("❌ Amari: AnimatedSprite2D tidak ditemukan!");
		}
		else
		{
			GD.Print("✅ Amari: AnimatedSprite2D ditemukan!");
			animatedSprite.Play("idle");
		}
		
		FloorStopOnSlope = true;
		FloorMaxAngle = Mathf.DegToRad(45);
		FloorSnapLength = 2.0f;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (player == null) return;
		
		float dt = (float)delta;
		
		// Gravitasi
		if (!IsOnFloor())
		{
			Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
		}
		else
		{
			Velocity = new Vector2(Velocity.X, 0);
		}
		
		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
		
		if (direction.X != 0)
		{
			facingDirection = direction.X;
		}
		
		if (distance > followDistance)
		{
			Velocity = new Vector2(direction.X * speed, Velocity.Y);
			UpdateAnimation(true);
		}
		else
		{
			Velocity = new Vector2(0, Velocity.Y);
			UpdateAnimation(false);
		}
		
		MoveAndSlide();
	}
	
	private void UpdateAnimation(bool isMoving)
	{
		if (animatedSprite == null) return;
		
		if (facingDirection > 0)
			animatedSprite.FlipH = false;
		else if (facingDirection < 0)
			animatedSprite.FlipH = true;
		
		if (isMoving)
		{
			if (animatedSprite.Animation != "walk")
				animatedSprite.Play("walk");
		}
		else
		{
			if (animatedSprite.Animation != "idle")
				animatedSprite.Play("idle");
		}
	}
}
