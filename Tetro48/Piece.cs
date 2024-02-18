using Raylib_cs;
using System.Numerics;

namespace Tetro48
{
    internal class Piece
    {
        public static Texture2D pieceTexture = Raylib.LoadTexture("..\\..\\..\\tetro48_blockColors.png");
        public static Texture2D centerTexture = Raylib.LoadTexture("..\\..\\..\\tetro48_pieceCenter.png");
        public static Color transparencyColor = new Color(255, 200, 200, 96);
        public static int resolution = 8;

        public bool CanRotate => color != 1 && color != 8;

        public int color = 0;
        public int center = 0;
        public List<VecInt2> blocks = new List<VecInt2>();

        public VecInt2 boundsMin, boundsMax;
        public VecInt2 GetPaddedBoundsMin() => boundsMin - VecInt2.One; 
        public VecInt2 GetPaddedBoundsMax() => boundsMax + VecInt2.One;

        public int Size => blocks.Count;

        public bool shouldDestroy = false;

        public void Draw(int boardX, int boardY, int boardWidth, int angle, bool transparent)
        {
            foreach (VecInt2 pos in blocks)
            {
                int x = center % boardWidth + pos.x;
                int y = center / boardWidth + pos.y;

                DrawCell(color, boardX, boardY, x, y, angle, transparent);
            }
        }

        public void DrawCenter(int boardX, int boardY, int boardWidth)
        {
            VecInt2 tile = GetCenterTile(boardWidth);
            Rectangle srec = new Rectangle(0, 0, resolution, resolution);
            Rectangle drec = new Rectangle(boardX + tile.x * resolution, boardY +tile.y * resolution, resolution, resolution);
            Raylib.DrawTexturePro(centerTexture, srec, drec, Vector2.Zero, 0, Color.White);
        }

        public bool IsIntersectingBounds(VecInt2 min, VecInt2 max)
        {
            return !(boundsMin.x > max.x || boundsMin.y > max.y || boundsMax.x < min.x || boundsMax.y < min.y);
        }

        public bool IsTouching(Piece p, int boardWidth)
        {
            VecInt2 center1 = GetCenterTile(boardWidth);
            VecInt2 center2 = p.GetCenterTile(boardWidth);

            bool touching = false;
            int i = 0;
            while (i < blocks.Count && !touching)
            {
                int j = 0;
                while (j < p.blocks.Count && !touching)
                {
                    int x1 = center1.x + blocks[i].x;
                    int y1 = center1.y + blocks[i].y;
                    int x2 = center2.x + p.blocks[j].x;
                    int y2 = center2.y + p.blocks[j].y;

                    int dx = Math.Abs(x1 - x2);
                    int dy = Math.Abs(y1 - y2);

                    if (dx + dy == 1) touching = true;
                    j++;
                }

                i++;
            }

            return touching;
        }

        public void MergeWith(Piece p, int boardWidth)
        {
            VecInt2 offset = p.GetCenterTile(boardWidth) - GetCenterTile(boardWidth);
            foreach (VecInt2 pos in p.blocks)
            {
                if (!blocks.Contains(offset + pos)) blocks.Add(offset + pos);
            }
        }

        public void Translate(VecInt2 offset, int boardWidth)
        {
            center += offset.x + offset.y * boardWidth;
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

        public void Highlight(int boardX, int boardY, int boardWidth, Color col)
        {
            VecInt2 pCenter = GetCenterTile(boardWidth);
            foreach (VecInt2 pos in blocks)
            {
                VecInt2 posTile = pCenter + pos;
                Rectangle drec = new Rectangle(boardX + posTile.x * 8, boardY + posTile.y * 8, resolution, resolution);
                Raylib.DrawRectangleRec(drec, col);
            }
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

        public VecInt2 GetCenterTile(int boardWidth)
        {
            return new VecInt2(center % boardWidth, center / boardWidth);
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