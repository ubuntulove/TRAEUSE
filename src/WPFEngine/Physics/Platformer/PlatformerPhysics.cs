using System;
using System.Collections.Generic;
using WPFEngine.ECS;
using WPFEngine.Math;
using WPFEngine.Scene;

namespace WPFEngine.Physics.Platformer
{
    [Flags]
    public enum CharacterFlags
    {
        None = 0,
        Grounded = 1 << 0,
        Airborne = 1 << 1,
        Crouching = 1 << 2,
        Prone = 1 << 3,
        Running = 1 << 4,
        Sliding = 1 << 5,
        OnIce = 1 << 6,
        OnMovingPlatform = 1 << 7,
        OnOneWayPlatform = 1 << 8,
        OnBouncySurface = 1 << 9,
        OnCrumblePlatform = 1 << 10,
        TouchingCeiling = 1 << 11,
        TouchingLeftWall = 1 << 12,
        TouchingRightWall = 1 << 13,
        OnLadder = 1 << 14,
        CanClimbLedge = 1 << 15,
        WasGrounded = 1 << 16,
        IsSleeping = 1 << 17,
        LockMovement = 1 << 18,
        LockRotation = 1 << 19
    }

    public enum CharacterState
    {
        Idle,
        Walking,
        Running,
        Crouching,
        Prone,
        Jumping,
        Falling,
        DoubleJumping,
        WallSliding,
        WallJumping,
        LedgeHanging,
        LedgeClimbing,
        LadderClimbing,
        Sliding,
        Landing,
        Bouncing,
        Frozen
    }

    public enum MovementState
    {
        Grounded,
        Airborne,
        Climbing
    }

    public enum JumpState
    {
        None,
        Charging,
        Jumping,
        Falling,
        Apex
    }

    public enum WallJumpDirection
    {
        Left,
        Right
    }

    public enum GroundType
    {
        Normal,
        Ice,
        Bouncy,
        Crumble,
        OneWay,
        Ladder,
        Moving
    }

    public class PlatformerPhysicsSettings
    {
        public float Gravity = -25f;
        public float MaxFallSpeed = -30f;
        public float FallMultiplier = 2.5f;
        public float LowJumpMultiplier = 2.0f;
        public float ApexGravityMultiplier = 0.5f;
        public float ApexThreshold = 5f;
        public float WalkSpeed = 5f;
        public float RunSpeed = 10f;
        public float SprintSpeed = 15f;
        public float SprintThreshold = 0.8f;
        public float Acceleration = 50f;
        public float Deceleration = 50f;
        public float AirAcceleration = 30f;
        public float AirDeceleration = 20f;
        public float AirControlPercentage = 0.8f;
        public float GroundFriction = 0.9f;
        public float IceFriction = 0.02f;
        public float CrouchSpeedMultiplier = 0.5f;
        public float ProneSpeedMultiplier = 0.2f;
        public float JumpForce = 12f;
        public float DoubleJumpForce = 10f;
        public float WallJumpForce = 10f;
        public float WallJumpHorizontalForce = 8f;
        public float WallSlideSpeed = 3f;
        public float LedgeGrabRange = 0.5f;
        public float LedgeClimbSpeed = 5f;
        public float LadderSpeed = 5f;
        public float LadderJumpForce = 8f;
        public float CoyoteTime = 0.1f;
        public float JumpBufferTime = 0.15f;
        public float BhopSpeedGain = 1.5f;
        public float BhopMaxSpeedGain = 8f;
        public float BhopWindowTime = 0.2f;
        public float BounceMultiplier = 1.5f;
        public float CrumbleTime = 1.5f;
        public float SlopeAngleLimit = 45f;
        public float SlopeAcceleration = 20f;
        public float CrouchHeightReduction = 0.5f;
        public float ProneHeightReduction = 0.3f;
        public bool AllowDoubleJump = true;
        public bool AllowWallJump = true;
        public bool AllowBhop = true;
        public bool AllowCrouch = true;
        public bool AllowProne = true;
        public bool AllowLedgeClimb = true;
        public bool AllowLadderClimb = true;
    }

    public class CapsuleColliderData
    {
        public float Radius;
        public float Height;
        public Vector2 Center;
        public float CrouchRadius;
        public float CrouchHeight;
        public float ProneRadius;
        public float ProneHeight;

        public CapsuleColliderData(float radius, float height, Vector2 center)
        {
            Radius = radius;
            Height = height;
            Center = center;
            CrouchRadius = radius;
            CrouchHeight = height * 0.5f;
            ProneRadius = radius * 1.2f;
            ProneHeight = height * 0.3f;
        }

        public float GetHeight(bool crouching, bool prone)
        {
            if (prone) return ProneHeight;
            if (crouching) return CrouchHeight;
            return Height;
        }

        public float GetRadius(bool crouching, bool prone)
        {
            if (prone) return ProneRadius;
            if (crouching) return CrouchRadius;
            return Radius;
        }
    }

    public class GroundHitInfo
    {
        public bool Hit;
        public Vector2 Point;
        public Vector2 Normal;
        public float Distance;
        public GameObject HitObject;
        public GroundType GroundType;
        public RigidBody2D PlatformRigidBody;
        public Vector2 PlatformVelocity;

        public GroundHitInfo()
        {
            Hit = false;
            Point = Vector2.Zero;
            Normal = Vector2.Up;
            Distance = float.MaxValue;
            HitObject = null;
            GroundType = GroundType.Normal;
            PlatformRigidBody = null;
            PlatformVelocity = Vector2.Zero;
        }

        public void Reset()
        {
            Hit = false;
            Point = Vector2.Zero;
            Normal = Vector2.Up;
            Distance = float.MaxValue;
            HitObject = null;
            GroundType = GroundType.Normal;
            PlatformRigidBody = null;
            PlatformVelocity = Vector2.Zero;
        }
    }

    public class WallHitInfo
    {
        public bool HitLeft;
        public bool HitRight;
        public Vector2 LeftPoint;
        public Vector2 RightPoint;
        public Vector2 LeftNormal;
        public Vector2 RightNormal;
        public float LeftDistance;
        public float RightDistance;
        public GameObject LeftHitObject;
        public GameObject RightHitObject;

        public void Reset()
        {
            HitLeft = false;
            HitRight = false;
            LeftPoint = Vector2.Zero;
            RightPoint = Vector2.Zero;
            LeftNormal = Vector2.Zero;
            RightNormal = Vector2.Zero;
            LeftDistance = float.MaxValue;
            RightDistance = float.MaxValue;
            LeftHitObject = null;
            RightHitObject = null;
        }
    }

    public class CeilingHitInfo
    {
        public bool Hit;
        public Vector2 Point;
        public Vector2 Normal;
        public float Distance;
        public GameObject HitObject;

        public void Reset()
        {
            Hit = false;
            Point = Vector2.Zero;
            Normal = Vector2.Down;
            Distance = float.MaxValue;
            HitObject = null;
        }
    }

    public class LadderInfo
    {
        public bool IsOnLadder;
        public Vector2 LadderPosition;
        public Vector2 LadderSize;
        public Vector2 TopPosition;
        public Vector2 BottomPosition;
        public GameObject LadderGameObject;

        public void Reset()
        {
            IsOnLadder = false;
            LadderPosition = Vector2.Zero;
            LadderSize = Vector2.Zero;
            TopPosition = Vector2.Zero;
            BottomPosition = Vector2.Zero;
            LadderGameObject = null;
        }
    }

    public class LedgeInfo
    {
        public bool CanGrabLeft;
        public bool CanGrabRight;
        public Vector2 LeftLedgePosition;
        public Vector2 RightLedgePosition;
        public Vector2 LeftClimbPosition;
        public Vector2 RightClimbPosition;
        public Vector2 LeftStandPosition;
        public Vector2 RightStandPosition;

        public void Reset()
        {
            CanGrabLeft = false;
            CanGrabRight = false;
            LeftLedgePosition = Vector2.Zero;
            RightLedgePosition = Vector2.Zero;
            LeftClimbPosition = Vector2.Zero;
            RightClimbPosition = Vector2.Zero;
            LeftStandPosition = Vector2.Zero;
            RightStandPosition = Vector2.Zero;
        }
    }

