namespace Tetro48
{
    internal class GameStateController
    {
        public GameState currentState = GameState.PlayerInControl;
        public GameState nextState = GameState.PlayerInControl;
        public bool playerHasControl = true;

        public int frameCounter = 0;

        public void SetGameState(GameState state)
        {
            currentState = state;
            nextState = state;
            frameCounter = 0;
            switch (state)
            {
                case GameState.PlayerInControl:
                    playerHasControl = true;
                    break;
                case GameState.Rotating:
                    playerHasControl = false;
                    break;
                case GameState.PiecesFalling:
                    playerHasControl = false;
                    break;
            }
        }

        public void SetNextGameState(GameState state)
        {
            nextState = state;
        }
    }

    internal enum GameState
    {
        PlayerInControl,
        Rotating,
        PiecesFalling,
        PieceClear,
        PieceMerging
    }
}