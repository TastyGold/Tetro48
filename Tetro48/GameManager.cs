using Raylib_cs;

namespace Tetro48
{
    internal static partial class GameManager
    {
        public static int boardWidth = 12;
        public static int boardHeight = 12;

        public static int boardScreenPosX = 100 - boardWidth * 4;
        public static int boardScreenPosY = 75 - boardWidth * 4;
        public const int boardTileSize = 8;

        public static int boardWorldX = boardTileSize * boardWidth / -2;
        public static int boardWorldY = boardTileSize * boardHeight / -2;

        public static int screenScale = 8;

        public static Board board = null!;
        public static DropZone dropZone = null!;
        public static BoardCamera boardCam = new BoardCamera(screenScale);
        public static GameStateController state = new GameStateController();
        public static NextQueue nextQueue = null!;

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

        public static VecInt2 mouseTile;

        public static List<Piece> fallingPieces = new List<Piece>();
        public static List<Piece> landedPieces = new List<Piece>();
        public static List<Piece> mergedPieces = new List<Piece>();
        public static List<int> clearedLines = new List<int>();

        public static List<VecInt2> clearedPieces = new List<VecInt2>();
        public static Color pieceClearColor = new Color(220, 220, 220, 255);
        public static int clearFlashRate = 8;
        public static int clearAnimDuration = 24; 

        public static int piecePopThreshold = 14;

        public static bool enableDebugOverlay = false;
        public static KeyboardKey debugEnableKey = KeyboardKey.F3;

        public static bool updateQueue = false;

        public static void Begin()
        {
            Raylib.InitWindow(200 * screenScale, 150 * screenScale, "Tetro48");
            Raylib.SetTargetFPS(60);
            PieceTypes.Initalise();

            nextQueue = new NextQueue(5);
            nextQueue.Initialise();

            board = new Board(boardWidth, boardHeight);
            dropZone = new DropZone(3, 0);

            heldPiece = PieceTypes.GetNewPiece(nextQueue.randomiser.GetNextPiece(true), 65, 0);
        }

        public static void Update()
        {
            UpdateGameState();

            if (Raylib.IsKeyPressed(KeyboardKey.R)) state.SetNextGameState(GameState.PlayerInControl);
            if (Raylib.IsKeyPressed(debugEnableKey)) enableDebugOverlay = !enableDebugOverlay;

            if (state.currentState == GameState.PlayerInControl) HandlePlayerControls();
            if (state.currentState == GameState.Rotating) boardCam.Update();
            if (state.currentState == GameState.PiecesFalling) HandleFallingPieces();
            if (state.currentState == GameState.PieceClear) HandlePieceClearing();
        }

        public static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(40, 40, 44, 255));
            DrawUI();

            Raylib.BeginMode2D(boardCam.camera);
            DrawBoardAndPieces();
            if (enableDebugOverlay) DrawDebugOverlay();
            Raylib.EndMode2D();

            if (enableDebugOverlay) DrawDebugUI();
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

