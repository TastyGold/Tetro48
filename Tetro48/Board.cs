using Raylib_cs;
using System.Numerics;

namespace Tetro48
{
    internal class Board
    {
        private readonly int width = 24;
        private readonly int height = 12;

        private Color gridColorA = new Color(20, 20, 21, 255);
        private Color gridColorB = new Color(28, 28, 30, 255);
        private Color gridBorderColor = new Color(60, 60, 63, 255);

        private Texture2D collisionTexture = Raylib.LoadTexture("..\\..\\..\\tetro48_collisionMap.png");

        public bool[] collisionMap = null!;

        public List<Piece> pieces = new List<Piece>();

        public void DrawBackground(int screenX, int screenY, int tileSize)
        {
            Raylib.DrawRectangle(screenX - tileSize / 8, screenY - tileSize / 8, tileSize * width + (tileSize / 4), tileSize * height + (tileSize / 4), gridBorderColor);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color col = (x + y) % 2 == 0 ? gridColorA : gridColorB;
                    Raylib.DrawRectangle(screenX + (x * tileSize), screenY + (y * tileSize), tileSize, tileSize, col);
                }
            }
        }

        public void DrawPieces(int screenX, int screenY, int angle)
        {
            foreach (Piece p in pieces)
            {
                p.Draw(screenX, screenY, width, angle, false);
            }
        }
        
        public void DrawCollisionMap(int screenX, int screenY, int tileSize)
        {
            for (int i = 0; i < collisionMap.Length; i++)
            {
                if (collisionMap[i])
                {
                    int x = i % width;
                    int y = i / width;
                    Rectangle srec = new Rectangle(0, 0, 8, 8);
                    Rectangle drec = new Rectangle(screenX + (x * tileSize), screenY + (y * tileSize), tileSize, tileSize);
                    Raylib.DrawTexturePro(collisionTexture, srec, drec, Vector2.Zero, 0, Color.White);
                }
            }
        }

        public bool IsColliding(Piece p, VecInt2 offset)
        {
            VecInt2 pCenter = GetTile(p.center, width) + offset;
            bool colliding = !IsInBounds(pCenter) || collisionMap[GetCell(pCenter, width)];
            int i = 0;
            while (colliding == false && i < p.blocks.Count)
            {
                pCenter = GetTile(p.center, width) + offset;
                VecInt2 blockTile = new VecInt2(pCenter.x + p.blocks[i].x, pCenter.y + p.blocks[i].y);
                if (IsInBounds(blockTile)) colliding = collisionMap[GetCell(blockTile, width)];
                else colliding = true;
                i++;
            }
            return colliding;
        }

        public void PlacePiece(Piece p)
        {
            pieces.Add(p);
            UpdateCollisionMap(p, true);
        }

        public void RemovePiece(Piece p)
        {
            bool removed = pieces.Remove(p);
            if (removed) UpdateCollisionMap(p, false);
        }

        public bool TryMovePiece(Piece p, VecInt2 translation)
        {
            bool moved = false;
            if (pieces.Contains(p)) UpdateCollisionMap(p, false);
            if (!IsColliding(p, translation))
            {
                p.Translate(translation, width);
                moved = true;
            }
            UpdateCollisionMap(p, true);
            return moved;
        }

        public void SortPiecesByLowestPoint(int gameAngle)
        {
            pieces = gameAngle switch
            {
                0 => pieces.OrderByDescending(p => p.boundsMax.y).ToList(),
                1 => pieces.OrderByDescending(p => p.boundsMax.x).ToList(),
                2 => pieces.OrderBy(p => p.boundsMin.y).ToList(),
                3 => pieces.OrderBy(p => p.boundsMin.x).ToList(),
                _ => throw new Exception($"Invalid game angle: {gameAngle}")
            };
                
        }

        public void ClearCollisionMap()
        {
            for (int i = 0; i < collisionMap.Length; i++)
            {
                collisionMap[i] = false;
            }
        }

        public void UpdateCollisionMap(Piece p, bool state)
        {
            collisionMap[p.center] = state;
            foreach (VecInt2 offset in p.blocks)
            {
                int cell = p.center + offset.x + offset.y * width;
                if (cell >= 0 && cell < collisionMap.Length)
                {
                    collisionMap[cell] = state;
                }
            }
        }

        public void RecalculateCollisionMap()
        {
            for (int i = 0; i < collisionMap.Length; i++)
            {
                collisionMap[i] = false;
            }
            
            foreach (Piece p in pieces)
            {
                UpdateCollisionMap(p, true);
            }
        }

        public static int GetCell(VecInt2 tile, int boardWidth)
        {
            return tile.x + boardWidth * tile.y;
        }
        
        public static VecInt2 GetTile(int cell, int boardWidth)
        {
            return new VecInt2(cell % boardWidth, cell / boardWidth);
        }

        public bool IsInBounds(VecInt2 tile)
        {
            return tile.x >= 0 && tile.y >= 0 && tile.x < width && tile.y < height;
        }

        public Board(int w, int h)
        {
            width = w;
            height = h;
            collisionMap = new bool[w * h];
        }
    }
}