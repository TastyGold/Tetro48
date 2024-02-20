namespace Tetro48
{
    internal class NextQueue
    {
        public readonly int size = 5;
        public int[] pieces = null!;
        public int nextPieceIndex = 0;

        public PieceBagRandomiser randomiser = new PieceBagRandomiser();
        public NextQueueRenderer renderer = new NextQueueRenderer();

        public void Initialise()
        {
            for (int i = 0; i < 5; i++)
            {
                pieces[i] = randomiser.GetNextPiece(true);
            }
        }

        public void Draw(int screenScale)
        {
            renderer.Draw(pieces, nextPieceIndex, screenScale);
        }

        public Piece GetNextPiece(int center, int angle)
        {
            return PieceTypes.GetNewPiece(pieces[nextPieceIndex], center, angle);
        }
        public void AdvanceQueue()
        {
            pieces[nextPieceIndex] = randomiser.GetNextPiece();

            nextPieceIndex++;
            if (nextPieceIndex >= size) nextPieceIndex = 0;
        }

        public NextQueue(int size)
        {
            this.size = size;
            pieces = new int[size];
        }
    }
}