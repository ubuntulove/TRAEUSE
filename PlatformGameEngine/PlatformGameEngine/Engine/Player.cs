using System.Windows.Input;

namespace PlatformGameEngine.Engine
{
    public class Player : GameObject
    {
        public bool IsGrounded;
        public float Speed;
        public float JumpForce;
        private float _maxSpeed;

        public Player(Vector2 position) : base("Player", position, 40, 60, Color.Cyan)
        {
            Speed = 300f;
            JumpForce = -500f;
            _maxSpeed = 400f;
            IsGrounded = false;
        }

        public override void Update(float deltaTime)
        {
            HandleInput();
            
            if (Velocity.X > _maxSpeed) Velocity.X = _maxSpeed;
            if (Velocity.X < -_maxSpeed) Velocity.X = -_maxSpeed;

            Velocity.X *= 0.85f;
            
            base.Update(deltaTime);
            
            if (Velocity.Y > 0)
            {
                IsGrounded = false;
            }
        }

        private void HandleInput()
        {
            if (InputManager.IsKeyPressed(Key.A) || InputManager.IsKeyPressed(Key.Left))
            {
                Velocity.X -= Speed * 0.1f;
            }
            if (InputManager.IsKeyPressed(Key.D) || InputManager.IsKeyPressed(Key.Right))
            {
                Velocity.X += Speed * 0.1f;
            }
            if ((InputManager.IsKeyDown(Key.W) || InputManager.IsKeyDown(Key.Up) || InputManager.IsKeyDown(Key.Space)) && IsGrounded)
            {
                Velocity.Y = JumpForce;
                IsGrounded = false;
            }
        }
    }
}