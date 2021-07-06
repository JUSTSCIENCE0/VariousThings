using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;

namespace KeyManagmentServer
{
    class NumberGenerator
    {
        private BigInteger p, g;
        private Random rnd;

        public BigInteger P
        {
            get
            {
                return p;
            }
        }

        public BigInteger G
        {
            get
            {
                return g;
            }
        }

        public NumberGenerator()
        {
            long sid = DateTime.Now.Ticks;
            rnd = new Random((int)sid);

            InitNums();
        }

        public void InitNums()
        {
            Console.WriteLine("Start Generate 1024-bit p");
            DateTime start = DateTime.Now;

            int keysize = 128;
            p = GenSimple(keysize);

            /*while(!IsSimple(p))
            {
                p = GenSimple(keysize);
            }*/

            DateTime end = DateTime.Now;
            TimeSpan delta = end - start;

            Console.WriteLine("Generated p = {0}, for {1} ms.", p, delta.TotalMilliseconds);
            Console.WriteLine("Lenght p = {0}", p.ToString().Length);

            start = DateTime.Now;
            g = GenG(p);
            end = DateTime.Now;
            delta = end - start;
            Console.WriteLine("Generated g = {0}, for {1} ms.", g, delta.TotalMilliseconds);
            Console.WriteLine("Lenght g = {0}", g.ToString().Length);
        }

        private bool IsSimple(BigInteger x)
        {
            if (x == 2)
                return true;
            for (int i=0; i<100; i++)
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
            BigInteger Num = new BigInteger(RowNum, true);
            Num %= max;
            return Num;
        }

        private BigInteger GenSimple(int size)
        {
            byte[] RowNum = new byte[32];
            rnd.NextBytes(RowNum);
            BigInteger Num = new BigInteger(RowNum, true);
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
                BigInteger N = new BigInteger(RowNum, true);
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

        private BigInteger GenG(BigInteger P)
        {
            List<BigInteger> fact = new List<BigInteger>();
            BigInteger phi = P - 1, n = phi;
            for (BigInteger i=2; i*i<=n; i = 2*i - 1)
            {
                //if (IsSimple(n))
                //    break;
                if (n % i == 0)
                {
                    fact.Add(i);
                    while (n % i == 0)
                    {
                        n /= i;
                    }
                    if (IsSimple(n))
                        break;
                }
            }

            if (n > 1)
                fact.Add(n);

            for (BigInteger res = 11; res <= P; ++res)
            {
                if (!IsSimple(res))
                    continue;
                bool ok = true;
                for (int i =0; i<fact.Count && ok; ++i)
                {
                    if (BigInteger.ModPow(res, phi / fact[i], P) == 1)
                        ok = false;
                    
                }
                if (ok)
                    return res;
            }

            return -1;
        }
    }
}
