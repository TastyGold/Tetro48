using Raylib_cs;
using System.Numerics;

namespace Tetro48
{
    internal class Piece
    {
        public static Texture2D pieceTexture = Raylib.LoadTexture("..\\..\\..\\tetro48_blockColors.png");
        public static int resolution = 8;

        public bool CanRotate => color != 1;

        public int color = 0;
        public int center = 0;
        public List<VecInt2> blocks = new List<VecInt2>();

        public void Draw(int boardX, int boardY, int blockSize, int boardWidth)
        {
            foreach (VecInt2 pos in blocks)
            {
                int x = center % boardWidth + pos.x;
                int y = center / boardWidth + pos.y;

                DrawCell(color, boardX, boardY, x, y);
            }

            DrawCell(color, boardX, boardY, center % boardWidth, center / boardWidth);
        }

        public void Rotate(int angle)
        {
            if (CanRotate)
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    blocks[i] = VecInt2.Rotate(blocks[i], angle);
                }
            }
        }

        public static void DrawCell(int color, int boardX, int boardY, int x, int y)
        {
            Rectangle src = new Rectangle(color * resolution, 0, resolution, resolution);
            Rectangle dest = new Rectangle(boardX + x * 8, boardY + y * 8, resolution, resolution);
            Raylib.DrawTexturePro(pieceTexture, src, dest, Vector2.Zero, 0, Color.White);
        }
    }
}