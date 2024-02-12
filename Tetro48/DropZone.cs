using Raylib_cs;
using System.Numerics;

namespace Tetro48
{
    internal class DropZone
    {
        public int size;
        public int angle = 0;

        public static Texture2D zoneTexture = Raylib.LoadTexture("..\\..\\..\\tetro48_dropZone.png");

        public static readonly Color outlineColor = new Color(38, 57, 87, 120);
        public static readonly Color outlineColorH = new Color(48, 89, 145, 120);
        public static readonly Color backgroundColor = new Color(38, 57, 87, 15);
        public static readonly Color backgroundColorH = new Color(38, 57, 87, 30);

        public void Draw(int screenX, int screenY, int boardWidth, int boardHeight, int tileSize)
        {
            int x = screenX + (angle == 3 ? tileSize * (boardWidth - size) : 0);
            int y = screenY + (angle == 2 ? tileSize * (boardHeight - size) : 0);
            Raylib.DrawRectangle(x, y, (angle % 2 == 0 ? boardWidth : size) * tileSize, (angle % 2 == 1 ? boardHeight : size) * tileSize, new Color(38, 57, 87, 150));
        }

        public int GetMinX(int boardWidth) => angle == 3 ? boardWidth - size : 0;
        public int GetMinY(int boardHeight) => angle == 2 ? boardHeight - size : 0;
        public int GetMaxX(int boardWidth) => (angle == 1 ? size : boardWidth) - 1;
        public int GetMaxY(int boardHeight) => (angle == 0 ? size : boardHeight) - 1;

        public bool InZoneBounds(VecInt2 tile, int boardWidth, int boardHeight)
        {
            return tile.x >= GetMinX(boardWidth)
                && tile.y >= GetMinY(boardHeight)
                && tile.x <= GetMaxX(boardWidth)
                && tile.y <= GetMaxY(boardHeight);
        }

        public bool InZoneBounds(int cell, int boardWidth, int boardHeight)
        {
            return InZoneBounds(new VecInt2(cell % boardWidth, cell / boardWidth), boardWidth, boardHeight);
        }

        public void DrawSmart(int screenX, int screenY, int boardWidth, int boardHeight, int tileSize, bool highlighted)
        {
            int x = GetMinX(boardWidth);
            int y = GetMinY(boardHeight);
            int w = (GetMaxX(boardWidth) - GetMinX(boardWidth));
            int h = (GetMaxY(boardHeight) - GetMinY(boardHeight));

            Raylib.DrawRectangle(screenX + (x * tileSize), screenY + (y * tileSize), (w + 1) * tileSize, (h + 1) * tileSize, highlighted ? backgroundColorH : backgroundColor);

            DrawOutline(x, y, w, h, screenX, screenY, tileSize, highlighted);
        }

        public static void DrawOutline(int x, int y, int w, int h, int screenX, int screenY, int tileSize, bool highlighted)
        {
            for (int yi = y; yi <= y + h; yi++)
            {
                for (int xi = x; xi <= x + w; xi++)
                {
                    int texX = 1;
                    int texY = 1;

                    if (xi == x) texX--;
                    if (xi == x + w) texX++;
                    if (yi == y) texY--;
                    if (yi == y + h) texY++;

                    Rectangle srec = new Rectangle(texX * 8, texY * 8, 8, 8);
                    Rectangle drec = new Rectangle(screenX + xi * tileSize, screenY + yi * tileSize, tileSize, tileSize);
                    Raylib.DrawTexturePro(zoneTexture, srec, drec, Vector2.Zero, 0, highlighted ? outlineColorH : outlineColor);
                }
            }
        }

        public DropZone(int size, int angle)
        {
            this.size = size;
            this.angle = angle;
        }
    }
}