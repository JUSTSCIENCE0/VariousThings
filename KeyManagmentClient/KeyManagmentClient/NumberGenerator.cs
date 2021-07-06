using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyManagmentClient
{
    class NumberGenerator
    {
        private BigInteger p, q, n, v, s;
        private Random rnd;

        public BigInteger P
        {
            get
            {
                return p;
            }
        }

        public BigInteger Q
        {
            get
            {
                return q;
            }
        }

        public BigInteger V
        {
            get
            {
                return v;
            }
        }

        public BigInteger N
        {
            get
            {
                return n;
            }
        }

        public BigInteger S
        {
            get
            {
                return s;
            }
        }

        public NumberGenerator(ref TextBox logConsole)
        {
            long sid = DateTime.Now.Ticks;
            rnd = new Random((int)sid);

            InitNums(ref logConsole);
        }

        public void InitNums(ref TextBox logConsole)
        {
            DateTime start = DateTime.Now;
            int keysize = 128;

            p = GenSimple(keysize);
            q = GenSimple(keysize);

            DateTime end = DateTime.Now;
            TimeSpan delta = end - start;

            logConsole.Text += "Generated p = " + p.ToString() + Environment.NewLine;
            logConsole.Text += "Generated q = " + q.ToString() + Environment.NewLine;
            logConsole.Text += "Generation time = " + delta.TotalMilliseconds.ToString() + " ms." + Environment.NewLine;
            logConsole.Update();

            n = p * q;
            logConsole.Text += "n = " + n.ToString() + Environment.NewLine;

            bool Vgenerated = false;
            BigInteger x = new BigInteger();
            BigInteger r_v = new BigInteger();
            BigInteger check;

            while (!Vgenerated)
            {
                logConsole.Text += "Start generate 1024-bit x" + Environment.NewLine;
                logConsole.Update();
                x = GenSimple(128);
                logConsole.Text += "Generated x = " + x.ToString() + Environment.NewLine;
                v = (x * x) % n;
                logConsole.Text += "v = (x * x) % n = " + v.ToString() + Environment.NewLine;
                r_v = FindInverse(v, n);
                logConsole.Text += "v^(-1) = " + r_v.ToString() + Environment.NewLine;
                check = (v * r_v) % n;
                logConsole.Text += "check v*v^(-1) MOD n = " + check.ToString() + Environment.NewLine;
                if (check == 1)
                    Vgenerated = true;
                else
                    logConsole.Text += "Check failed, try again!" + Environment.NewLine;
            }

            BigInteger r_x = FindInverse(x, n);
            logConsole.Text += "x^(-1) = " + r_x.ToString() + Environment.NewLine;

            check = (r_x * r_x) % n;
            if (check == r_v)
            {
                logConsole.Text += "check x^(-2) MOD n == v^(-1) - success" + Environment.NewLine;
                s = r_x;
            }
            else
            {
                logConsole.Text += "check x^(-2) MOD n == v^(-1) - failed" + Environment.NewLine;
                throw new Exception("Something done wrong, try again!");
            }
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
            byte[] RowNum = new byte[128];
            rnd.NextBytes(RowNum);
            BigInteger Num = new BigInteger(RowNum);
            if (Num < 0) Num = -Num;
            if (Num % 2 == 0)
                Num++;

            while (!IsSimple(Num))
            {
                Num += 2;
            }

            /*int t = 64;
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
            }*/


            size = Num.ToByteArray().Length;
            return Num;
        }

        private BigInteger EuclidAlgoritm(BigInteger a, BigInteger b, ref BigInteger x, ref BigInteger y)
        {
            if (a == 0)
            {
                x = 0; y = 1;
                return b;
            }
            BigInteger x1 = new BigInteger();
            BigInteger y1 = new BigInteger();
            BigInteger d = EuclidAlgoritm(b % a, a, ref x1, ref y1);

            x = y1 - (b / a) * x1;
            y = x1;
            return d;
        }

        private BigInteger FindInverse(BigInteger a, BigInteger n)
        {
            BigInteger x = new BigInteger();
            BigInteger y = new BigInteger();
            BigInteger g = EuclidAlgoritm(a, n, ref x, ref y);
            if (g != 1)
            {
                return 0;
            }
            else
            {
                if (x < 0)
                    return n + x;
                else
                    return x % n;
            }
        }
    }
}
