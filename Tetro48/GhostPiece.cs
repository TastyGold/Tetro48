using Raylib_cs;
using System.Numerics;

namespace Tetro48
{
    internal static class GhostPiece
    {
        public static Texture2D ghostPieceTexture = Raylib.LoadTexture("..\\..\\..\\tetro48_ghostPiece.png");
        public static Color ghostColor = new Color(255, 255, 255, 155);

        public static void Draw(Board board, Piece piece, int angle, int boardX, int boardY, int boardWidth)
        {
            VecInt2 gravity = GameManager.gravityVectors[angle];
            int originalPosition = piece.center;
            bool canMove = !board.IsColliding(piece, gravity);
            while (canMove)
            {
                piece.Translate(gravity, boardWidth);
                canMove = !board.IsColliding(piece, gravity);
            }
            foreach (VecInt2 offset in piece.blocks)
            {
                VecInt2 tile = piece.GetCenterTile(boardWidth) + offset;
                Rectangle srec = new Rectangle(0, 0, 8, 8);
                Rectangle drec = new Rectangle(boardX + tile.x * Piece.resolution + Piece.resolution * 0.5f, boardY + tile.y * Piece.resolution + Piece.resolution * 0.5f, Piece.resolution, Piece.resolution);
                Raylib.DrawTexturePro(ghostPieceTexture, srec, drec, Vector2.One * Piece.resolution * 0.5f, angle * -90, ghostColor);
            }
            piece.center = originalPosition;
        }
    }
}