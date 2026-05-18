namespace PlatformGameEngine.Engine
{
    public class GameObject
    {
        public string Name;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Width;
        public float Height;
        public Color Color;
        public bool IsActive;
        public bool IsStatic;

        public GameObject(string name, Vector2 position, float width, float height, Color color)
        {
            Name = name;
            Position = position;
            Velocity = Vector2.Zero;
            Width = width;
            Height = height;
            Color = color;
            IsActive = true;
            IsStatic = false;
        }

        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Width, Height);

        public virtual void Update(float deltaTime)
        {
            if (!IsStatic)
            {
                Position += Velocity * deltaTime;
            }
        }
    }
}