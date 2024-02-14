using Raylib_cs;

namespace Tetro48
{
    internal static partial class GameManager
    {
        public static int boardWidth = 12;
        public static int boardHeight = 12;

        public static int boardScreenPosX = (100 - boardWidth * 4);
        public static int boardScreenPosY = (75 - boardWidth * 4);
        public const int boardTileSize = 8;

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

        public static GameStateController state = new GameStateController();

        public static void Begin()
        {
            Raylib.InitWindow(200 * screenScale, 150 * screenScale, "Tetr048");
            Raylib.SetTargetFPS(60);
            PieceTypes.Initalise();

            board = new Board(boardWidth, boardHeight);
            dropZone = new DropZone(3, 0);

            heldPiece = PieceTypes.GetNewPiece(2, 65, 0);
        }

        public static void Update()
        {
            UpdateGameState();
            if (Raylib.IsKeyPressed(KeyboardKey.R)) state.SetGameState(GameState.PlayerInControl);

            if (state.currentState == GameState.PlayerInControl) HandlePlayerControls();
            if (state.currentState == GameState.Rotating) boardCam.Update();
            if (state.currentState == GameState.PiecesFalling) HandleFallingPieces();
        }

        public static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(40, 40, 44, 255));

            Raylib.BeginMode2D(boardCam.camera);
            board.DrawBackground(boardWorldX, boardWorldY, boardTileSize);
            if (state.currentState == GameState.PlayerInControl) dropZone.DrawSmart(boardWorldX, boardWorldY, boardWidth, boardHeight, boardTileSize, dropZone.InZoneBounds(mouseTile, boardWidth, boardHeight));
            board.DrawPieces(boardWorldX, boardWorldY, gameAngle);
            if (state.currentState == GameState.PlayerInControl) heldPiece.Draw(boardWorldX, boardWorldY, boardWidth, gameAngle, !canPlacePiece);
            //heldPiece.DrawBounds(boardWorldX, boardWorldY, boardWidth, boardTileSize);
            //board.DrawCollisionMap(boardWorldX, boardWorldY, boardTileSize);
            Raylib.EndMode2D();

            Raylib.DrawText($"GameAngle: {gameAngle}", 10, 10, 5*screenScale, Color.White);
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
       
        public static void HandlePlayerControls()
        {
            mouseTile = boardCam.GetMouseTile(boardWidth, boardHeight, boardTileSize);
            VecInt2 pieceTile = GetMouseToPieceTile(mouseTile);

            int pieceCell = Board.GetCell(pieceTile, boardWidth);
            heldPiece.center = pieceCell;

            canPlacePiece = dropZone.InZoneBounds(pieceTile, boardWidth, boardHeight) && !board.IsColliding(heldPiece, VecInt2.Zero);

            if (Raylib.IsKeyPressed(KeyboardKey.E)) heldPiece.Rotate(3, false);
            if (Raylib.IsKeyPressed(KeyboardKey.Q)) heldPiece.Rotate(1, false);


            if (Raylib.IsKeyPressed(KeyboardKey.Right))
            {
                boardCam.Rotate(true);
                heldPiece.Rotate(1, true);
                gameAngle++;
                state.SetGameState(GameState.Rotating);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Left))
            {
                boardCam.Rotate(false);
                heldPiece.Rotate(3, true);
                gameAngle--;
                state.SetGameState(GameState.Rotating);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Up))
            {
                boardCam.Rotate180(true);
                heldPiece.Rotate(2, true);
                gameAngle += 2;
                state.SetGameState(GameState.Rotating);
            }

            if (Raylib.IsMouseButtonPressed(0))
            {
                board.PlacePiece(heldPiece);
                heldPiece = PieceTypes.GetNewPiece(rand.Next(12), pieceCell, gameAngle);

                state.SetGameState(GameState.PiecesFalling);
            }

            if (gameAngle < 0) gameAngle += 4;
            if (gameAngle >= 4) gameAngle -= 4;
            dropZone.angle = gameAngle;
        }

        public static void HandleFallingPieces()
        {
            if (state.frameCounter % 3 == 0)
            {
                board.ClearCollisionMap();

                for (int i = 0; i < board.pieces.Count; i++)
                {
                    board.pieces[i].Translate(gravityVectors[gameAngle], boardWidth);
                }

                bool[] blocksMoved = new bool[board.pieces.Count];
                bool changeMade = true;
                while (changeMade == true && !blocksMoved.AreAll(true))
                {
                    changeMade = false;

                    for (int i = 0; i < board.pieces.Count; i++)
                    {
                        if (board.IsColliding(board.pieces[i], VecInt2.Zero) && !blocksMoved[i])
                        {
                            board.pieces[i].Translate(VecInt2.Zero - gravityVectors[gameAngle], boardWidth);
                            board.UpdateCollisionMap(board.pieces[i], true);
                            blocksMoved[i] = true;
                            changeMade = true;
                        }
                    }
                }

                if (blocksMoved.AreAll(true)) state.SetGameState(GameState.PlayerInControl);
            }
        }

        public static bool AreAll(this bool[] arr, bool state)
        {
            bool areAll = true;
            int i = 0;
            while (areAll == true && i < arr.Length)
            {
                areAll = arr[i] == state;
                i++;
            }
            return areAll;
        }

        public static void UpdateGameState()
        {
            state.frameCounter++;

            if (state.currentState == GameState.Rotating && boardCam.TargetAngleReached())
            {
                state.SetGameState(GameState.PiecesFalling);
            }
        }
    }
}