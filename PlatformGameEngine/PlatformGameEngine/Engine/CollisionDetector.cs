namespace PlatformGameEngine.Engine
{
    public static class CollisionDetector
    {
        public static bool CheckCollision(Rectangle a, Rectangle b)
        {
            return a.Left < b.Right && a.Right > b.Left &&
                   a.Top < b.Bottom && a.Bottom > b.Top;
        }

        public static bool CheckCollision(GameObject a, GameObject b)
        {
            if (!a.IsActive || !b.IsActive) return false;
            return CheckCollision(a.Bounds, b.Bounds);
        }

        public static Vector2 GetCollisionNormal(Rectangle a, Rectangle b)
        {
            float overlapX = Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left);
            float overlapY = Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top);

            if (overlapX < overlapY)
            {
                return a.Center.X < b.Center.X ? new Vector2(-1, 0) : new Vector2(1, 0);
            }
            else
            {
                return a.Center.Y < b.Center.Y ? new Vector2(0, -1) : new Vector2(0, 1);
            }
        }

        public static float GetOverlapAmount(Rectangle a, Rectangle b)
        {
            float overlapX = Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left);
            float overlapY = Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top);
            return Math.Min(overlapX, overlapY);
        }
    }
}