    public class InputBuffer
    {
        public bool JumpPressed;
        public bool JumpHeld;
        public bool CrouchPressed;
        public bool CrouchHeld;
        public float HorizontalInput;
        public float VerticalInput;
        public float JumpPressTime;
        public float CrouchPressTime;
        public int JumpCount;
        public bool SprintPressed;

        public InputBuffer()
        {
            Reset();
        }

        public void Reset()
        {
            JumpPressed = false;
            JumpHeld = false;
            CrouchPressed = false;
            CrouchHeld = false;
            HorizontalInput = 0f;
            VerticalInput = 0f;
            JumpPressTime = 0f;
            CrouchPressTime = 0f;
            JumpCount = 0;
            SprintPressed = false;
        }

        public void RecordJump()
        {
            JumpPressed = true;
            JumpPressTime = 0f;
            JumpCount++;
        }

        public void RecordCrouch()
        {
            CrouchPressed = true;
            CrouchPressTime = 0f;
        }

        public void Update(float deltaTime)
        {
            if (JumpPressTime >= 0f)
                JumpPressTime += deltaTime;
            if (CrouchPressTime >= 0f)
                CrouchPressTime += deltaTime;

            if (JumpPressTime > 0.2f)
            {
                JumpPressed = false;
            }
            if (CrouchPressTime > 0.2f)
            {
                CrouchPressed = false;
            }
        }
    }

    public class AnimatorStateInfo
    {
        public MovementState MovementState;
        public CharacterState CharacterState;
        public JumpState JumpState;
        public bool IsGrounded;
        public bool IsMoving;
        public bool IsCrouching;
        public bool IsProne;
        public bool IsRunning;
        public bool IsOnIce;
        public bool IsSliding;
        public bool IsClimbing;
        public bool IsWallSliding;
        public bool IsLedgeGrabbing;
        public float HorizontalSpeed;
        public float VerticalSpeed;
        public float GroundAngle;
        public float LandingIntensity;
        public float JumpPhase;
        public float AirTime;
        public int ConsecutiveJumps;

        public AnimatorStateInfo()
        {
            MovementState = MovementState.Grounded;
            CharacterState = CharacterState.Idle;
            JumpState = JumpState.None;
            IsGrounded = false;
            IsMoving = false;
            IsCrouching = false;
            IsProne = false;
            IsRunning = false;
            IsOnIce = false;
            IsSliding = false;
            IsClimbing = false;
            IsWallSliding = false;
            IsLedgeGrabbing = false;
            HorizontalSpeed = 0f;
            VerticalSpeed = 0f;
            GroundAngle = 0f;
            LandingIntensity = 0f;
            JumpPhase = 0f;
            AirTime = 0f;
            ConsecutiveJumps = 0;
        }

        public void Update(CharacterController2D controller)
        {
            IsGrounded = controller.IsGrounded;
            IsCrouching = controller.IsCrouching;
            IsProne = controller.IsProne;
            IsRunning = controller.IsRunning;
            IsOnIce = controller.IsOnIce;
            HorizontalSpeed = Math.Abs(controller.Velocity.X);
            VerticalSpeed = controller.Velocity.Y;
            IsMoving = HorizontalSpeed > 0.1f;
            MovementState = controller.GetMovementState();
            CharacterState = controller.CurrentState;
            JumpState = controller.CurrentJumpState;
            IsSliding = controller.IsSliding;
            IsClimbing = controller.IsOnLadder;
            IsWallSliding = controller.IsWallSliding;
            IsLedgeGrabbing = controller.IsLedgeHanging;
            GroundAngle = controller.GroundAngle;
            AirTime = controller.AirTime;
            ConsecutiveJumps = controller.JumpCount;
        }
    }

    public class CharacterController2D : Component
    {
        private CapsuleColliderData colliderData;
        private PlatformerPhysicsSettings settings;
        private Vector2 velocity;
        private Vector2 targetVelocity;
        private Vector2 platformVelocity;
        private Vector2 lastPlatformVelocity;
        private CharacterState currentState;
        private MovementState movementState;
        private JumpState jumpState;
        private CharacterFlags flags;
        private GroundHitInfo groundInfo;
        private WallHitInfo wallInfo;
        private CeilingHitInfo ceilingInfo;
        private LadderInfo ladderInfo;
        private LedgeInfo ledgeInfo;
        private InputBuffer inputBuffer;
        private AnimatorStateInfo animatorState;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private float airTime;
        private float jumpHeight;
        private float apexTime;
        private int jumpCount;
        private bool canDoubleJump;
        private bool wasGrounded;
        private float landingIntensity;
        private float landingTimer;
        private float slideTimer;
        private float crumbleTimer;
        private bool isCrumbling;
        private float ledgeClimbTimer;
        private Vector2 ledgeStartPosition;
        private Vector2 ledgeTargetPosition;
        private float bhopTimer;
        private float lastGroundedTime;
        private Vector2 moveDirection;
        private float currentSpeed;
        private float slopeAngle;
        private Vector2 slopeNormal;
        private bool isOnSlope;
        private float wallJumpTimer;
        private float ledgeHangTimer;
        private bool isTransitioning;
        private Vector2 pendingVelocity;

        public CapsuleColliderData ColliderData => colliderData;
        public PlatformerPhysicsSettings Settings => settings;
        public Vector2 Velocity => velocity;
        public Vector2 TargetVelocity => targetVelocity;
        public Vector2 PlatformVelocity => platformVelocity;
        public CharacterState CurrentState => currentState;
        public MovementState CurrentMovementState => movementState;
        public JumpState CurrentJumpState => jumpState;
        public CharacterFlags Flags => flags;
        public GroundHitInfo GroundInfo => groundInfo;
        public WallHitInfo WallInfo => wallInfo;
        public CeilingHitInfo CeilingInfo => ceilingInfo;
        public LadderInfo LadderInfoData => ladderInfo;
        public LedgeInfo LedgeInfoData => ledgeInfo;
        public InputBuffer InputBufferData => inputBuffer;
        public AnimatorStateInfo AnimatorState => animatorState;

        public bool IsGrounded => (flags & CharacterFlags.Grounded) != 0;
        public bool IsCrouching => (flags & CharacterFlags.Crouching) != 0;
        public bool IsProne => (flags & CharacterFlags.Prone) != 0;
        public bool IsRunning => (flags & CharacterFlags.Running) != 0;
        public bool IsOnIce => (flags & CharacterFlags.OnIce) != 0;
        public bool IsSliding => (flags & CharacterFlags.Sliding) != 0;
        public bool IsOnLadder => (flags & CharacterFlags.OnLadder) != 0;
        public bool IsWallSliding => currentState == CharacterState.WallSliding;
        public bool IsLedgeHanging => currentState == CharacterState.LedgeHanging || currentState == CharacterState.LedgeClimbing;
        public bool IsJumping => jumpState == JumpState.Jumping;
        public bool IsFalling => jumpState == JumpState.Falling || jumpState == JumpState.Apex;
        public bool CanJump => IsGrounded || coyoteTimeCounter < settings.CoyoteTime;
        public bool CanDoubleJump => settings.AllowDoubleJump && canDoubleJump && !IsGrounded;
        public bool CanWallJump => settings.AllowWallJump && (wallInfo.HitLeft || wallInfo.HitRight) && !IsGrounded;
        public float AirTime => airTime;
        public int JumpCount => jumpCount;
        public float CoyoteTimeRemaining => Math.Max(0f, settings.CoyoteTime - coyoteTimeCounter);
        public float JumpBufferRemaining => Math.Max(0f, settings.JumpBufferTime - jumpBufferCounter);
        public float GroundAngle => slopeAngle;
        public Vector2 GroundNormal => slopeNormal;
        public bool IsTouchingCeiling => (flags & CharacterFlags.TouchingCeiling) != 0;
        public bool IsTouchingLeftWall => (flags & CharacterFlags.TouchingLeftWall) != 0;
        public bool IsTouchingRightWall => (flags & CharacterFlags.TouchingRightWall) != 0;
        public float LandingIntensity => landingIntensity;
        public Vector2 Position => Transform != null ? Transform.Position : Vector2.Zero;

