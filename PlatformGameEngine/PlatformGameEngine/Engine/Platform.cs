namespace PlatformGameEngine.Engine
{
    public class Platform : GameObject
    {
        public Platform(Vector2 position, float width, float height, Color color) 
            : base("Platform", position, width, height, color)
        {
            IsStatic = true;
        }
    }
}