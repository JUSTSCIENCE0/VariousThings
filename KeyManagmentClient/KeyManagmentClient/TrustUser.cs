using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KeyManagmentClient
{
    struct TrustMessage
    {
        public string Text;
        public bool Out;
    }

    class TrustUser
    {
        public string login;

        private BigInteger DHKey;
        private BigInteger key;

        public BigInteger Key
        {
            get
            {
                return key;
            }
        }

        public TrustUser(string name, BigInteger dh, BigInteger p, BigInteger a)
        {
            login = name;
            DHKey = dh;

            key = BigInteger.ModPow(dh, a, p);
        }

        public List<TrustMessage> messages = new List<TrustMessage>();
    }
}
