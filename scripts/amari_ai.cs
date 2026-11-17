using Godot;

public partial class amari_ai : CharacterBody2D
{
	private CharacterBody2D player;
	private AnimatedSprite2D animatedSprite;

	[Export] private float followDistance = 80.0f;
	[Export] private float normalSpeed = 200.0f; 
	[Export] private float dragSpeed = 100.0f;   
	[Export] public float Gravity = 1000.0f;
	[Export] public float PauseDuration = 1.0f;
	[Export] public float IcePauseRadius = 80f;

	private float facingDirection = 1.0f;

	public bool FollowPlayer { get; set; } = false;
	public bool AutoWalkMode { get; set; } = false;
	public bool HasExited { get; set; } = false;
	public Vector2 AutoWalkTarget { get; set; }
	public float AutoWalkSpeed { get; set; } = 500f;
	
	public bool IsInCutscene { get; set; } = false; 

	private bool _isJumping = false;

	private Godot.Collections.Array<Area2D> _visitedIce = new();
	private bool _pausingAtIce = false;
	private float _pauseTimer = 0f;

	private bool _isPlayingPickup = false; 

	public bool IsWalkingDone => !AutoWalkMode;
	public bool IsJumping => _isJumping;

	public override void _Ready()
	{
		AddToGroup("Amari");
		player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
		animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

		if (player != null)
		{
			AddCollisionExceptionWith(player);
			GD.Print("[Amari] ✓ Collision dengan Player di-disable");
		}

		var currentScene = GetTree().CurrentScene;
		bool bypassBorders = false;

		if (currentScene != null)
		{
			string scenePath = currentScene.SceneFilePath.ToLower();
			GD.Print($"[Amari] Scene: '{scenePath}'");

			if (scenePath.Contains("ice_cave_1"))
			{
				bypassBorders = true;
				GD.Print("[Amari] ✓ Bypass borders ON");
			}
		}

		if (bypassBorders)
		{
			var borders = GetTree().GetNodesInGroup("Border");
			foreach (var node in borders)
			{
				if (node is PhysicsBody2D pb)
				{
					AddCollisionExceptionWith(pb);
					GD.Print($"  → Bypass: {pb.Name}");
				}
			}
		}

		var iceCount = GetTree().GetNodesInGroup("IcePickup").Count;
		GD.Print($"[Amari] Ada {iceCount} es di scene");

		if (animatedSprite != null && animatedSprite.SpriteFrames != null)
		{
			var animNames = animatedSprite.SpriteFrames.GetAnimationNames();
			GD.Print($"[Amari] Animasi: {string.Join(", ", animNames)}");

			if (animatedSprite.SpriteFrames.HasAnimation("idleWithKeranjang"))
			{
				animatedSprite.Play("idleWithKeranjang");
				animatedSprite.Offset = new Vector2(0, 0); 
			}
			else if (animatedSprite.SpriteFrames.HasAnimation("idle"))
			{
				animatedSprite.Play("idle");
				animatedSprite.Offset = new Vector2(0, 0);
			}
		}

		FloorStopOnSlope = true;
		FloorMaxAngle = Mathf.DegToRad(45);
		FloorSnapLength = 2.0f;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (HasExited) return;

		float dt = (float)delta;

		if (_isJumping)
		{
			Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
			
			if (IsOnFloor() && Velocity.Y >= 0) 
			{
				_isJumping = false;
				GD.Print("[Amari] Mendarat setelah loncat paksa!");
			}

			MoveAndSlide();
			return; 
		}

		if (!IsOnFloor())
			Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
		else
			Velocity = new Vector2(Velocity.X, 0);

		// AUTO WALK
		if (AutoWalkMode)
		{
			if (_pausingAtIce)
			{
				Velocity = new Vector2(0, Velocity.Y);
				
				if (!_isPlayingPickup)
					UpdateAnimation(false);
					
				_pauseTimer -= dt;

				if (_pauseTimer <= 0)
				{
					_pausingAtIce = false;
					_isPlayingPickup = false; 
					GD.Print("[Amari] Lanjut jalan");
				}

				MoveAndSlide();
				return;
			}

			CheckIceProximity();

			if (_pausingAtIce)
			{
				MoveAndSlide();
				return;
			}

			float dist = Mathf.Abs(GlobalPosition.X - AutoWalkTarget.X);
			if (dist > 15f)
			{
				float dir = Mathf.Sign(AutoWalkTarget.X - GlobalPosition.X);
				Velocity = new Vector2(dir * AutoWalkSpeed, Velocity.Y);
				if (dir != 0) facingDirection = dir;
				UpdateAnimation(true);
			}
			else
			{
				Velocity = new Vector2(0, Velocity.Y);
				AutoWalkMode = false;
				UpdateAnimation(false);
				GD.Print("[Amari] Sampai target!");

				var controller = GetTree().GetFirstNodeInGroup("IceCave1Controller");
				if (controller != null && controller.HasMethod("OnAmariReachedEnd"))
					controller.Call("OnAmariReachedEnd");
			}
			MoveAndSlide();
			return;
		}

		// FOLLOW PLAYER
		if (FollowPlayer && player != null)
		{
			float dist = Mathf.Abs(player.GlobalPosition.X - GlobalPosition.X);
			float dirX = Mathf.Sign(player.GlobalPosition.X - GlobalPosition.X);

			if (dirX != 0) facingDirection = dirX;

			bool isPlayerDragging = false;
			if (player is player p)
				isPlayerDragging = p.IsDraggingSword;

			float currentSpeed = isPlayerDragging ? dragSpeed : normalSpeed;

			if (dist > followDistance)
			{
				Velocity = new Vector2(dirX * currentSpeed, Velocity.Y);
				UpdateAnimation(true);
			}
			else
			{
				Velocity = new Vector2(0, Velocity.Y);
				UpdateAnimation(false);
			}
			MoveAndSlide();
			return;
		}

		// IDLE
		Velocity = new Vector2(0, Velocity.Y);
		UpdateAnimation(false);
		MoveAndSlide();
	}

