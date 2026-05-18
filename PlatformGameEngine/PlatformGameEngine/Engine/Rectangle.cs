namespace PlatformGameEngine.Engine
{
    public struct Rectangle
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;

        public Vector2 Center => new Vector2(X + Width / 2, Y + Height / 2);
    }
}