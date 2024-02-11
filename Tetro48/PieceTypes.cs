using System.IO;

namespace Tetro48
{
    internal static class PieceTypes
    {
        public static string dataPath = "..\\..\\..\\tetro48_pieceData.txt";

        private static List<List<VecInt2>> pieceData = new();

        public static void Initalise()
        {
            string[] lines = File.ReadAllLines(dataPath);
            for (int i = 0; i < lines.Length; i++)
            {
                List<VecInt2> blocks = new();
                string[] values = lines[i].Split(',');
                for (int j = 0; j < values.Length; j += 2)
                {
                    blocks.Add(new VecInt2(Convert.ToInt32(values[j]), Convert.ToInt32(values[j + 1])));
                }
                pieceData.Add(blocks);
            }
        }

        public static Piece GetNewPiece(int id, int center, int rotation)
        {
            if (id >= pieceData.Count) throw new Exception($"Piece ID {id} not found");

            Piece p = new Piece();
            p.color = id;
            p.center = center;
            for (int i = 0; i < pieceData[id].Count; i++)
            {
                p.blocks.Add(VecInt2.Rotate(pieceData[id][i], rotation));
            }

            return p;
        }
    }
}