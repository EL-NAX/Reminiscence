using Godot;

public partial class player : CharacterBody2D
{
	private AnimatedSprite2D _animatedSprite;

	// MOVEMENT
	[Export] public float Speed = 200.0f;
	[Export] public float DragSpeed = 100.0f; 
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

	// INTERACT
	private bool isInteracting = false;
	[Export] public float InteractDuration = 0.5f;

	// MOVEMENT LOCK
	public bool IsMovementLocked { get; set; } = false;

	// ===== TAMBAHAN BARU =====
	public bool IsDraggingSword { get; set; } = false;
	public bool IsFalling { get; set; } = false;

	// ===== HEALTH & STAMINA =====
	[Export] public float MaxHealth = 100f;
	public float CurrentHealth { get; set; } = 100f;
	[Export] public float MaxStamina = 50f;
	public float CurrentStamina { get; set; } = 50f;
	[Export] public float StaminaCostJump = 2f;
	[Export] public float StaminaRegenRate = 10f; 
	[Export] public ProgressBar HealthBar;
	[Export] public ProgressBar StaminaBar;

	public bool CanMove { get; set; } = true;
	public bool CanJump { get; set; } = true; 
	public bool CanDash { get; set; } = false;
	public bool CanAttack { get; set; } = false;

	private bool _autoWalking = false;
	private float _autoWalkTargetX = 0f;
	private float _autoWalkSpeed = 100f;
	public bool IsAutoWalking => _autoWalking;

	// ===== TAMBAHAN BUAT AUDIO =====
	private RayCast2D _groundRay;
	private AudioStreamPlayer _sfxStepGrass;
	private AudioStreamPlayer _sfxStepStone;
	private AudioStreamPlayer _sfxStepSnow;
	private AudioStreamPlayer _sfxJump;
	private AudioStreamPlayer _sfxLand;
	
	private float _stepTimer = 0f;
	private float _stepInterval = 0.5f; // Jeda antar langkah kaki (diperlama biar gak cepet)
	private bool _wasOnFloor = true;
	// ================================

	public override void _Ready()
	{
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		
		_groundRay = GetNodeOrNull<RayCast2D>("GroundRay");
		_sfxStepGrass = GetNodeOrNull<AudioStreamPlayer>("SFX_StepGrass");
		_sfxStepStone = GetNodeOrNull<AudioStreamPlayer>("SFX_StepStone");
		_sfxStepSnow = GetNodeOrNull<AudioStreamPlayer>("SFX_StepSnow");
		_sfxJump = GetNodeOrNull<AudioStreamPlayer>("SFX_Jump");
		_sfxLand = GetNodeOrNull<AudioStreamPlayer>("SFX_Land");
		
		FloorStopOnSlope = true;
		FloorMaxAngle = Mathf.DegToRad(45);
		FloorSnapLength = 2.0f;

		if (_animatedSprite != null)
		{
			_animatedSprite.Play("idle");
			_animatedSprite.Frame = 0; 
		}
		
		CurrentHealth = MaxHealth;
		CurrentStamina = MaxStamina;
	}

	public override void _Process(double delta)
	{
		if (CurrentStamina < MaxStamina)
		{
			CurrentStamina += StaminaRegenRate * (float)delta;
			CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina);
		}
		if (HealthBar != null) HealthBar.Value = CurrentHealth;
		if (StaminaBar != null) StaminaBar.Value = CurrentStamina;

		// Logika suara langkah
		if (IsOnFloor() && Velocity.X != 0 && !IsMovementLocked && !_autoWalking)
		{
			_stepTimer += (float)delta;
			if (_stepTimer >= _stepInterval)
			{
				PlayFootstepSound();
				_stepTimer = 0f;
			}
		}
		else
		{
			_stepTimer = _stepInterval; 
		}
		
