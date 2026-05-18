namespace PlatformGameEngine.Engine
{
    public class Enemy : GameObject
    {
        private float _moveRange;
        private Vector2 _startPosition;
        private float _moveSpeed;
        private int _direction;

        public Enemy(Vector2 position, float moveRange) : base("Enemy", position, 30, 30, Color.Red)
        {
            _moveRange = moveRange;
            _startPosition = position;
            _moveSpeed = 80f;
            _direction = 1;
        }

        public override void Update(float deltaTime)
        {
            Position.X += _moveSpeed * _direction * deltaTime;

            float distanceFromStart = Math.Abs(Position.X - _startPosition.X);
            
            if (distanceFromStart >= _moveRange)
            {
                _direction *= -1;
            }

            base.Update(deltaTime);
        }
    }
}