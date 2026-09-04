using Godot;

public partial class Player : CharacterBody2D
{
	// NODE REFERENCES
	private AnimatedSprite2D _animatedSprite;

	// MOVEMENT
	[Export] public float Speed = 200.0f;
	[Export] public float JumpForce = 400.0f;
	[Export] public float Gravity = 1000.0f;

	// DASH
	[Export] public float DashSpeed = 600.0f;
	[Export] public float DashDuration = 0.15f;
	[Export] public float DashCooldown = 0.5f;

	private float dashTimer = 0.0f;
	private float dashCooldownTimer = 0.0f;
	private bool isDashing = false;

	// Arah terakhir Player bergerak
	private float facingDirection = 1.0f;

	// ATTACK
	private bool isAttacking = false;

	// JUMP BUFFER & COYOTE TIME
	private float jumpBufferTimer = 0.0f;
	private float coyoteTimer = 0.0f;
	private bool isJumpHeld = false;

	public override void _Ready()
	{
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// PENGATURAN ANTI LICIN
		FloorStopOnSlope = true;
		FloorMaxAngle = Mathf.DegToRad(45);
		FloorSnapLength = 2.0f;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (dashCooldownTimer > 0) dashCooldownTimer -= dt;
		if (jumpBufferTimer > 0) jumpBufferTimer -= dt;
		if (coyoteTimer > 0) coyoteTimer -= dt;

		if (isDashing)
		{
			dashTimer -= dt;
			Velocity = new Vector2(facingDirection * DashSpeed, 0);
			MoveAndSlide();
			UpdateAnimations(facingDirection);

			if (dashTimer <= 0) isDashing = false;
			return;
		}

		if (!IsOnFloor())
		{
			Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
		}
		else
		{
			Velocity = new Vector2(Velocity.X, 0);
			coyoteTimer = 0.1f;
		}

		float direction = Input.GetAxis("move_left", "move_right");
		if (direction != 0) facingDirection = direction;

		Velocity = new Vector2(direction * Speed, Velocity.Y);

		if (Input.IsActionJustPressed("jump"))
		{
			jumpBufferTimer = 0.1f;
			isJumpHeld = true;
		}

		if (Input.IsActionJustReleased("jump"))
		{
			isJumpHeld = false;
			if (Velocity.Y < 0)
			{
				Velocity = new Vector2(Velocity.X, Velocity.Y * 0.5f);
			}
		}

		if (jumpBufferTimer > 0 && (IsOnFloor() || coyoteTimer > 0))
		{
			Velocity = new Vector2(Velocity.X, -JumpForce);
			jumpBufferTimer = 0;
			coyoteTimer = 0;
		}

		// HANYA BISA DASH SAAT DI TANAH
		if (Input.IsActionJustPressed("dash") && dashCooldownTimer <= 0)
		{
			if (IsOnFloor()) 
			{
				StartDash();
			}
		}

		if (Input.IsActionJustPressed("attack") && !isAttacking)
		{
			Attack();
		}

		MoveAndSlide();
		UpdateAnimations(direction);
	}

	private void UpdateAnimations(float direction)
	{
		if (facingDirection > 0) _animatedSprite.FlipH = false;
		else if (facingDirection < 0) _animatedSprite.FlipH = true;

		if (isDashing)
		{
			_animatedSprite.Play("walk");
		}
		else if (isAttacking)
		{
			_animatedSprite.Play("attack");
		}
		else if (!IsOnFloor())
		{
			_animatedSprite.Play("jump");
		}
		else if (direction != 0)
		{
			_animatedSprite.Play("walk");
		}
		else
		{
			_animatedSprite.Play("idle");
		}
	}

	private void StartDash()
	{
		isDashing = true;
		dashTimer = DashDuration;
		dashCooldownTimer = DashCooldown;
	}

	private void Attack()
	{
		isAttacking = true;
		GetTree().CreateTimer(0.2).Timeout += FinishAttack;
	}

	private void FinishAttack()
	{
		isAttacking = false;
	}
}
