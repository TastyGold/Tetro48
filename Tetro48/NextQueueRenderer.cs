using Raylib_cs;
using System.Numerics;

namespace Tetro48
{
    internal class NextQueueRenderer
    {
        public Texture2D piecePreviews = Raylib.LoadTexture("..\\..\\..\\tetro48_nextQueuePreviews.png");

        public readonly int screenPosX = 155;
        public readonly int screenPosY = 17;
        public readonly int resolution = 6;
        public readonly int tileWidth = 5;

        public void Draw(int[] queue, int startIndex, int screenScale)
        {
            DrawBackground(queue, screenScale);
            DrawPieces(queue, startIndex, screenScale);
        }

        public void DrawBackground(int[] queue, int screenScale)
        {
            int height = GetBackgroundHeight(queue);
            Raylib.DrawRectangle(
                (screenPosX - 1) * screenScale,
                (screenPosY - 1) * screenScale,
                (tileWidth * resolution + 2) * screenScale,
                (height * resolution + 2) * screenScale,
                Board.gridBorderColor
                );

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < tileWidth; x++)
                {
                    int sx = screenPosX + x * resolution;
                    int sy = screenPosY + y * resolution;
                    Color col = (x + y) % 2 == 0 ? Board.gridColorA : Board.gridColorB;
                    Raylib.DrawRectangle(sx * screenScale, sy * screenScale, resolution * screenScale, resolution * screenScale, col);
                }
            }
        }

        public void DrawPieces(int[] queue, int index, int screenScale)
        {
            int tileY = 1;

            for (int i = 0; i < queue.Length; i++)
            {
                DrawPiece(queue[index], tileY, screenScale);

                tileY += GetPiecePreviewHeight(queue[index]);
                index++;
                if (index >= queue.Length) index = 0;
            }
        }

        public void DrawPiece(int id, int tileY, int screenScale)
        {
            Rectangle srec = new Rectangle(id * 24, 0, 24, 18);
            Rectangle drec = new Rectangle(
                (screenPosX + resolution / 2) * screenScale,
                (screenPosY + tileY * resolution) * screenScale,
                24 * screenScale, 18 * screenScale);
            Raylib.DrawTexturePro(piecePreviews, srec, drec, Vector2.Zero, 0, Color.White);
        }

        public int GetBackgroundHeight(int[] queue)
        {
            int height = 1;
            for (int i = 0; i < queue.Length; i++)
            {
                height += GetPiecePreviewHeight(queue[i]);
            }
            return height;
        }

        public static int GetPiecePreviewHeight(int id)
        {
            return id == 8 || id == 9 ? 4 : 3;
        }
    }
}