        public event Action OnJump;
        public event Action OnDoubleJump;
        public event Action OnWallJump;
        public event Action OnLand;
        public event Action OnWallSlideStart;
        public event Action OnWallSlideEnd;
        public event Action OnCrumble;
        public event Action OnLedgeGrab;
        public event Action OnLedgeClimbStart;
        public event Action OnLedgeClimbComplete;
        public event Action OnLadderEnter;
        public event Action OnLadderExit;
        public event Action OnStateChanged;

        public CharacterController2D()
        {
            colliderData = new CapsuleColliderData(0.3f, 1.2f, new Vector2(0f, 0.6f));
            settings = new PlatformerPhysicsSettings();
            velocity = Vector2.Zero;
            targetVelocity = Vector2.Zero;
            platformVelocity = Vector2.Zero;
            lastPlatformVelocity = Vector2.Zero;
            currentState = CharacterState.Idle;
            movementState = MovementState.Grounded;
            jumpState = JumpState.None;
            flags = CharacterFlags.Grounded;
            groundInfo = new GroundHitInfo();
            wallInfo = new WallHitInfo();
            ceilingInfo = new CeilingHitInfo();
            ladderInfo = new LadderInfo();
            ledgeInfo = new LedgeInfo();
            inputBuffer = new InputBuffer();
            animatorState = new AnimatorStateInfo();
            coyoteTimeCounter = 0f;
            jumpBufferCounter = float.MaxValue;
            airTime = 0f;
            jumpHeight = 0f;
            apexTime = 0f;
            jumpCount = 0;
            canDoubleJump = true;
            wasGrounded = true;
            landingIntensity = 0f;
            landingTimer = 0f;
            slideTimer = 0f;
            crumbleTimer = 0f;
            isCrumbling = false;
            ledgeClimbTimer = 0f;
            ledgeStartPosition = Vector2.Zero;
            ledgeTargetPosition = Vector2.Zero;
            bhopTimer = 0f;
            lastGroundedTime = 0f;
            moveDirection = Vector2.Zero;
            currentSpeed = 0f;
            slopeAngle = 0f;
            slopeNormal = Vector2.Up;
            isOnSlope = false;
            wallJumpTimer = 0f;
            ledgeHangTimer = 0f;
            isTransitioning = false;
            pendingVelocity = Vector2.Zero;
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if ((flags & CharacterFlags.LockMovement) != 0)
            {
                velocity = Vector2.Zero;
                return;
            }

            UpdateTimers(deltaTime);
            ProcessInput();
            UpdateGroundDetection();
            UpdateWallDetection();
            UpdateCeilingDetection();
            UpdateLadderDetection();
            UpdateLedgeDetection();
            ApplyPhysics(deltaTime);
            UpdateState();
            UpdateAnimatorState();
            HandlePlatformMovement(deltaTime);
            isTransitioning = false;
        }

        private void UpdateTimers(float deltaTime)
        {
            inputBuffer.Update(deltaTime);

            if (IsGrounded)
            {
                coyoteTimeCounter = 0f;
                airTime = 0f;
            }
            else
            {
                coyoteTimeCounter += deltaTime;
                airTime += deltaTime;
            }

            if (inputBuffer.JumpPressed)
            {
                jumpBufferCounter = 0f;
            }
            else if (jumpBufferCounter < settings.JumpBufferTime)
            {
                jumpBufferCounter += deltaTime;
            }

            if (landingTimer > 0f)
            {
                landingTimer -= deltaTime;
            }

            if (wallJumpTimer > 0f)
            {
                wallJumpTimer -= deltaTime;
            }

            if (bhopTimer > 0f)
            {
                bhopTimer -= deltaTime;
            }

            if (ledgeHangTimer > 0f)
            {
                ledgeHangTimer -= deltaTime;
            }

            if (isCrumbling && crumbleTimer > 0f)
            {
                crumbleTimer -= deltaTime;
                if (crumbleTimer <= 0f)
                {
                    TriggerCrumble();
                }
            }
        }

        private void ProcessInput()
        {
            moveDirection = new Vector2(inputBuffer.HorizontalInput, inputBuffer.VerticalInput);

            if (IsLedgeHanging)
            {
                if (inputBuffer.JumpPressed)
                {
                    StartLedgeJump();
                }
                else if (moveDirection.y < -0.5f)
                {
                    ReleaseLedge();
                }
                else if (moveDirection.y > 0.5f && settings.AllowLedgeClimb)
                {
                    StartLedgeClimb();
                }
            }
            else if (IsOnLadder)
            {
                if (inputBuffer.JumpPressed && !IsGrounded)
                {
                    ExitLadder(true);
                }
                else if (IsGrounded && moveDirection.y > 0.5f)
                {
                    ExitLadder(false);
                }
            }
            else
            {
                if (inputBuffer.JumpPressed || jumpBufferCounter < settings.JumpBufferTime)
                {
                    TryJump();
                }

                if (inputBuffer.CrouchPressed || (inputBuffer.CrouchHeld && settings.AllowCrouch))
                {
                    if (!IsCrouching && !IsProne && IsGrounded && IsOnSlope && settings.AllowSlopeSliding)
                    {
                        StartSliding();
                    }
                    else
                    {
                        StartCrouch();
                    }
                }
                else if (inputBuffer.CrouchHeld)
                {
                    if (!IsCrouching && IsGrounded)
                    {
                        StartCrouch();
                    }
                }
                else if (IsCrouching && !IsProne)
                {
                    TryStandUp();
                }

                if (IsCrouching && inputBuffer.JumpPressed && settings.AllowProne)
                {
                    StartProne();
                }
                else if (IsProne && !inputBuffer.CrouchHeld)
                {
                    TryStandUp();
                }

                if (inputBuffer.SprintPressed && !IsCrouching && !IsProne)
                {
                    flags |= CharacterFlags.Running;
                }
                else if (!inputBuffer.CrouchHeld)
                {
                    flags &= ~CharacterFlags.Running;
                }
            }
        }

        public void SetInput(Vector2 move, bool jump, bool crouch, bool sprint = false)
        {
            inputBuffer.HorizontalInput = move.X;
            inputBuffer.VerticalInput = move.Y;

            if (jump && !inputBuffer.JumpHeld)
            {
                inputBuffer.RecordJump();
            }
            inputBuffer.JumpHeld = jump;

            if (crouch && !inputBuffer.CrouchHeld)
            {
                inputBuffer.RecordCrouch();
            }
            inputBuffer.CrouchHeld = crouch;
            inputBuffer.SprintPressed = sprint;
        }

        public void ApplyHorizontalInput(float direction, float deltaTime)
        {
            if (IsLedgeHanging || IsOnLadder || isTransitioning)
                return;

            float targetSpeed = CalculateTargetSpeed();

            if (IsOnIce)
            {
                float iceAcceleration = settings.Acceleration * settings.IceFriction;
                velocity.X = Mathf.MoveTowards(velocity.X, direction * targetSpeed, iceAcceleration * deltaTime);
            }
            else if (IsGrounded)
            {
                float accel = IsRunning ? settings.Acceleration * 1.5f : settings.Acceleration;
                float decel = settings.Deceleration;

                if (Math.Abs(direction) > 0.1f)
                {
                    velocity.X = Mathf.MoveTowards(velocity.X, direction * targetSpeed, accel * deltaTime);
                }
                else
                {
                    velocity.X = Mathf.MoveTowards(velocity.X, 0f, decel * deltaTime * settings.GroundFriction);
                }

                if (settings.AllowBhop && bhopTimer > 0f && !IsRunning && airTime < settings.BhopWindowTime)
                {
                    velocity.X += direction * settings.BhopSpeedGain * deltaTime * 60f;
                    velocity.X = Mathf.Clamp(velocity.X, -settings.BhopMaxSpeedGain, settings.BhopMaxSpeedGain);
                }
            }
            else
            {
                float airAccel = settings.AirAcceleration * settings.AirControlPercentage;
                float airDecel = settings.AirDeceleration * settings.AirControlPercentage;

                if (Math.Abs(direction) > 0.1f)
                {
                    velocity.X = Mathf.MoveTowards(velocity.X, direction * targetSpeed, airAccel * deltaTime);
                }
                else
                {
                    velocity.X = Mathf.MoveTowards(velocity.X, 0f, airDecel * deltaTime);
                }
            }

            if (wallJumpTimer <= 0f)
            {
                if (direction < 0f && wallInfo.HitLeft)
                {
                    velocity.X = 0f;
                }
                else if (direction > 0f && wallInfo.HitRight)
                {
                    velocity.X = 0f;
                }
            }

            currentSpeed = Math.Abs(velocity.X);
        }

