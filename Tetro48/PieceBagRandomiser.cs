namespace Tetro48
{
    internal class PieceBagRandomiser
    {
        public static Random rand = new Random();

        public float easyBagProbability = 0.75f;

        public List<int> easyBag = new List<int>();
        public List<int> hardBag = new List<int>();

        public const int easyPieceCount = 7;
        public const int totalPieceCount = 12;

        public int GetNextPiece(bool guaranteeEasyPiece = false)
        {
            if (easyBag.Count <= 0) RefillBag(easyBag, 0, easyPieceCount - 1);
            if (hardBag.Count <= 0) RefillBag(hardBag, easyPieceCount, totalPieceCount - 1);

            List<int> bag = guaranteeEasyPiece || rand.NextSingle() < easyBagProbability ? easyBag : hardBag;

            int index = rand.Next(bag.Count);
            int id = bag[index];
            bag.RemoveAt(index);

            return id;
        }

        public static void RefillBag(List<int> bag, int minId, int maxId)
        {
            for (int i = minId; i <= maxId; i++)
            {
                bag.Add(i);
            }
        }
    }
}