	private void CheckIceProximity()
	{
		if (_isPlayingPickup) return; 

		var iceList = GetTree().GetNodesInGroup("IcePickup");

		foreach (var node in iceList)
		{
			if (node is Area2D ice && IsInstanceValid(ice))
			{
				if (_visitedIce.Contains(ice)) continue;

				float dist = GlobalPosition.DistanceTo(ice.GlobalPosition);
				if (dist < IcePauseRadius)
				{
					_visitedIce.Add(ice);
					_pausingAtIce = true;
					_isPlayingPickup = true; 
					_pauseTimer = PauseDuration;
					GD.Print($"[Amari] Pause di es (dist: {dist:F1})");
					
					if (animatedSprite != null)
					{
						if (animatedSprite.SpriteFrames.HasAnimation("pickup"))
						{
							animatedSprite.Play("pickup");
						}
						else if (animatedSprite.SpriteFrames.HasAnimation("interact"))
						{
							animatedSprite.Play("interact");
						}
						animatedSprite.Offset = new Vector2(0, 8); 
					}
					
					return;
				}
			}
		}
	}

	private void UpdateAnimation(bool isMoving)
	{
		if (animatedSprite == null) return;

		if (IsInCutscene) return;
		if (_pausingAtIce || _isPlayingPickup) return;

		if (facingDirection > 0) animatedSprite.FlipH = false;
		else if (facingDirection < 0) animatedSprite.FlipH = true;

		if (isMoving)
		{
			if (animatedSprite.SpriteFrames != null && animatedSprite.SpriteFrames.HasAnimation("walkWithKeranjang"))
			{
				if (animatedSprite.Animation != "walkWithKeranjang") 
				{
					animatedSprite.Play("walkWithKeranjang");
					animatedSprite.Offset = new Vector2(0, 10); 
				}
			}
			else if (animatedSprite.Animation != "walk") 
			{
				animatedSprite.Play("walk");
				animatedSprite.Offset = new Vector2(0, 8);
			}
		}
		else
		{
			if (animatedSprite.SpriteFrames != null && animatedSprite.SpriteFrames.HasAnimation("idleWithKeranjang"))
			{
				if (animatedSprite.Animation != "idleWithKeranjang") 
				{
					animatedSprite.Play("idleWithKeranjang");
					animatedSprite.Offset = new Vector2(0, 0); 
				}
			}
			else if (animatedSprite.Animation != "idle") 
			{
				animatedSprite.Play("idle");
				animatedSprite.Offset = new Vector2(0, 0);
			}
		}
	}

	public async void FadeOutAndExit()
	{
		if (HasExited) return;
		HasExited = true;
		Velocity = Vector2.Zero;

		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0.0f, 0.6f);
		await ToSignal(tween, "finished");

		Visible = false;
		GD.Print("[Amari] Hidden setelah fade out");
	}

	// ===== WRAPPER =====
	public void StartFollowing() { FollowPlayer = true; AutoWalkMode = false; }
	public void SetIdle() { FollowPlayer = false; AutoWalkMode = false; Velocity = Vector2.Zero; UpdateAnimation(false); }
	public void WalkToX(float targetX) => WalkToX(targetX, 70f);
	public void WalkToX(float targetX, float walkSpeed) { AutoWalkMode = true; FollowPlayer = false; AutoWalkTarget = new Vector2(targetX, GlobalPosition.Y); AutoWalkSpeed = walkSpeed; }
	
	// FUNGSI INI YANG DIUBAH BIAR BISA TERIMA SPEED
	public void WalkToPosition(Vector2 target, float walkSpeed = 100f) 
	{ 
		AutoWalkMode = true; 
		FollowPlayer = false; 
		AutoWalkTarget = target; 
		AutoWalkSpeed = walkSpeed; 
	}
	
	public void JumpToPosition(Vector2 pos) { GlobalPosition = pos; Velocity = Vector2.Zero; }
	
	public void JumpToPosition(Vector2 target, float jumpForce)
	{
		AutoWalkMode = false; FollowPlayer = false;
		float dir = Mathf.Sign(target.X - GlobalPosition.X); if (dir == 0) dir = 1;
		float distX = Mathf.Abs(target.X - GlobalPosition.X);
		
		float airTime = (2f * jumpForce) / Gravity;
		float speedX = distX / Mathf.Max(airTime, 0.1f);
		
		Velocity = new Vector2(dir * speedX, -jumpForce);
		_isJumping = true; 
		facingDirection = dir;
		
		GD.Print($"[Amari] JumpToPosition: X={speedX}, Y={-jumpForce}");
	}

	public float GetFacingDirection() => facingDirection;
	public void SetFacingDirection(float dir) => facingDirection = dir;
}