        private float CalculateTargetSpeed()
        {
            if (IsProne)
                return settings.WalkSpeed * settings.ProneSpeedMultiplier;
            if (IsCrouching)
                return settings.WalkSpeed * settings.CrouchSpeedMultiplier;
            if (IsRunning || currentSpeed > settings.WalkSpeed * settings.SprintThreshold)
                return settings.RunSpeed;
            return settings.WalkSpeed;
        }

        private void TryJump()
        {
            if (IsLedgeHanging)
            {
                StartLedgeJump();
                return;
            }

            if (IsOnLadder)
            {
                ExitLadder(true);
                return;
            }

            if (CanJump)
            {
                Jump();
            }
            else if (CanDoubleJump)
            {
                DoubleJump();
            }
            else if (CanWallJump)
            {
                WallJump();
            }

            jumpBufferCounter = float.MaxValue;
        }

        public void Jump()
        {
            if (!CanJump && !IsOnLadder)
                return;

            velocity.Y = settings.JumpForce;
            jumpState = JumpState.Jumping;
            jumpCount = 1;
            jumpHeight = 0f;
            canDoubleJump = true;
            coyoteTimeCounter = settings.CoyoteTime;
            bhopTimer = 0f;
            isOnSlope = false;

            if (groundInfo.Hit)
            {
                lastGroundedTime = 0f;
            }

            OnJump?.Invoke();
        }

        public void DoubleJump()
        {
            if (!CanDoubleJump)
                return;

            velocity.Y = settings.DoubleJumpForce;
            jumpState = JumpState.Jumping;
            jumpCount = 2;
            canDoubleJump = false;
            jumpHeight = 0f;

            OnDoubleJump?.Invoke();
        }

        public void WallJump()
        {
            if (!CanWallJump)
                return;

            float direction = wallInfo.HitLeft ? 1f : -1f;

            velocity.X = direction * settings.WallJumpHorizontalForce;
            velocity.Y = settings.WallJumpForce;

            jumpState = JumpState.Jumping;
            jumpCount++;
            jumpHeight = 0f;
            canDoubleJump = true;
            wallJumpTimer = 0.2f;

            flags |= CharacterFlags.Airborne;
            flags &= ~CharacterFlags.Grounded;

            OnWallJump?.Invoke();
        }

        private void StartSliding()
        {
            currentState = CharacterState.Sliding;
            flags |= CharacterFlags.Sliding;
            slideTimer = 0.5f;
        }

        private void StartCrouch()
        {
            if (!settings.AllowCrouch)
                return;

            if (IsProne)
            {
                TryStandUp();
                return;
            }

            flags |= CharacterFlags.Crouching;
            flags &= ~CharacterFlags.Running;

            if (currentSpeed > settings.WalkSpeed * settings.CrouchSpeedMultiplier)
            {
                velocity.X *= settings.CrouchSpeedMultiplier;
            }
        }

        private void TryStandUp()
        {
            Vector2 standPosition = Transform.Position;
            float currentHeight = colliderData.GetHeight(true, false);
            float standHeight = colliderData.Height;

            CeilingHitInfo testCeiling = new CeilingHitInfo();
            Vector2 testOrigin = standPosition + new Vector2(0, (standHeight - currentHeight) / 2f + 0.1f);
            PerformCeilingCheck(testOrigin, standHeight, standPosition, testCeiling);

            if (!testCeiling.Hit || testCeiling.Distance > standHeight * 0.8f)
            {
                flags &= ~CharacterFlags.Crouching;
                flags &= ~CharacterFlags.Prone;
            }
        }

        private void StartProne()
        {
            if (!settings.AllowProne)
                return;

            flags |= CharacterFlags.Prone;
            flags &= ~CharacterFlags.Crouching;
        }

        private void UpdateGroundDetection()
        {
            groundInfo.Reset();
            flags &= ~CharacterFlags.Grounded;
            flags &= ~CharacterFlags.OnIce;
            flags &= ~CharacterFlags.OnBouncySurface;
            flags &= ~CharacterFlags.OnCrumblePlatform;
            flags &= ~CharacterFlags.OnOneWayPlatform;
            flags &= ~CharacterFlags.OnMovingPlatform;

            if (currentState == CharacterState.LedgeClimbing || currentState == CharacterState.LedgeHanging)
                return;

            float currentHeight = colliderData.GetHeight(IsCrouching, IsProne);
            float currentRadius = colliderData.GetRadius(IsCrouching, IsProne);

            Vector2 position = Transform.Position;
            Vector2 feetPosition = position - new Vector2(0, currentHeight / 2f - currentRadius);

            int rayCount = 5;
            float raySpacing = currentRadius * 1.8f;
            float rayLength = currentRadius + 0.1f;

            for (int i = 0; i < rayCount; i++)
            {
                float t = (i / (float)(rayCount - 1)) - 0.5f;
                Vector2 rayOrigin = feetPosition + new Vector2(t * raySpacing, 0);
                Vector2 rayDirection = Vector2.Down;

                GroundHitInfo rayHit = PerformGroundRaycast(rayOrigin, rayDirection, rayLength);

                if (rayHit.Hit && rayHit.Distance < groundInfo.Distance)
                {
                    groundInfo = rayHit;
                }
            }

            Vector2 centerRayOrigin = feetPosition + new Vector2(0, -currentRadius * 0.3f);
            GroundHitInfo centerHit = PerformGroundRaycast(centerRayOrigin, Vector2.Down, rayLength * 1.5f);
            if (centerHit.Hit && centerHit.Distance < groundInfo.Distance)
            {
                groundInfo = centerHit;
            }

            if (groundInfo.Hit)
            {
                flags |= CharacterFlags.Grounded;
                slopeNormal = groundInfo.Normal;
                slopeAngle = Vector2.Angle(Vector2.Up, groundInfo.Normal);
                isOnSlope = slopeAngle > 5f && slopeAngle < settings.SlopeAngleLimit;

                if (slopeAngle > 45f)
                {
                    flags |= CharacterFlags.Sliding;
                }

                if (groundInfo.GroundType == GroundType.Ice)
                    flags |= CharacterFlags.OnIce;
                else if (groundInfo.GroundType == GroundType.Bouncy)
                    flags |= CharacterFlags.OnBouncySurface;
                else if (groundInfo.GroundType == GroundType.Crumble)
                    flags |= CharacterFlags.OnCrumblePlatform;
                else if (groundInfo.GroundType == GroundType.OneWay)
                    flags |= CharacterFlags.OnOneWayPlatform;
                else if (groundInfo.GroundType == GroundType.Moving)
                    flags |= CharacterFlags.OnMovingPlatform;

                if (groundInfo.PlatformRigidBody != null)
                {
                    platformVelocity = groundInfo.PlatformVelocity;
                }

                if (!wasGrounded && velocity.Y <= 0f)
                {
                    landingIntensity = Math.Min(Math.Abs(velocity.Y) / 20f, 1f);
                    landingTimer = 0.2f;
                    OnLand?.Invoke();
                }
            }

            wasGrounded = IsGrounded;
        }

        private GroundHitInfo PerformGroundRaycast(Vector2 origin, Vector2 direction, float distance)
        {
            GroundHitInfo hitInfo = new GroundHitInfo();

            RaycastHit2D hit = CollisionSystem.Raycast(origin, direction, distance, GetLayerMask());

            if (hit.Hit)
            {
                hitInfo.Hit = true;
                hitInfo.Point = hit.Point;
                hitInfo.Normal = hit.Normal;
                hitInfo.Distance = hit.Distance;
                hitInfo.HitObject = hit.Collider?.GameObject;

                if (hit.Collider != null)
                {
                    GroundType groundType = DetermineGroundType(hit.Collider);
                    hitInfo.GroundType = groundType;

                    RigidBody2D rb = hit.Collider.GetComponent<RigidBody2D>();
                    if (rb != null)
                    {
                        hitInfo.PlatformRigidBody = rb;
                        hitInfo.PlatformVelocity = rb.Velocity;
                    }
                }
            }

            return hitInfo;
        }