		// Deteksi mendarat
		if (IsOnFloor() && !_wasOnFloor)
		{
			if (_sfxLand != null) _sfxLand.Play();
		}
		_wasOnFloor = IsOnFloor();
	}

	// =========================================================
	// LOGIKA AUDIO: PAKAI CUSTOM DATA TILEMAP
	// =========================================================
	private void PlayFootstepSound()
	{
		if (_groundRay == null) return;

		_groundRay.ForceRaycastUpdate();
		if (_groundRay.IsColliding())
		{
			var collider = _groundRay.GetCollider();
			
			// Cek apakah yang ditabrak adalah TileMap
			if (collider is TileMap tileMap)
			{
				// Ambil posisi lokal dari tabrakan
				Vector2I cellPos = tileMap.LocalToMap(tileMap.ToLocal(_groundRay.GetCollisionPoint()));
				
				// Ambil data tile di posisi itu (pakai layer 0)
				TileData tileData = tileMap.GetCellTileData(0, cellPos);
				
				if (tileData != null)
				{
					if (tileData.GetCustomData("salju").AsBool())
					{
						if (_sfxStepSnow != null) _sfxStepSnow.Play();
						return;
					}
					else if (tileData.GetCustomData("batu").AsBool())
					{
						if (_sfxStepStone != null) _sfxStepStone.Play();
						return;
					}
					else if (tileData.GetCustomData("rumput").AsBool())
					{
						if (_sfxStepGrass != null) _sfxStepGrass.Play();
						return;
					}
				}
			}
			
			// Fallback: Kalau gak ada custom data, pakai nama node
			if (collider is Node2D node)
			{
				string name = node.Name.ToString().ToLower();
				if (name.Contains("snow") || name.Contains("es") || name.Contains("ice"))
				{
					if (_sfxStepSnow != null) _sfxStepSnow.Play();
				}
				else if (name.Contains("stone") || name.Contains("batu") || name.Contains("rock"))
				{
					if (_sfxStepStone != null) _sfxStepStone.Play();
				}
				else
				{
					if (_sfxStepGrass != null) _sfxStepGrass.Play();
				}
			}
		}
		else
		{
			if (_sfxStepGrass != null) _sfxStepGrass.Play();
		}
	}

	public void TakeDamage(float amount)
	{
		CurrentHealth -= amount;
		CurrentHealth = Mathf.Max(CurrentHealth, 0);
		if (CurrentHealth <= 0)
		{
			GD.Print("Player Mati!");
		}
	}

	public bool UseStamina(float amount)
	{
		if (CurrentStamina >= amount)
		{
			CurrentStamina -= amount;
			return true;
		}
		return false;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (dashCooldownTimer > 0) dashCooldownTimer -= dt;
		if (attackCooldown > 0) attackCooldown -= dt;
		if (jumpBufferTimer > 0) jumpBufferTimer -= dt;
		if (coyoteTimer > 0) coyoteTimer -= dt;

		// === AUTO WALK ===
		if (_autoWalking)
		{
			float dist = Mathf.Abs(GlobalPosition.X - _autoWalkTargetX);
			if (dist > 5f)
			{
				float dir = Mathf.Sign(_autoWalkTargetX - GlobalPosition.X);
				Velocity = new Vector2(dir * _autoWalkSpeed, Velocity.Y);
				if (dir != 0) facingDirection = dir;
			}
			else
			{
				Velocity = new Vector2(0, Velocity.Y);
				_autoWalking = false;
			}

			if (!IsOnFloor())
				Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);

			MoveAndSlide();
			UpdateAnimations(Velocity.X);
			return;
		}

		// === MOVEMENT LOCK ===
		if (IsMovementLocked)
		{
			Velocity = new Vector2(0, Velocity.Y);
			if (!IsOnFloor())
				Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
			MoveAndSlide();
			if (!IsDraggingSword && !IsFalling) UpdateAnimations(0);
			return;
		}

		// === FALLING STATE ===
		if (IsFalling)
		{
			Velocity = new Vector2(0, Velocity.Y);
			if (!IsOnFloor())
				Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
			MoveAndSlide();
			return; 
		}

		// === INTERACT LOCK ===
		if (isInteracting)
		{
			Velocity = new Vector2(0, Velocity.Y);
			if (!IsOnFloor())
				Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
			MoveAndSlide();
			UpdateAnimations(0);
			return;
		}

		// === DASH ===
		if (isDashing)
		{
			dashTimer -= dt;
			Velocity = new Vector2(facingDirection * DashSpeed, 0);
			MoveAndSlide();
			UpdateAnimations(facingDirection);
			if (dashTimer <= 0) isDashing = false;
			return;
		}

		// === GRAVITY ===
		if (!IsOnFloor())
			Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
		else
		{
			Velocity = new Vector2(Velocity.X, 0);
			coyoteTimer = 0.1f;
		}

		// === MOVEMENT ===
		float direction = 0f;
		if (CanMove)
		{
			direction = Input.GetAxis("move_left", "move_right");
			if (direction != 0) facingDirection = direction;
		}

		float currentSpeed = IsDraggingSword ? DragSpeed : Speed;
		Velocity = new Vector2(direction * currentSpeed, Velocity.Y);

		// KUNCI INPUT SAAT NYERET PEDANG
		if (!IsDraggingSword) 
		{
			if (CanJump)
			{
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
					if (UseStamina(StaminaCostJump))
					{
						Velocity = new Vector2(Velocity.X, -JumpForce);
						jumpBufferTimer = 0;
						coyoteTimer = 0;
						
						if (_sfxJump != null) _sfxJump.Play();
					}
				}
			}

			if (CanDash)
			{
				if (Input.IsActionJustPressed("dash") && dashCooldownTimer <= 0)
				{
					if (IsOnFloor()) StartDash();
				}
			}

			if (CanAttack)
			{
				if (Input.IsActionJustPressed("attack") && !isAttacking && attackCooldown <= 0 && direction == 0)
					Attack();
			}
		}

		MoveAndSlide();
		UpdateAnimations(direction);
	}

	private void UpdateAnimations(float direction)
	{
		if (_animatedSprite == null) return;
		if (IsMovementLocked) return;

		if (facingDirection > 0) _animatedSprite.FlipH = false;
		else if (facingDirection < 0) _animatedSprite.FlipH = true;

		if (IsDraggingSword)
		{
			if (direction != 0)
				_animatedSprite.Play("nyeretPedang");
			else
				_animatedSprite.Play("idleWithSword");
			return; 
		}

		if (isInteracting) _animatedSprite.Play("interact");
		else if (isDashing) _animatedSprite.Play("walk");
		else if (isAttacking) _animatedSprite.Play("attack");
		else if (!IsOnFloor()) _animatedSprite.Play("jump"); 
		else if (direction != 0) _animatedSprite.Play("walk");
		else _animatedSprite.Play("idle");
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
		GetTree().CreateTimer(AttackDuration).Timeout += () => { isAttacking = false; };
	}

	public void PlayInteractAnimation()
	{
		if (!isInteracting)
		{
			isInteracting = true;
			Velocity = Vector2.Zero;

			if (_animatedSprite != null)
				_animatedSprite.Play("interact");

			GetTree().CreateTimer(InteractDuration).Timeout += () => { isInteracting = false; };
		}
	}

	public bool IsInteracting() => isInteracting;

	// ===== AUTO WALK =====
	public void AutoWalkToX(float targetX, float walkSpeed = 100f)
	{
		_autoWalking = true;
		_autoWalkTargetX = targetX;
		_autoWalkSpeed = walkSpeed;
		isDashing = false;
		isAttacking = false;
		isInteracting = false;
	}

	public void StopAutoWalk()
	{
		_autoWalking = false;
		Velocity = new Vector2(0, Velocity.Y);
	}

	// ===== ABILITY UNLOCK METHODS =====
	public void UnlockJump() { CanJump = true; }
	public void UnlockDash() { CanDash = true; }
	public void UnlockAttack() { CanAttack = true; }
	public void UnlockAll() { CanJump = true; CanDash = true; CanAttack = true; }
}
