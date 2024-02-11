using Raylib_cs;

namespace Tetro48
{
    internal static class GameManager
    {
        public static int boardWidth = 12;
        public static int boardHeight = 12;

        public static Board board = null!;

        public static int screenScale = 6;

        public static int boardScreenPosX = (100 - boardWidth * 4);
        public static int boardScreenPosY = (75 - boardWidth * 4);
        public static int boardTileSize = 8;

        public static int boardWorldX = boardTileSize * boardWidth / -2;
        public static int boardWorldY = boardTileSize * boardHeight / -2;

        public static Piece examplePiece = new Piece();
        public static int rotation = 0;

        public static Random rand = new Random();

        public static BoardCamera boardCam = new BoardCamera(screenScale);

        public static void Begin()
        {
            Raylib.InitWindow(200 * screenScale, 150 * screenScale, "Tetr048");
            Raylib.SetTargetFPS(60);
            PieceTypes.Initalise();

            board = new Board(boardWidth, boardHeight);

            examplePiece = PieceTypes.GetNewPiece(4, 65, 0);
        }

        public static void Update()
        {
            int cell = boardCam.GetMouseTile(boardWidth, boardHeight, boardTileSize);
            examplePiece.center = cell;

            if (Raylib.IsKeyPressed(KeyboardKey.E)) examplePiece.Rotate(3);
            if (Raylib.IsKeyPressed(KeyboardKey.Q)) examplePiece.Rotate(1);

            if (Raylib.IsMouseButtonPressed(0))
            {
                board.PlacePiece(examplePiece);
                examplePiece = PieceTypes.GetNewPiece(rand.Next(12), cell, 0);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Right)) boardCam.Rotate(true);
            if (Raylib.IsKeyPressed(KeyboardKey.Left)) boardCam.Rotate(false);

            boardCam.Update();
        }

        public static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(40, 40, 44, 255));
            Raylib.BeginMode2D(boardCam.camera);
            board.DrawBackground(boardWorldX, boardWorldY, boardTileSize);
            board.DrawPieces(boardWorldX, boardWorldY, screenScale, boardWidth);
            examplePiece.Draw(boardWorldX, boardWorldY, screenScale, boardWidth);
            Raylib.EndMode2D();
            Raylib.EndDrawing();
        }

        public static void End()
        {
            Raylib.CloseWindow();
        }
    }
}