        private GroundType DetermineGroundType(Collider2D collider)
        {
            if (collider is OneWayPlatform oneWay)
                return GroundType.OneWay;
            if (collider is CrumblePlatform crumble)
                return GroundType.Crumble;
            if (collider is BouncySurface bouncy)
                return GroundType.Bouncy;
            if (collider is IceSurface)
                return GroundType.Ice;
            if (collider is MovingPlatform)
                return GroundType.Moving;
            if (collider is Ladder)
                return GroundType.Ladder;

            if (collider.GameObject != null)
            {
                string tag = collider.GameObject.Tag;
                if (tag.Contains("Ice")) return GroundType.Ice;
                if (tag.Contains("Bouncy")) return GroundType.Bouncy;
                if (tag.Contains("Crumble")) return GroundType.Crumble;
                if (tag.Contains("OneWay")) return GroundType.OneWay;
                if (tag.Contains("Moving")) return GroundType.Moving;
            }

            return GroundType.Normal;
        }

        private void UpdateWallDetection()
        {
            wallInfo.Reset();
            flags &= ~CharacterFlags.TouchingLeftWall;
            flags &= ~CharacterFlags.TouchingRightWall;

            float currentHeight = colliderData.GetHeight(IsCrouching, IsProne);
            float currentRadius = colliderData.GetRadius(IsCrouching, IsProne);

            Vector2 position = Transform.Position;
            float wallCheckHeight = currentHeight * 0.6f;
            float wallCheckDistance = currentRadius + 0.05f;

            Vector2 midPoint = position - new Vector2(0, currentHeight * 0.1f);

            RaycastHit2D leftHit = CollisionSystem.Raycast(
                midPoint + new Vector2(-currentRadius, 0),
                Vector2.Left,
                wallCheckDistance,
                GetLayerMask()
            );

            if (leftHit.Hit && IsValidWallSurface(leftHit.Normal))
            {
                wallInfo.HitLeft = true;
                wallInfo.LeftPoint = leftHit.Point;
                wallInfo.LeftNormal = leftHit.Normal;
                wallInfo.LeftDistance = leftHit.Distance;
                wallInfo.LeftHitObject = leftHit.Collider?.GameObject;
                flags |= CharacterFlags.TouchingLeftWall;
            }

            RaycastHit2D rightHit = CollisionSystem.Raycast(
                midPoint + new Vector2(currentRadius, 0),
                Vector2.Right,
                wallCheckDistance,
                GetLayerMask()
            );

            if (rightHit.Hit && IsValidWallSurface(rightHit.Normal))
            {
                wallInfo.HitRight = true;
                wallInfo.RightPoint = rightHit.Point;
                wallInfo.RightNormal = rightHit.Normal;
                wallInfo.RightDistance = rightHit.Distance;
                wallInfo.RightHitObject = rightHit.Collider?.GameObject;
                flags |= CharacterFlags.TouchingRightWall;
            }

            if (wallInfo.HitLeft || wallInfo.HitRight)
            {
                flags |= CharacterFlags.Airborne;
                flags &= ~CharacterFlags.Grounded;

                if (velocity.Y < 0 && !IsGrounded && !IsOnLadder && moveDirection.x != 0)
                {
                    bool touchingWall = (moveDirection.x < 0 && wallInfo.HitLeft) ||
                                       (moveDirection.x > 0 && wallInfo.HitRight);

                    if (touchingWall && currentState != CharacterState.WallSliding)
                    {
                        currentState = CharacterState.WallSliding;
                        OnWallSlideStart?.Invoke();
                    }
                }
            }
        }

        private bool IsValidWallSurface(Vector2 normal)
        {
            float angle = Vector2.Angle(Vector2.Right, normal);
            return angle > 60f && angle < 120f;
        }

        private void UpdateCeilingDetection()
        {
            ceilingInfo.Reset();
            flags &= ~CharacterFlags.TouchingCeiling;

            float currentHeight = colliderData.GetHeight(IsCrouching, IsProne);
            float currentRadius = colliderData.GetRadius(IsCrouching, IsProne);

            Vector2 position = Transform.Position;
            Vector2 headPosition = position + new Vector2(0, currentHeight / 2f - currentRadius);

            int rayCount = 3;
            float raySpacing = currentRadius * 1.5f;
            float rayLength = currentRadius + 0.1f;

            for (int i = 0; i < rayCount; i++)
            {
                float t = (i / (float)(rayCount - 1)) - 0.5f;
                Vector2 rayOrigin = headPosition + new Vector2(t * raySpacing, currentRadius * 0.5f);

                RaycastHit2D hit = CollisionSystem.Raycast(rayOrigin, Vector2.Up, rayLength, GetLayerMask());

                if (hit.Hit)
                {
                    ceilingInfo.Hit = true;
                    ceilingInfo.Point = hit.Point;
                    ceilingInfo.Normal = hit.Normal;
                    ceilingInfo.Distance = hit.Distance;
                    ceilingInfo.HitObject = hit.Collider?.GameObject;
                    flags |= CharacterFlags.TouchingCeiling;
                    break;
                }
            }

            if (ceilingInfo.Hit && velocity.Y > 0)
            {
                velocity.Y = 0f;
            }
        }

        private void UpdateLadderDetection()
        {
            ladderInfo.Reset();
            flags &= ~CharacterFlags.OnLadder;

            if (!settings.AllowLadderClimb)
                return;

            float currentHeight = colliderData.GetHeight(IsCrouching, IsProne);
            Vector2 position = Transform.Position;

            Vector2 searchOrigin = position;
            float searchRadius = currentHeight;

            List<Collider2D> nearbyColliders = CollisionSystem.OverlapCircle(searchOrigin, searchRadius, GetLayerMask());

            foreach (var collider in nearbyColliders)
            {
                if (collider is Ladder ladder)
                {
                    Vector2 ladderPos = ladder.Transform?.Position ?? Vector2.Zero;
                    Vector2 ladderSize = ladder.Size;

                    float leftEdge = ladderPos.X - ladderSize.X / 2f;
                    float rightEdge = ladderPos.X + ladderSize.X / 2f;
                    float bottomEdge = ladderPos.Y - ladderSize.Y / 2f;
                    float topEdge = ladderPos.Y + ladderSize.Y / 2f;

                    if (position.X >= leftEdge && position.X <= rightEdge &&
                        position.Y >= bottomEdge && position.Y <= topEdge)
                    {
                        ladderInfo.IsOnLadder = true;
                        ladderInfo.LadderPosition = ladderPos;
                        ladderInfo.LadderSize = ladderSize;
                        ladderInfo.TopPosition = new Vector2(ladderPos.X, topEdge);
                        ladderInfo.BottomPosition = new Vector2(ladderPos.X, bottomEdge);
                        ladderInfo.LadderGameObject = ladder.GameObject;
                        flags |= CharacterFlags.OnLadder;
                        break;
                    }
                }
            }

            if (ladderInfo.IsOnLadder && currentState != CharacterState.LedgeHanging && currentState != CharacterState.LedgeClimbing)
            {
                if (currentState != CharacterState.LadderClimbing)
                {
                    EnterLadder();
                }
            }
        }

        private void EnterLadder()
        {
            currentState = CharacterState.LadderClimbing;
            movementState = MovementState.Climbing;
            velocity = Vector2.Zero;
            OnLadderEnter?.Invoke();
        }

        private void ExitLadder(bool jumpOff)
        {
            movementState = IsGrounded ? MovementState.Grounded : MovementState.Airborne;

            if (jumpOff)
            {
                velocity.Y = settings.LadderJumpForce;
                if (moveDirection.x != 0)
                {
                    velocity.X = moveDirection.x * settings.WalkSpeed;
                }
            }

            ladderInfo.Reset();
            OnLadderExit?.Invoke();
        }

