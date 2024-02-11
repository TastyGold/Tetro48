using Raylib_cs;

namespace Tetro48
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameManager.Begin();

            while (!Raylib.WindowShouldClose())
            {
                GameManager.Update();
                GameManager.Draw();
            }

            GameManager.End();
        }
    }
}