using Godot;

public partial class player : CharacterBody2D
{
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

	private float facingDirection = 1.0f;

	// ATTACK
	private bool isAttacking = false;
	private float attackCooldown = 0.0f;
	[Export] public float AttackDuration = 0.3f;
	[Export] public float AttackCooldownDuration = 0.6f;

	// JUMP BUFFER & COYOTE TIME
	private float jumpBufferTimer = 0.0f;
	private float coyoteTimer = 0.0f;
	private bool isJumpHeld = false;

	// ===== TAMBAHAN: INTERACT =====
	private bool isInteracting = false;
	[Export] public float InteractDuration = 0.5f;
	// ===== END TAMBAHAN =====

	public override void _Ready()
	{
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		FloorStopOnSlope = true;
		FloorMaxAngle = Mathf.DegToRad(45);
		FloorSnapLength = 2.0f;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (dashCooldownTimer > 0) dashCooldownTimer -= dt;
		if (attackCooldown > 0) attackCooldown -= dt;
		if (jumpBufferTimer > 0) jumpBufferTimer -= dt;
		if (coyoteTimer > 0) coyoteTimer -= dt;

		// ===== TAMBAHAN: Cegah movement saat interact =====
		if (isInteracting)
		{
			Velocity = new Vector2(0, Velocity.Y);
			if (!IsOnFloor())
			{
				Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
			}
			MoveAndSlide();
			UpdateAnimations(0);
			return;
		}
		// ===== END TAMBAHAN =====

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

		// Jump Logic
		if (Input.IsActionJustPressed("jump"))
		{
			jumpBufferTimer = 0.1f;
			isJumpHeld = true;
		}
		if (Input.IsActionJustReleased("jump"))
		{
			isJumpHeld = false;
			if (Velocity.Y < 0) Velocity = new Vector2(Velocity.X, Velocity.Y * 0.5f);
		}
		if (jumpBufferTimer > 0 && (IsOnFloor() || coyoteTimer > 0))
		{
			Velocity = new Vector2(Velocity.X, -JumpForce);
			jumpBufferTimer = 0;
			coyoteTimer = 0;
		}

		// Dash Logic (Hanya di tanah)
		if (Input.IsActionJustPressed("dash") && dashCooldownTimer <= 0)
		{
			if (IsOnFloor()) StartDash();
		}

		// ===== ATTACK LOGIC (Hanya bisa saat DIAM / IDLE) =====
		// direction == 0 artinya tidak menekan A atau D
		if (Input.IsActionJustPressed("attack") && !isAttacking && attackCooldown <= 0 && direction == 0)
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

		// PRIORITAS: Interact -> Dash -> Attack -> Jump -> Walk -> Idle
		// ===== TAMBAHAN: Cek interact paling atas =====
		if (isInteracting)
		{
			_animatedSprite.Play("interact");
		}
		// ===== END TAMBAHAN =====
		else if (isDashing)
		{
			_animatedSprite.Play("walk"); // Ganti ke animasi dash kalau ada
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
		// ===== PERBAIKAN: baris else if () yang kosong dihapus =====
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
		attackCooldown = AttackCooldownDuration;
		GetTree().CreateTimer(AttackDuration).Timeout += () =>
		{
			isAttacking = false;
		};
	}

	// ===== TAMBAHAN: Method Interact =====
	public void PlayInteractAnimation()
	{
		if (!isInteracting)
		{
			isInteracting = true;
			Velocity = Vector2.Zero;

			GetTree().CreateTimer(InteractDuration).Timeout += () =>
			{
				isInteracting = false;
			};
		}
	}

	public bool IsInteracting()
	{
		return isInteracting;
	}
	// ===== END TAMBAHAN =====
}