        public static void DrawBoardAndPieces()
        {
            board.DrawBackground(boardWorldX, boardWorldY, boardTileSize);
            if (state.currentState == GameState.PlayerInControl) dropZone.DrawSmart(boardWorldX, boardWorldY, boardWidth, boardHeight, boardTileSize, dropZone.InZoneBounds(mouseTile, boardWidth, boardHeight));
            board.DrawPieces(boardWorldX, boardWorldY, gameAngle);
            if (state.currentState == GameState.PlayerInControl)
            {
                GhostPiece.Draw(board, heldPiece, gameAngle, boardWorldX, boardWorldY, boardWidth);
                heldPiece.Draw(boardWorldX, boardWorldY, boardWidth, gameAngle, !canPlacePiece);
            }
            if (state.currentState == GameState.PieceClear) DrawPieceClearing();
        }
        public static void DrawUI()
        {
            nextQueue.Draw(screenScale);
        }
        public static void DrawDebugOverlay()
        {
            //heldPiece.DrawBounds(boardWorldX, boardWorldY, boardWidth, boardTileSize);
            //HighlightLandedPieces();
            board.DrawPieceCenters(boardWorldX, boardWorldY);
            if (state.currentState == GameState.PlayerInControl) heldPiece.DrawCenter(boardWorldX, boardWorldY, boardWidth);
            HighlightMergedPieces();
            board.DrawCollisionMap(boardWorldX, boardWorldY, boardTileSize);
        }
        public static void DrawDebugUI()
        {
            Raylib.DrawText($"GameAngle: {gameAngle}", screenScale, screenScale, 5 * screenScale, Color.White);
            Raylib.DrawText($"GameState: {Enum.GetName(typeof(GameState), state.currentState)}", screenScale, 7 * screenScale, 5 * screenScale, Color.White);
        }
        public static void DrawPieceClearing()
        {
            if (state.frameCounter % (clearFlashRate * 2) < clearFlashRate)
            {
                for (int i = 0; i < clearedPieces.Count; i++)
                {
                    Raylib.DrawRectangle(boardWorldX + clearedPieces[i].x * boardTileSize, boardWorldY + clearedPieces[i].y * boardTileSize, boardTileSize, boardTileSize, pieceClearColor);
                }
            }
        }

        public static void HandlePlayerControls()
        {
            if (updateQueue)
            {
                nextQueue.AdvanceQueue();
                updateQueue = false;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.E)) heldPiece.Rotate(3, false);
            if (Raylib.IsKeyPressed(KeyboardKey.Q)) heldPiece.Rotate(1, false);

            mouseTile = boardCam.GetMouseTile(boardWidth, boardHeight, boardTileSize);
            VecInt2 pieceTile = GetMouseToPieceTile(mouseTile);

            int pieceCell = Board.GetCell(pieceTile, boardWidth);
            heldPiece.center = pieceCell;

            canPlacePiece = dropZone.InZoneBounds(pieceTile, boardWidth, boardHeight) && !board.IsColliding(heldPiece, VecInt2.Zero);

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
                bool pieceCanFall = !board.IsColliding(heldPiece, gravityVectors[gameAngle]);

                board.pieces.Add(heldPiece);

                if (pieceCanFall)
                {
                    state.SetGameState(GameState.PiecesFalling);
                }
                else
                {
                    landedPieces.Clear();
                    landedPieces.Add(heldPiece);
                    board.UpdateCollisionMap(heldPiece, true);
                    HandleLandedPieces(out _);
                }

                heldPiece = nextQueue.GetNextPiece(pieceCell, gameAngle);
                updateQueue = true;
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
                landedPieces.Clear();

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
                        if (blocksMoved[i]) continue;

