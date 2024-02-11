using Raylib_cs;

namespace Tetro48
{
    internal class Board
    {
        private int width = 24;
        private int height = 12;

        private Color gridColorA = new Color(20, 20, 21, 255);
        private Color gridColorB = new Color(28, 28, 30, 255);
        private Color gridBorderColor = new Color(60, 60, 63, 255);

        public int[,] board = null!;

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

        public void DrawPieces(int screenX, int screenY, int tileSize, int boardWidth)
        {
            foreach (Piece p in pieces)
            {
                p.Draw(screenX, screenY, tileSize, boardWidth);
            }
        }

        public void PlacePiece(Piece p)
        {
            pieces.Add(p);
        }
        
        public Board(int w, int h)
        {
            width = w;
            height = h;
            board = new int[w, h];
        }
    }
}