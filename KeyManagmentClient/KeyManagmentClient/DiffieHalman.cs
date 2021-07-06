using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace KeyManagmentClient
{
    class DiffieHalman
    {
        public BigInteger a;
        public BigInteger p, g;
        private Random rnd;

        //private BigInteger key;

        public BigInteger A
        {
            get
            {
                return BigInteger.ModPow(g, a, p);
            }
        }

        /*public BigInteger Key
        {
            get
            {
                return key;
            }
        }*/

        public DiffieHalman(BigInteger P, BigInteger G)
        {
            p = P; g = G;
            rnd = new Random();
            a = GenSimple(64);
        }

        private bool IsSimple(BigInteger x)
        {
            if (x == 2)
                return true;
            for (int i = 0; i < 100; i++)
            {
                BigInteger a = GenNumber(x - 2) + 2;
                if (NOD(a, x) != 1)
                    return false;
                if (BigInteger.ModPow(a, x - 1, x) != 1)
                    return false;
            }
            return true;
        }

        private BigInteger NOD(BigInteger a, BigInteger b)
        {
            if (b == 0)
                return a;
            return NOD(b, a % b);
        }

        private BigInteger GenNumber(BigInteger max)
        {
            int size = max.ToByteArray().Length;
            byte[] RowNum = new byte[size];
            rnd.NextBytes(RowNum);
            BigInteger Num = new BigInteger(RowNum);
            if (Num < 0) Num = -Num;
            Num %= max;
            return Num;
        }

        private BigInteger GenSimple(int size)
        {
            byte[] RowNum = new byte[32];
            rnd.NextBytes(RowNum);
            BigInteger Num = new BigInteger(RowNum);
            if (Num < 0) Num = -Num;
            if (Num % 2 == 0)
                Num++;

            while (!IsSimple(Num))
            {
                Num += 2;
            }

            int t = 32;
            while (t < size)
            {
                RowNum = new byte[t];
                rnd.NextBytes(RowNum);
                BigInteger N = new BigInteger(RowNum);
                if (N < 0) N = -N;
                if (N % 2 != 0)
                    N++;
                do
                {
                    N += 2;
                } while (!IsSimple(N * Num + 1));
                Num = N * Num + 1;
                t *= 2;
            }


            size = Num.ToByteArray().Length;
            return Num;
        }
    }
}
