using Raylib_cs;
using System.Numerics;

namespace Tetro48
{
    internal class Board
    {
        private readonly int width = 24;
        private readonly int height = 12;

        public static Color gridColorA = new Color(20, 20, 21, 255);
        public static Color gridColorB = new Color(28, 28, 30, 255);
        public static Color gridBorderColor = new Color(60, 60, 63, 255);

        private Texture2D collisionTexture = Raylib.LoadTexture("..\\..\\..\\tetro48_collisionMap.png");

        public bool[] collisionMap = null!;
        public int[,] colorMap = null!;
        private int[,] floodFill = null!;

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

        public void DrawPieceCenters(int screenX, int screenY)
        {
            foreach (Piece p in pieces)
            {
                p.DrawCenter(screenX, screenY, width);
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
            if (!IsInBounds(pCenter)) return true;

            bool colliding = !IsInBounds(pCenter);
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

        public bool IsLineComplete(int line, bool horizontal)
        {
            bool complete = true;
            int i = 0;
            while (complete == true && i < (horizontal ? width : height))
            {
                int x = horizontal ? i : line;
                int y = horizontal ? line : i;
                complete &= collisionMap[GetCell(new VecInt2(x, y), width)];
                i++;
            }
            return complete;
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

        public void CalculateColorMap()
        {
            for (int x = 0; x < colorMap.GetLength(0); x++)
            {
                for (int y = 0; y < colorMap.GetLength(1); y++)
                {
                    colorMap[x, y] = -1;
                }
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                VecInt2 pCenter = pieces[i].GetCenterTile(width);
                for (int j = 0; j < pieces[i].blocks.Count; j++)
                {
                    VecInt2 tile = pCenter + pieces[i].blocks[j];
                    if (IsInBounds(tile))
                    {
                        colorMap[tile.x, tile.y] = pieces[i].color;
                    }
                }
            }
        }

        public void ResetPiecesFromColorMap()
        {
            pieces.Clear();
            for (int y = 0; y < colorMap.GetLength(1); y++)
            {
                for (int x = 0; x < colorMap.GetLength(0); x++)
                {
                    if (colorMap[x, y] >= 0)
                    {
                        ExtractPieceFromColorMap(x, y);
                    }
                }
            }
        }

        public void ExtractPieceFromColorMap(int startX, int startY)
        {
            //reset floodFill array
            for (int y = 0; y < floodFill.GetLength(1); y++)
            {
                for (int x = 0; x < floodFill.GetLength(0); x++)
                {
                    if (y < startY || (x < startX && y == startY))
                    {
                        floodFill[x, y] = -1; //confirmed negative
                    }
                    else if (y > startY || (x > startX && y == startY))
                    {
                        floodFill[x, y] = 0; //to be confirmed
                    }
                    else
                    {
                        floodFill[x, y] = 1; //confirmed positive
                    }
                }
            }

            //flood fill algorithm
            List<VecInt2> confirmedTiles = new List<VecInt2>();
            confirmedTiles.Add(new VecInt2(startX, startY));

            List<VecInt2> possibleTiles = new List<VecInt2>();
            if (startX < width - 1) possibleTiles.Add(new VecInt2(startX + 1, startY));
            if (startY < height - 1) possibleTiles.Add(new VecInt2(startX, startY + 1));

            while (possibleTiles.Count > 0)
            {
                if (colorMap[possibleTiles[0].x, possibleTiles[0].y] == colorMap[startX, startY])
                {
                    confirmedTiles.Add(possibleTiles[0]);
                    floodFill[possibleTiles[0].x, possibleTiles[0].y] = 1;
                    for (int i = 0; i < 4; i++)
                    {
                        VecInt2 offset = GameManager.gravityVectors[i];
                        if (IsInBounds(possibleTiles[0] + offset) && floodFill[possibleTiles[0].x + offset.x, possibleTiles[0].y + offset.y] == 0)
                        {
                            possibleTiles.Add(new VecInt2(possibleTiles[0].x + offset.x, possibleTiles[0].y + offset.y));
                        }
                    }
                }
                
                possibleTiles.RemoveAt(0);
            }

            Piece newPiece = new Piece();
            newPiece.color = colorMap[startX, startY];
            VecInt2 pCenter = new VecInt2(startX, startY);
            newPiece.center = GetCell(pCenter, width);
            foreach (VecInt2 tile in confirmedTiles)
            {
                VecInt2 offset = tile - pCenter;
                newPiece.blocks.Add(offset);
                colorMap[tile.x, tile.y] = -1;
            }
            pieces.Add(newPiece);
        }

        public void PrintMapToConsole(int[,] map)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    string color = /*map[x, y] < 0 ? "  " : */map[x, y].ToString();
                    if (color.Length < 2) color = " " + color;
                    Console.Write(color + " ");
                    Console.Write(' ');
                }
                Console.WriteLine();
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
            colorMap = new int[w, h];
            floodFill = new int[w, h];
        }
    }
}