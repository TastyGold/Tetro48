using Raylib_cs;
using System.Numerics;

namespace Tetro48
{
    internal class Piece
    {
        public static Texture2D pieceTexture = Raylib.LoadTexture("..\\..\\..\\tetro48_blockColors.png");
        public static Color transparencyColor = new Color(255, 200, 200, 150);
        public static int resolution = 8;

        public bool CanRotate => color != 1 && color != 8;

        public int color = 0;
        public int center = 0;
        public List<VecInt2> blocks = new List<VecInt2>();

        public VecInt2 boundsMin, boundsMax;

        public void Draw(int boardX, int boardY, int boardWidth, int angle, bool transparent)
        {
            foreach (VecInt2 pos in blocks)
            {
                int x = center % boardWidth + pos.x;
                int y = center / boardWidth + pos.y;

                DrawCell(color, boardX, boardY, x, y, angle, transparent);
            }

            DrawCell(color, boardX, boardY, center % boardWidth, center / boardWidth, angle, transparent);
        }

        public void UpdateBounds()
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                boundsMin = new VecInt2(Math.Min(boundsMin.x, blocks[i].x), Math.Min(boundsMin.y, blocks[i].y));
                boundsMax = new VecInt2(Math.Max(boundsMax.x, blocks[i].x), Math.Max(boundsMax.y, blocks[i].y));
            }
        }

        public void DrawBounds(int screenX, int screenY, int boardWidth, int tileSize)
        {
            VecInt2 centerTile = Board.GetTile(center, boardWidth);
            DropZone.DrawOutline(
                centerTile.x + boundsMin.x,
                centerTile.y + boundsMin.y,
                boundsMax.x - boundsMin.x,
                boundsMax.y - boundsMin.y,
                screenX, screenY, tileSize, true);
        }

        public void Rotate(int angle, bool forceRotate)
        {
            if (CanRotate || forceRotate)
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    blocks[i] = VecInt2.Rotate(blocks[i], angle);
                }

                RotateBounds(angle);
            }
        }

        public void RotateBounds(int angle)
        {
            VecInt2 a = VecInt2.Rotate(boundsMin, angle);
            VecInt2 b = VecInt2.Rotate(boundsMax, angle);

            boundsMin = new VecInt2(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
            boundsMax = new VecInt2(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
        }

        public static void DrawCell(int color, int boardX, int boardY, int x, int y, int angle, bool transparent)
        {
            Rectangle srec = new Rectangle(color * resolution, 0, resolution, resolution);
            Rectangle drec = new Rectangle(boardX + x * 8 + resolution * 0.5f, boardY + y * 8 + resolution * 0.5f, resolution, resolution);
            Raylib.DrawTexturePro(pieceTexture, srec, drec, Vector2.One * resolution * 0.5f, angle * -90, transparent ? transparencyColor : Color.White);
        }
    }
}