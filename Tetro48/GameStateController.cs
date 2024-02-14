namespace Tetro48
{
    public class GameStateController
    {
        public GameState currentState = GameState.PlayerInControl;
        public bool playerHasControl = true;

        public int frameCounter = 0;

        public void SetGameState(GameState state)
        {
            currentState = state;
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
    }

    public enum GameState
    {
        PlayerInControl,
        Rotating,
        PiecesFalling,
        LineClear,
    }
}