        private void UpdateLedgeDetection()
        {
            ledgeInfo.Reset();
            flags &= ~CharacterFlags.CanClimbLedge;

            if (!settings.AllowLedgeClimb)
                return;
            if (IsGrounded || IsOnLadder || IsLedgeHanging)
                return;
            if (velocity.Y > 0.1f)
                return;

            float currentHeight = colliderData.GetHeight(IsCrouching, IsProne);
            float currentRadius = colliderData.GetRadius(IsCrouching, IsProne);
            float ledgeGrabHeight = currentHeight * 0.8f;

            Vector2 position = Transform.Position;
            float checkDistance = currentRadius + settings.LedgeGrabRange;

            if (wallInfo.HitLeft)
            {
                Vector2 wallTop = wallInfo.LeftPoint + new Vector2(0, -currentRadius);
                Vector2 grabPosition = new Vector2(wallTop.X + currentRadius, wallTop.Y);

                bool canGrab = CheckLedgeGrabPosition(grabPosition, position, currentHeight);
                if (canGrab)
                {
                    ledgeInfo.CanGrabLeft = true;
                    ledgeInfo.LeftLedgePosition = grabPosition;
                    ledgeInfo.LeftClimbPosition = grabPosition + new Vector2(0, currentHeight * 0.3f);
                    ledgeInfo.LeftStandPosition = grabPosition + new Vector2(currentRadius * 2, currentHeight * 0.5f);
                    flags |= CharacterFlags.CanClimbLedge;
                }
            }

            if (wallInfo.HitRight)
            {
                Vector2 wallTop = wallInfo.RightPoint + new Vector2(0, -currentRadius);
                Vector2 grabPosition = new Vector2(wallTop.X - currentRadius, wallTop.Y);

                bool canGrab = CheckLedgeGrabPosition(grabPosition, position, currentHeight);
                if (canGrab)
                {
                    ledgeInfo.CanGrabRight = true;
                    ledgeInfo.RightLedgePosition = grabPosition;
                    ledgeInfo.RightClimbPosition = grabPosition + new Vector2(0, currentHeight * 0.3f);
                    ledgeInfo.RightStandPosition = grabPosition + new Vector2(-currentRadius * 2, currentHeight * 0.5f);
                    flags |= CharacterFlags.CanClimbLedge;
                }
            }
        }

        private bool CheckLedgeGrabPosition(Vector2 grabPosition, Vector2 playerPosition, float playerHeight)
        {
            Vector2 testPosition = grabPosition + new Vector2(0, playerHeight * 0.5f);
            float testRadius = 0.1f;

            List<Collider2D> hits = CollisionSystem.OverlapCircle(testPosition, testRadius, GetLayerMask());
            bool hasCeiling = false;

            foreach (var hit in hits)
            {
                Vector2 hitPos = hit.Transform?.Position ?? Vector2.Zero;
                if (hitPos.Y > grabPosition.Y)
                {
                    hasCeiling = true;
                    break;
                }
            }

            bool clearAbove = !hasCeiling;

            Vector2 belowGrab = grabPosition + new Vector2(0, -0.5f);
            RaycastHit2D groundCheck = CollisionSystem.Raycast(belowGrab, Vector2.Down, 1f, GetLayerMask());
            bool hasFloor = groundCheck.Hit;

            return clearAbove && hasFloor && Math.Abs(grabPosition.Y - playerPosition.Y) < playerHeight;
        }

        private void StartLedgeClimb()
        {
            if (!settings.AllowLedgeClimb)
                return;

            isTransitioning = true;
            currentState = CharacterState.LedgeClimbing;
            ledgeClimbTimer = 0f;
            velocity = Vector2.Zero;

            ledgeStartPosition = Transform.Position;

            if (ledgeInfo.CanGrabLeft)
            {
                ledgeTargetPosition = ledgeInfo.LeftStandPosition;
            }
            else if (ledgeInfo.CanGrabRight)
            {
                ledgeTargetPosition = ledgeInfo.RightStandPosition;
            }

            OnLedgeClimbStart?.Invoke();
        }

        private void CompleteLedgeClimb()
        {
            Transform.Position = ledgeTargetPosition;
            currentState = CharacterState.Idle;
            velocity = Vector2.Zero;
            isTransitioning = false;
            OnLedgeClimbComplete?.Invoke();
        }

        private void ReleaseLedge()
        {
            currentState = CharacterState.Falling;
            ledgeHangTimer = 0.1f;
        }

        private void StartLedgeJump()
        {
            if (!CanWallJump)
                return;

            float direction = ledgeInfo.CanGrabLeft ? 1f : -1f;

            velocity.X = direction * settings.WallJumpHorizontalForce;
            velocity.Y = settings.JumpForce * 0.8f;

            currentState = CharacterState.WallJumping;
            wallJumpTimer = 0.15f;
            isTransitioning = true;
        }

        private void ApplyPhysics(float deltaTime)
        {
            if (IsOnLadder && currentState == CharacterState.LadderClimbing)
            {
                ApplyLadderPhysics(deltaTime);
                return;
            }

            if (IsLedgeHanging)
            {
                velocity = Vector2.Zero;
                return;
            }

            if (currentState == CharacterState.LedgeClimbing)
            {
                ledgeClimbTimer += deltaTime;
                float t = ledgeClimbTimer / 0.3f;

                if (t >= 1f)
                {
                    CompleteLedgeClimb();
                }
                else
                {
                    Vector2 start = ledgeInfo.CanGrabLeft ? ledgeInfo.LeftLedgePosition : ledgeInfo.RightLedgePosition;
                    Vector2 end = ledgeInfo.CanGrabLeft ? ledgeInfo.LeftStandPosition : ledgeInfo.RightStandPosition;
                    Transform.Position = Vector2.Lerp(start, end, t);
                }

                return;
            }

            ApplyGravity(deltaTime);
            ApplyJumpPhysics(deltaTime);
            ApplyMovement(deltaTime);
            ApplyFriction(deltaTime);
            ApplyVelocity(deltaTime);

            if (IsOnIce && IsGrounded)
            {
                ApplyIcePhysics(deltaTime);
            }

            if (groundInfo.Hit && groundInfo.GroundType == GroundType.Bouncy)
            {
                HandleBouncySurface();
            }
        }

        private void ApplyGravity(float deltaTime)
        {
            if (IsGrounded && !isOnSlope)
            {
                velocity.Y = 0f;
                jumpState = JumpState.None;
                return;
            }

            if (IsOnLadder)
                return;

            float gravity = settings.Gravity;

            if (velocity.Y < 0)
            {
                gravity *= settings.FallMultiplier;
            }
            else if (velocity.Y > 0 && !inputBuffer.JumpHeld)
            {
                gravity *= settings.LowJumpMultiplier;
            }

            float apexGravityMultiplier = 1f;
            if (Math.Abs(velocity.Y) < settings.ApexThreshold && Math.Abs(velocity.X) > 1f)
            {
                apexGravityMultiplier = settings.ApexGravityMultiplier;
                jumpState = JumpState.Apex;
                apexTime += deltaTime;
            }
            else
            {
                jumpState = JumpState.Falling;
                apexTime = 0f;
            }

            velocity.Y += gravity * apexGravityMultiplier * deltaTime;

            if (IsWallSliding && velocity.Y < -settings.WallSlideSpeed)
            {
                velocity.Y = -settings.WallSlideSpeed;
            }

            float maxFall = settings.MaxFallSpeed;
            if (velocity.Y < maxFall)
            {
                velocity.Y = maxFall;
            }
        }

        private void ApplyJumpPhysics(float deltaTime)
        {
            if (!IsJumping && !IsFalling)
                return;

            jumpHeight += velocity.Y * deltaTime;
        }

        private void ApplyMovement(float deltaTime)
        {
            if (IsLedgeHanging || IsOnLadder || isTransitioning)
                return;

            float horizontalInput = inputBuffer.HorizontalInput;
            ApplyHorizontalInput(horizontalInput, deltaTime);
        }

        private void ApplyFriction(float deltaTime)
        {
            if (!IsGrounded)
                return;

            if (Math.Abs(inputBuffer.HorizontalInput) < 0.1f)
            {
                float friction = IsOnIce ? settings.IceFriction : settings.GroundFriction;
                velocity.X = Mathf.MoveTowards(velocity.X, 0f, friction * deltaTime * 60f);
            }
        }

