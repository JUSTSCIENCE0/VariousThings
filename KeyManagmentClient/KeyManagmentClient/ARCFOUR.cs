using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace KeyManagmentClient
{
    class ARCFOUR
    {
        [DllImport("ARCFOUR.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitGenerator(byte[] Key, UInt16 length);

        [DllImport("ARCFOUR.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte NextBYTE();

        public ARCFOUR()
        {
            byte[] Key = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            InitGenerator(Key, 16);

            byte qw1 = NextBYTE();
            byte qw2 = NextBYTE();
            byte qw3 = NextBYTE();
        }

        public void NewKey(byte[] DHkey, long timestamp)
        {
            byte[] key = new byte[DHkey.Length + 8];
            byte[] newTS = BitConverter.GetBytes(timestamp);

            Array.Copy(DHkey, key, DHkey.Length);
            Array.Copy(newTS, 0, key, DHkey.Length, 8);

            InitGenerator(key, (UInt16)key.Length);
        }

        public byte[] ConjunctionWithRC(byte[] data)
        {
            byte[] res = data;

            for (int i = 0; i < res.Length; i++)
            {
                byte key = NextBYTE();
                res[i] ^= key;
            }

            return res;
        }
    }
}
