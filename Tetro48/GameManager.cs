using Raylib_cs;

namespace Tetro48
{
    internal static class GameManager
    {
        public static int boardWidth = 12;
        public static int boardHeight = 12;

        public static int boardScreenPosX = (100 - boardWidth * 4);
        public static int boardScreenPosY = (75 - boardWidth * 4);
        public static int boardTileSize = 8;

        public static int boardWorldX = boardTileSize * boardWidth / -2;
        public static int boardWorldY = boardTileSize * boardHeight / -2;

        public static int screenScale = 8;

        public static Board board = null!;
        public static DropZone dropZone = null!;

        public static int gameAngle = 0;
        public readonly static VecInt2[] gravityVectors =
        {
            new VecInt2(0, 1),
            new VecInt2(1, 0),
            new VecInt2(0, -1),
            new VecInt2(-1, 0),
        };

        public static Piece heldPiece = new Piece();
        public static bool canPlacePiece = false;

        public static Random rand = new Random();

        public static BoardCamera boardCam = new BoardCamera(screenScale);

        public static VecInt2 mouseTile;

        public static void Begin()
        {
            Raylib.InitWindow(200 * screenScale, 150 * screenScale, "Tetr048");
            Raylib.SetTargetFPS(60);
            PieceTypes.Initalise();

            board = new Board(boardWidth, boardHeight);
            dropZone = new DropZone(3, 0);

            heldPiece = PieceTypes.GetNewPiece(9, 65, 0);
        }

        public static void Update()
        {
            mouseTile = boardCam.GetMouseTile(boardWidth, boardHeight, boardTileSize);
            VecInt2 pieceTile = GetMouseToPieceTile(mouseTile);

            int pieceCell = Board.GetCell(pieceTile, boardWidth);
            heldPiece.center = pieceCell;

            canPlacePiece = dropZone.InZoneBounds(pieceTile, boardWidth, boardHeight) && !board.IsColliding(heldPiece);

            if (Raylib.IsKeyPressed(KeyboardKey.E)) heldPiece.Rotate(3, false);
            if (Raylib.IsKeyPressed(KeyboardKey.Q)) heldPiece.Rotate(1, false);

            if (Raylib.IsMouseButtonPressed(0))
            {
                board.PlacePiece(heldPiece);
                heldPiece = PieceTypes.GetNewPiece(rand.Next(12), pieceCell, gameAngle);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Right))
            {
                boardCam.Rotate(true);
                heldPiece.Rotate(1, true);
                gameAngle++;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Left))
            {
                boardCam.Rotate(false);
                heldPiece.Rotate(3, true);
                gameAngle--;
            }
            if (gameAngle < 0) gameAngle += 4;
            if (gameAngle >= 4) gameAngle -= 4;

            dropZone.angle = gameAngle;

            boardCam.Update();
        }

        public static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(40, 40, 44, 255));

            Raylib.BeginMode2D(boardCam.camera);
            board.DrawBackground(boardWorldX, boardWorldY, boardTileSize);
            dropZone.DrawSmart(boardWorldX, boardWorldY, boardWidth, boardHeight, boardTileSize, dropZone.InZoneBounds(mouseTile, boardWidth, boardHeight));
            board.DrawPieces(boardWorldX, boardWorldY, gameAngle);
            heldPiece.Draw(boardWorldX, boardWorldY, boardWidth, gameAngle, !canPlacePiece);
            //heldPiece.DrawBounds(boardWorldX, boardWorldY, boardWidth, boardTileSize);
            //board.DrawCollisionMap(boardWorldX, boardWorldY, boardTileSize);
            Raylib.EndMode2D();

            Raylib.EndDrawing();
        }

        public static void End()
        {
            Raylib.CloseWindow();
        }

        public static VecInt2 GetMouseToPieceTile(VecInt2 mouseTile)
        {
            VecInt2 tile = mouseTile;
            if (tile.x + heldPiece.boundsMin.x < 0) tile.x = -heldPiece.boundsMin.x;
            if (tile.y + heldPiece.boundsMin.y < 0) tile.y = -heldPiece.boundsMin.y;
            if (tile.x + heldPiece.boundsMax.x >= boardWidth) tile.x = boardWidth - heldPiece.boundsMax.x - 1;
            if (tile.y + heldPiece.boundsMax.y >= boardHeight) tile.y = boardHeight - heldPiece.boundsMax.y - 1;
            return tile;
        }
    }
}