        private void ApplyIcePhysics(float deltaTime)
        {
            if (Math.Abs(inputBuffer.HorizontalInput) < 0.1f && Math.Abs(velocity.X) < 0.1f)
            {
                velocity.X *= 0.95f;
            }
        }

        private void ApplyLadderPhysics(float deltaTime)
        {
            float verticalInput = inputBuffer.VerticalInput;

            if (Math.Abs(verticalInput) > 0.1f)
            {
                velocity.Y = verticalInput * settings.LadderSpeed;
            }
            else
            {
                velocity.Y = 0f;
            }

            float targetX = ladderInfo.LadderPosition.X;
            Transform.Position = new Vector2(targetX, Transform.Position.Y);
            velocity.X = 0f;

            if (Transform.Position.Y < ladderInfo.BottomPosition.Y)
            {
                ExitLadder(false);
            }
            else if (Transform.Position.Y > ladderInfo.TopPosition.Y - colliderData.Height * 0.5f)
            {
                velocity.Y = 0f;
                Transform.Position = new Vector2(Transform.Position.X, ladderInfo.TopPosition.Y - colliderData.Height * 0.5f);
            }
        }

        private void ApplyVelocity(float deltaTime)
        {
            Vector2 displacement = velocity * deltaTime;
            displacement += platformVelocity * deltaTime;

            Transform.Position += displacement;

            if (Transform.Position.Y < -100f)
            {
                Transform.Position = new Vector2(Transform.Position.X, -100f);
                velocity = Vector2.Zero;
            }
        }

        private void HandlePlatformMovement(float deltaTime)
        {
            if ((flags & CharacterFlags.OnMovingPlatform) == 0)
                return;

            if (groundInfo.PlatformRigidBody != null)
            {
                Vector2 platformDelta = groundInfo.PlatformVelocity * deltaTime;
                Transform.Position += platformDelta;
            }
        }

        private void HandleBouncySurface()
        {
            if (IsGrounded && velocity.Y <= 0f)
            {
                velocity.Y = -velocity.Y * settings.BounceMultiplier;
                currentState = CharacterState.Bouncing;
                OnLand?.Invoke();
            }
        }

        private void TriggerCrumble()
        {
            isCrumbling = false;

            if (groundInfo.HitObject != null)
            {
                CrumblePlatform crumblePlatform = groundInfo.HitObject.GetComponent<CrumblePlatform>();
                crumblePlatform?.Crumble();
            }

            OnCrumble?.Invoke();
        }

        private void UpdateState()
        {
            CharacterState newState = currentState;

            if (currentState == CharacterState.LedgeClimbing)
            {
                return;
            }

            if (isTransitioning)
            {
                return;
            }

            if (IsOnLadder)
            {
                newState = CharacterState.LadderClimbing;
                movementState = MovementState.Climbing;
            }
            else if (IsLedgeHanging)
            {
                newState = CharacterState.LedgeHanging;
                movementState = MovementState.Airborne;
            }
            else if (IsWallSliding)
            {
                newState = CharacterState.WallSliding;
                movementState = MovementState.Airborne;
            }
            else if (IsSliding)
            {
                newState = CharacterState.Sliding;
                movementState = MovementState.Grounded;
            }
            else if (IsGrounded)
            {
                movementState = MovementState.Grounded;

                if (IsProne)
                {
                    newState = CharacterState.Prone;
                }
                else if (IsCrouching)
                {
                    newState = CharacterState.Crouching;
                }
                else if (Math.Abs(velocity.X) > 0.1f)
                {
                    newState = IsRunning ? CharacterState.Running : CharacterState.Walking;
                }
                else
                {
                    newState = CharacterState.Idle;
                }
            }
            else
            {
                movementState = MovementState.Airborne;

                if (jumpCount == 2)
                {
                    newState = CharacterState.DoubleJumping;
                }
                else if (velocity.Y > 0)
                {
                    newState = CharacterState.Jumping;
                }
                else
                {
                    newState = CharacterState.Falling;
                }
            }

            if (newState != currentState)
            {
                OnStateChanged?.Invoke();
                currentState = newState;
            }
        }

        private void UpdateAnimatorState()
        {
            animatorState.Update(this);
        }

        public void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
        }

        public void SetVelocityX(float x)
        {
            velocity.X = x;
        }

        public void SetVelocityY(float y)
        {
            velocity.Y = y;
        }

        public void AddVelocity(Vector2 force)
        {
            velocity += force;
        }

        public void AddImpulse(Vector2 impulse)
        {
            velocity += impulse;
        }

        public Vector2 GetVelocity()
        {
            return velocity;
        }

        public float GetSpeed()
        {
            return velocity.Length();
        }

        public float GetHorizontalSpeed()
        {
            return Math.Abs(velocity.X);
        }

        public float GetVerticalSpeed()
        {
            return velocity.Y;
        }

        public bool IsMoving()
        {
            return velocity.LengthSquared() > 0.01f;
        }

        public bool IsFacingRight()
        {
            return moveDirection.x >= 0;
        }

        public void SetFacingDirection(float direction)
        {
            moveDirection = new Vector2(direction, moveDirection.Y);
        }

        public void Flip()
        {
            moveDirection = new Vector2(-moveDirection.X, moveDirection.Y);
        }

        public Vector2 GroundSphereCast(Vector2 origin, float radius, float distance, Vector2 direction, int layerMask = -1)
        {
            Vector2 displacement = Vector2.Zero;
            List<Collider2D> hits = CollisionSystem.OverlapCircle(origin, radius, layerMask);

            foreach (var hit in hits)
            {
                Vector2 hitPoint = hit.Transform?.Position ?? Vector2.Zero;
                Vector2 toHit = hitPoint - origin;
                float overlap = radius - toHit.Length();

                if (overlap > 0 && direction.Dot(toHit) > 0)
                {
                    Vector2 pushOut = direction.Normalized() * overlap;
                    displacement += pushOut;
                }
            }

            return displacement;
        }

        public Vector2 CeilingSphereCast(Vector2 origin, float radius, float distance, int layerMask = -1)
        {
            Vector2 displacement = Vector2.Zero;
            List<Collider2D> hits = CollisionSystem.OverlapCircle(origin, radius, layerMask);

            foreach (var hit in hits)
            {
                Vector2 hitPoint = hit.Transform?.Position ?? Vector2.Zero;
                Vector2 toHit = hitPoint - origin;

                if (toHit.Y > 0)
                {
                    float overlap = radius - toHit.Length();
                    if (overlap > 0)
                    {
                        displacement += new Vector2(0, -overlap);
                    }
                }
            }

            return displacement;
        }

        public void MoveWithPlatform(Vector2 platformDelta)
        {
            Transform.Position += platformDelta;
        }

        public bool IsGroundedCheck()
        {
            return IsGrounded;
        }

        public bool IsTouchingWallCheck()
        {
            return wallInfo.HitLeft || wallInfo.HitRight;
        }

        public bool IsTouchingCeilingCheck()
        {
            return ceilingInfo.Hit;
        }

        public void EnableMovementLock()
        {
            flags |= CharacterFlags.LockMovement;
        }

        public void DisableMovementLock()
        {
            flags &= ~CharacterFlags.LockMovement;
        }

        public void EnableRotationLock()
        {
            flags |= CharacterFlags.LockRotation;
        }

        public void DisableRotationLock()
        {
            flags &= ~CharacterFlags.LockRotation;
        }

        public void ForceGrounded()
        {
            flags |= CharacterFlags.Grounded;
            coyoteTimeCounter = 0f;
            airTime = 0f;
        }

        public void ForceAirborne()
        {
            flags &= ~CharacterFlags.Grounded;
        }

        public void ResetJumpState()
        {
            jumpState = JumpState.None;
            jumpCount = 0;
            canDoubleJump = true;
        }

