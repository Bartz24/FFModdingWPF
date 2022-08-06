using Bartz24.Data;
using System.Linq;

namespace Bartz24.FF12
{
    public class DataStoreLicenseBoard : DataStore
    {
        protected byte[] header;
        public ushort[,] Board { get; set; }

        public override void LoadData(byte[] data, int offset = 0)
        {
            header = data.SubArray(0, 8);

            byte[] boardData = data.SubArray(8, data.Length - 8);

            Board = new ushort[24, 24];
            for (int x = 0; x < 24; x++)
            {
                for (int y = 0; y < 24; y++)
                {
                    Board[y, x] = boardData.ReadUShort(y * 24 * 2 + x * 2);
                }
            }
        }

        public override byte[] Data
        {
            get
            {
                byte[] boardData = new byte[24 * 24 * 2];
                for (int x = 0; x < 24; x++)
                {
                    for (int y = 0; y < 24; y++)
                    {
                        boardData.SetUShort(y * 24 * 2 + x * 2, Board[y, x]);
                    }
                }
                return header.Concat(boardData);
            }
        }

        public override int GetDefaultLength()
        {
            return -1;
        }
    }
}
