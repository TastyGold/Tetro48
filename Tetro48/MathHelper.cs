namespace Tetro48
{
    internal class MathHelper
    {
        public static float Modulo(float x, float m)
        {
            return (x % m + m) % m;
        }

        public static int Modulo(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}