        public void ResetAllState()
        {
            velocity = Vector2.Zero;
            platformVelocity = Vector2.Zero;
            currentState = CharacterState.Idle;
            movementState = MovementState.Grounded;
            jumpState = JumpState.None;
            flags = CharacterFlags.Grounded;
            coyoteTimeCounter = 0f;
            jumpBufferCounter = float.MaxValue;
            airTime = 0f;
            jumpCount = 0;
            canDoubleJump = true;
            landingIntensity = 0f;
            inputBuffer.Reset();
            groundInfo.Reset();
            wallInfo.Reset();
            ceilingInfo.Reset();
            ladderInfo.Reset();
            ledgeInfo.Reset();
        }

        private int GetLayerMask()
        {
            return -1;
        }

        public AABB GetBounds()
        {
            float height = colliderData.GetHeight(IsCrouching, IsProne);
            float radius = colliderData.GetRadius(IsCrouching, IsProne);

            Vector2 center = Transform.Position;
            Vector2 min = center - new Vector2(radius, height / 2f);
            Vector2 max = center + new Vector2(radius, height / 2f);

            return new AABB(min, max);
        }

        public Vector2 GetFeetPosition()
        {
            float height = colliderData.GetHeight(IsCrouching, IsProne);
            float radius = colliderData.GetRadius(IsCrouching, IsProne);
            return Transform.Position - new Vector2(0, height / 2f - radius);
        }

        public Vector2 GetHeadPosition()
        {
            float height = colliderData.GetHeight(IsCrouching, IsProne);
            float radius = colliderData.GetRadius(IsCrouching, IsProne);
            return Transform.Position + new Vector2(0, height / 2f - radius);
        }

        public void DebugDraw()
        {
            DrawBounds();
            DrawGroundCheck();
            DrawVelocity();
        }

        private void DrawBounds()
        {
        }

        private void DrawGroundCheck()
        {
        }

        private void DrawVelocity()
        {
        }
    }

    public class OneWayPlatform : Component
    {
        public Vector2 Size { get; set; } = new Vector2(2f, 0.2f);
        public string PlatformTag { get; set; } = "Platform";
    }

    public class CrumblePlatform : Component
    {
        public float CrumbleDelay { get; set; } = 1.5f;
        public float RegrowDelay { get; set; } = 3f;
        public bool IsCrumbling { get; private set; }
        public bool IsVisible { get; private set; } = true;

        private float crumbleTimer;
        private float regrowTimer;
        private bool wasTriggered;

        public event Action OnCrumbleStart;
        public event Action OnCrumbleComplete;
        public event Action OnRegrow;

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (IsCrumbling && crumbleTimer > 0f)
            {
                crumbleTimer -= deltaTime;
                if (crumbleTimer <= 0f)
                {
                    CompleteCrumble();
                }
            }

            if (!IsVisible && regrowTimer > 0f)
            {
                regrowTimer -= deltaTime;
                if (regrowTimer <= 0f)
                {
                    Regrow();
                }
            }
        }

        public void TriggerCrumble()
        {
            if (IsCrumbling || !IsVisible)
                return;

            IsCrumbling = true;
            crumbleTimer = CrumbleDelay;
            wasTriggered = true;
            OnCrumbleStart?.Invoke();
        }

        private void CompleteCrumble()
        {
            IsCrumbling = false;
            IsVisible = false;
            gameObject.SetActive(false);
            regrowTimer = RegrowDelay;
            OnCrumbleComplete?.Invoke();
        }

        private void Regrow()
        {
            IsVisible = true;
            gameObject.SetActive(true);
            wasTriggered = false;
            OnRegrow?.Invoke();
        }

        public bool CanPassThrough(Vector2 playerPosition, Vector2 playerVelocity)
        {
            if (!IsVisible)
                return true;

            Vector2 platformPosition = Transform.Position;
            float platformTop = platformPosition.Y + Size.Y / 2f;

            bool playerBelow = playerPosition.Y < platformTop - 0.1f;
            bool movingUp = playerVelocity.Y > 0f;

            return playerBelow && movingUp;
        }
    }

    public class BouncySurface : Component
    {
        public float BounceMultiplier { get; set; } = 1.5f;
        public float MinBounceVelocity { get; set; } = 5f;
    }

    public class IceSurface : Component
    {
        public float Friction { get; set; } = 0.02f;
        public float AccelerationMultiplier { get; set; } = 0.3f;
    }

    public class MovingPlatform : Component
    {
        public Vector2 StartPosition { get; set; }
        public Vector2 EndPosition { get; set; }
        public float Speed { get; set; } = 2f;
        public bool AutoStart { get; set; } = true;
        public bool PingPong { get; set; } = true;

        private Vector2 currentTarget;
        private Vector2 velocity;
        private bool isMovingForward = true;
        private bool isActive;

        public Vector2 Velocity => velocity;

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            StartPosition = Transform.Position;
            EndPosition = StartPosition + new Vector2(3f, 0f);
            currentTarget = EndPosition;
            isActive = AutoStart;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!isActive)
                return;

            Vector2 previousPosition = Transform.Position;

            Vector2 direction = (currentTarget - Transform.Position).Normalized();
            Transform.Position += direction * Speed * deltaTime;

            velocity = (Transform.Position - previousPosition) / deltaTime;

            float distanceToTarget = (currentTarget - Transform.Position).Length();
            if (distanceToTarget < 0.1f)
            {
                if (PingPong)
                {
                    isMovingForward = !isMovingForward;
                    currentTarget = isMovingForward ? EndPosition : StartPosition;
                }
                else
                {
                    Transform.Position = StartPosition;
                }
            }
        }

        public void Start()
        {
            isActive = true;
        }

        public void Stop()
        {
            isActive = false;
            velocity = Vector2.Zero;
        }

        public void Reset()
        {
            Transform.Position = StartPosition;
            currentTarget = EndPosition;
            isMovingForward = true;
            velocity = Vector2.Zero;
        }

        public void SetTargets(Vector2 start, Vector2 end)
        {
            StartPosition = start;
            EndPosition = end;
            currentTarget = EndPosition;
            Transform.Position = StartPosition;
            isMovingForward = true;
        }
    }

    public class Ladder : Component
    {
        public Vector2 Size { get; set; } = new Vector2(0.5f, 3f);
        public bool IsClimbable { get; set; } = true;
        public float ClimbSpeed { get; set; } = 5f;
    }

    public class RaycastHit2D
    {
        public bool Hit { get; set; }
        public Vector2 Point { get; set; }
        public Vector2 Normal { get; set; }
        public float Distance { get; set; }
        public Collider2D Collider { get; set; }
        public GameObject GameObject => Collider?.GameObject;

        public static RaycastHit2D None => new RaycastHit2D { Hit = false };
    }

    public static class CollisionSystem
    {
        public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask = -1)
        {
            return new RaycastHit2D { Hit = false };
        }

        public static List<Collider2D> OverlapCircle(Vector2 center, float radius, int layerMask = -1)
        {
            return new List<Collider2D>();
        }

        public static List<RaycastHit2D> CircleCastAll(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask = -1)
        {
            return new List<RaycastHit2D>();
        }

        public static RaycastHit2D BoxCast(Vector2 center, Vector2 size, float angle, Vector2 direction, float distance, int layerMask = -1)
        {
            return new RaycastHit2D { Hit = false };
        }

        public static bool CheckCircle(Vector2 center, float radius, int layerMask = -1)
        {
            return false;
        }

        public static bool CheckBox(Vector2 center, Vector2 size, int layerMask = -1)
        {
            return false;
        }
    }

    public abstract class Collider2D : Component
    {
        public bool IsTrigger { get; set; }
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public RigidBody2D AttachedRigidBody { get; set; }
        public abstract AABB Bounds { get; }
    }

    public class BoxCollider2D : Collider2D
    {
        public Vector2 Size { get; set; } = Vector2.One;

        public override AABB Bounds
        {
            get
            {
                Vector2 center = Transform != null ? Transform.Position + Offset : Offset;
                return new AABB(center, Size.X / 2f, Size.Y / 2f);
            }
        }
    }

    public class CircleCollider2D : Collider2D
    {
        public float Radius { get; set; } = 0.5f;

        public override AABB Bounds
        {
            get
            {
                Vector2 center = Transform != null ? Transform.Position + Offset : Offset;
                return AABB.CreateFromCircle(center, Radius);
            }
        }
    }
}