                        if (!IsTranslatedPositionValid(board.pieces[i].center) || board.IsColliding(board.pieces[i], VecInt2.Zero))
                        {
                            board.pieces[i].Translate(VecInt2.Zero - gravityVectors[gameAngle], boardWidth);
                            board.UpdateCollisionMap(board.pieces[i], true);
                            blocksMoved[i] = true;
                            changeMade = true;
                            if (fallingPieces.Contains(board.pieces[i])) landedPieces.Add(board.pieces[i]);
                        }
                    }
                }

                fallingPieces.Clear();
                for (int i = 0; i < board.pieces.Count; i++)
                {
                    if (!blocksMoved[i]) fallingPieces.Add(board.pieces[i]);
                }

                bool returnPlayerControl = true;
                if (landedPieces.Count > 0)
                {
                    HandleLandedPieces(out returnPlayerControl);
                }

                if (blocksMoved.AreAll(true) && returnPlayerControl) state.SetNextGameState(GameState.PlayerInControl);
            }
        }

        public static bool IsTranslatedPositionValid(int cell)
        {
            int x = cell % boardWidth;

            return gameAngle switch
            {
                1 => x != 0,
                3 => x != boardWidth - 1,
                _ => true
            };
        }

        public static void HandleLandedPieces(out bool returnPlayerControl)
        {
            returnPlayerControl = true;

            //check for piece merges
            mergedPieces.Clear();
            for (int i = 0; i < landedPieces.Count; i++)
            {
                if (!board.pieces.Contains(landedPieces[i])) continue;

                VecInt2 paddedMin = landedPieces[i].GetPaddedBoundsMin();
                VecInt2 paddedMax = landedPieces[i].GetPaddedBoundsMax();

                int j = 0;
                while (j < board.pieces.Count)
                {
                    if (landedPieces[i].color != board.pieces[j].color || 
                        landedPieces[i] == board.pieces[j] || 
                        !board.pieces[j].IsIntersectingBounds(paddedMin, paddedMax))
                    {
                        j++;
                    }
                    else if (landedPieces[i].IsTouching(board.pieces[j], boardWidth))
                    {
                        landedPieces[i].MergeWith(board.pieces[j], boardWidth);
                        board.pieces.RemoveAt(j);

                        if (!mergedPieces.Contains(landedPieces[i])) mergedPieces.Add(landedPieces[i]);
                    }
                    else j++;
                }
            }

            //check for line clears
            bool linesAreHorizontal = gameAngle % 2 == 0;
            int iMax = linesAreHorizontal ? boardWidth : boardHeight;
            clearedLines.Clear();
            for (int i = 0; i < iMax; i++)
            {
                bool lineCleared = board.IsLineComplete(i, linesAreHorizontal);

                if (lineCleared)
                {
                    Console.WriteLine($"Cleared line {i}");
                    returnPlayerControl = false;
                    clearedLines.Add(i);
                }
            }

            //filter for piece pops
            int mi = 0;
            while (mi < mergedPieces.Count)
            {
                if (mergedPieces[mi].Size >= piecePopThreshold)
                {
                    mi++;
                    returnPlayerControl = false;
                }
                else
                {
                    mergedPieces.RemoveAt(mi);
                }
            }

            //calculate pieces cleared
            if (clearedLines.Count > 0 || mergedPieces.Count > 0)
            {
                clearedPieces.Clear();

                for (int i = 0; i < mergedPieces.Count; i++)
                {
                    VecInt2 pCenter = mergedPieces[i].GetCenterTile(boardWidth);
                    for (int j = 0; j < mergedPieces[i].blocks.Count; j++)
                    {
                        VecInt2 tile = pCenter + mergedPieces[i].blocks[j];
                        if (!clearedPieces.Contains(tile))
                        {
                            clearedPieces.Add(tile);
                        }
                    }
                }

                for (int i = 0; i < clearedLines.Count; i++)
                {
                    for (int j = 0; j < iMax; j++)
                    {
                        int x = linesAreHorizontal ? j : clearedLines[i];
                        int y = linesAreHorizontal ? clearedLines[i] : j;
                        VecInt2 tile = new VecInt2(x, y);
                        if (!clearedPieces.Contains(tile))
                        {
                            clearedPieces.Add(tile);
                        }
                    }
                }

                state.SetGameState(GameState.PieceClear);
            }
        }

        public static void HandlePieceClearing()
        {
            if (state.frameCounter >= clearAnimDuration)
            {
                board.CalculateColorMap();

                for (int i = 0; i < clearedPieces.Count; i++)
                {
                    board.colorMap[clearedPieces[i].x, clearedPieces[i].y] = -1;
                }

                board.ResetPiecesFromColorMap();

                landedPieces.Clear();
                mergedPieces.Clear();
                board.RecalculateCollisionMap();

                state.SetGameState(GameState.PiecesFalling);
            }
        }

        public static void HighlightLandedPieces()
        {
            foreach (Piece p in landedPieces)
            {
                p.Highlight(boardWorldX, boardWorldY, boardWidth, new Color(255, 0, 0, 140));
            }
        }

        public static void HighlightMergedPieces()
        {
            foreach (Piece p in mergedPieces)
            {
                p.Highlight(boardWorldX, boardWorldY, boardWidth, new Color(0, 255, 0, 140));
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
                state.SetNextGameState(GameState.PiecesFalling);
            }

            if (state.currentState != state.nextState) state.SetGameState(state.nextState);
        }
    }
}