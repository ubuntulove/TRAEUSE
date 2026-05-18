namespace PlatformGameEngine.Engine
{
    public class Collectible : GameObject
    {
        public bool Collected;

        public Collectible(Vector2 position) : base("Collectible", position, 20, 20, Color.Yellow)
        {
            Collected = false;
            IsStatic = true;
        }

        public void Collect()
        {
            Collected = true;
            IsActive = false;
        }
    }
}