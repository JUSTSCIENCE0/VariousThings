using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Numerics;

namespace KeyManagmentServer
{
    class Message
    {
        public string from;
        public string address;
        public byte[] data;
    }

    class Client
    {
        private Socket socket;
        private string name;
        private bool alive;
        private Mutex mutex;
        private Random rnd = new Random();

        public BigInteger A, V, N;

        private string status;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Status
        {
            get
            {
                return status;
            }
        }

        public bool IsAlive
        {
            get
            {
                return alive;
            }
        }

        public Client(TcpClient client, string Name_, bool registered)
        {
            mutex = new Mutex();
            A = new BigInteger();
            V = new BigInteger();
            N = new BigInteger();
            mutex.WaitOne();

            socket = client.Client;
            Console.WriteLine("New client {0} from {1} connected at port {2}", 
                Name_,
                IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString()),
                ((IPEndPoint)socket.RemoteEndPoint).Port
                );
            name = Name_;
            alive = true;
            byte[] data = { 200 };
            socket.Send(data);
            if (registered)
                status = "authorization";
            else
                status = "registration";

            mutex.ReleaseMutex();
        }

        public bool RecvFFSKey()
        {
            try
            {
                mutex.WaitOne();

                int check = socket.Available;
                if (check == 0)
                {
                    mutex.ReleaseMutex();
                    return false;
                }

                
                byte[] data = new byte[1];
                socket.Receive(data, 1, SocketFlags.None);
                if (data[0] != 200)
                {
                    mutex.ReleaseMutex();
                    Console.WriteLine("Client {0} error: sended incorrect announce!", name);
                    alive = false;
                    return false;
                }

                byte[] row_size = new byte[4];
                socket.Receive(row_size, 4, SocketFlags.None);
                int size = BitConverter.ToInt32(row_size);

                byte[] row_V = new byte[size];
                socket.Receive(row_V, size, SocketFlags.None);
                V = new BigInteger(row_V, true);
                Console.WriteLine("Client {0} received V = {1}", name, V.ToString());

                row_size = new byte[4];
                socket.Receive(row_size, 4, SocketFlags.None);
                size = BitConverter.ToInt32(row_size);

                byte[] row_N = new byte[size];
                socket.Receive(row_N, size, SocketFlags.None);
                N = new BigInteger(row_N, true);
                Console.WriteLine("Client {0} received N = {1}", name, N.ToString());

                data[0] = 200;
                socket.Send(data);

                status = "authorization";

                mutex.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine("Client {0} error: {1}", name, exp.Message);
                alive = false;
                mutex.ReleaseMutex();
                return false;
            }
        }

        public bool Authorization()
        {
            try
            {
                mutex.WaitOne();

                StreamReader db = new StreamReader(File.OpenRead("D:/keys/database"));
                string Vtxt = "NaN";
                string Ntxt = "NaN";

                while (!db.EndOfStream)
                {
                    string text = db.ReadLine();
                    if (text == name)
                    {
                        Vtxt = db.ReadLine();
                        Ntxt = db.ReadLine();
                        break;
                    }
                }
                db.Close();

                byte[] answ = { 200 };
                if (Vtxt == "NaN" || Ntxt == "NaN")
                {
                    answ[0] = 254;
                    socket.Send(answ);
                    Console.WriteLine("Client {0} error: public key not found!", name);
                    alive = false;
                    mutex.ReleaseMutex();
                    return false;
                }

                socket.Send(answ);
                Console.WriteLine("Client {0} founded public key!", name);

                V = BigInteger.Parse(Vtxt);
                N = BigInteger.Parse(Ntxt);

                for(int i=0; i<16; i++)
                {
                    Console.WriteLine("Client {0} Round of authorization {1}", name, i+1);
                    if (FFSAuthRound())
                    {
                        Console.WriteLine("Client {0} Round of authorization {1} - Success!", name, i + 1);
                    }
                    else
                    {
                        Console.WriteLine("Client {0} Round of authorization {1} - Failed!", name, i + 1);
                        alive = false;
                        mutex.ReleaseMutex();
                        return false;
                    }
                }

                status = "DH exchange";

                mutex.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine("Client {0} error: {1}", name, exp.Message);
                alive = false;
                mutex.ReleaseMutex();
                return false;
            }
        }

        private bool FFSAuthRound()
        {
            byte[] data = { 200 };
            socket.Send(data);

            byte[] row_size = new byte[4];
            socket.Receive(row_size, 4, SocketFlags.None);
            int size = BitConverter.ToInt32(row_size);

            byte[] row_z = new byte[size];
            socket.Receive(row_z, size, SocketFlags.None);
            BigInteger z = new BigInteger(row_z, true);

            Console.WriteLine("Client {0} received z = {1}", name, z.ToString());

            int rand = rnd.Next();
            if (rand % 2 == 0)
            {
                data[0] = 0;
                socket.Send(data);

                Console.WriteLine("Client {0} bit = {1}", name, data[0]);

                row_size = new byte[4];
                socket.Receive(row_size, 4, SocketFlags.None);
                size = BitConverter.ToInt32(row_size);

                byte[] row_r = new byte[size];
                socket.Receive(row_r, size, SocketFlags.None);
                BigInteger r = new BigInteger(row_r, true);

                Console.WriteLine("Client {0} received r = {1}", name, r.ToString());

                if (BigInteger.ModPow(r, 2, N) == z)
                {
                    data[0] = 200;
                    socket.Send(data);
                    return true;
                }
                else
                {
                    data[0] = 255;
                    socket.Send(data);
                    return false;
                }
            }
            else
            {
                data[0] = 1;
                socket.Send(data);

                Console.WriteLine("Client {0} bit = {1}", name, data[0]);

                row_size = new byte[4];
                socket.Receive(row_size, 4, SocketFlags.None);
                size = BitConverter.ToInt32(row_size);

                byte[] row_y = new byte[size];
                socket.Receive(row_y, size, SocketFlags.None);
                BigInteger y = new BigInteger(row_y, true);

                Console.WriteLine("Client {0} received y = {1}", name, y.ToString());

                if ((y * y * V) % N == z)
                {
                    data[0] = 200;
                    socket.Send(data);
                    return true;
                }
                else
                {
                    data[0] = 255;
                    socket.Send(data);
                    return false;
                }
            }
        }

        public bool SendDH(BigInteger p, BigInteger g)
        {
            try
            {
                mutex.WaitOne();

                byte[] data = { 101 };
                socket.Send(data);

                short P_size = (short)p.ToByteArray().Length;
                data = BitConverter.GetBytes(P_size);
                socket.Send(data);

                data = p.ToByteArray();
                socket.Send(data);

                byte G_size = (byte)g.ToByteArray().Length;
                data[0] = G_size;
                socket.Send(data, 1, SocketFlags.None);

                data = g.ToByteArray();
                socket.Send(data);

                data = new byte[2];
                socket.Receive(data, 2, SocketFlags.None);
                short A_size = BitConverter.ToInt16(data);

                data = new byte[A_size];
                socket.Receive(data, A_size, SocketFlags.None);

                A = new BigInteger(data);
                Console.WriteLine("Client {0} public key = {1}", name, A.ToString());

                status = "ready";
                mutex.ReleaseMutex();

                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine("Client {0} error: {1}", name, exp.Message);
                alive = false;
                mutex.ReleaseMutex();

                return false;
            }
        }

        public void SendPK(string name, BigInteger pk)
        {
            try
            {
                mutex.WaitOne();

                byte[] data = { 102 };
                socket.Send(data);

                short size = (short)(name.Length * 2);
                data = BitConverter.GetBytes(size);
                socket.Send(data);

                data = Encoding.Unicode.GetBytes(name);
                socket.Send(data);

                size = (short)pk.ToByteArray().Length;
                data = BitConverter.GetBytes(size);
                socket.Send(data);

                data = pk.ToByteArray();
                socket.Send(data);

                mutex.ReleaseMutex();
            }
            catch (Exception exp)
            {
                Console.WriteLine("Client {0} error: {1}", name, exp.Message);
                alive = false;
                mutex.ReleaseMutex();
            }
        }

        public void UpdateTimestamp(long Timestamp)
        {
            try
            {
                mutex.WaitOne();

                byte[] data = { 100 };
                socket.Send(data);
                data = BitConverter.GetBytes(Timestamp);
                socket.Send(data);

                mutex.ReleaseMutex();
            }
            catch (Exception exp)
            {
                Console.WriteLine("Client {0} error: {1}", name, exp.Message);
                alive = false;
                mutex.ReleaseMutex();
            }
        }

        public Message RecvData()
        {
            try
            {
                mutex.WaitOne();

                int check = socket.Available;
                if (check == 0)
                {
                    mutex.ReleaseMutex();
                    return null;
                }

                byte[] data = new byte[1];
                socket.Receive(data, 1, SocketFlags.None);
                if (data[0] != 110)
                {
                    mutex.ReleaseMutex();
                    return null;
                }

                byte[] row_size = new byte[4];
                socket.Receive(row_size, 4, SocketFlags.None);
                int size = BitConverter.ToInt32(row_size);

                byte[] row_address = new byte[size];
                socket.Receive(row_address, size, SocketFlags.None);
                string addr = Encoding.Unicode.GetString(row_address);

                row_size = new byte[4];
                socket.Receive(row_size, 4, SocketFlags.None);
                size = BitConverter.ToInt32(row_size);

                data = new byte[size];
                socket.Receive(data, size, SocketFlags.None);

                Message res = new Message();
                res.from = name;
                res.address = addr;
                res.data = data;

                mutex.ReleaseMutex();

                return res;
            }
            catch (Exception exp)
            {
                Console.WriteLine("Client {0} error: {1}", name, exp.Message);
                alive = false;
                mutex.ReleaseMutex();

                return null;
            }
        }

        public void SendData(Message msg)
        {
            try
            {
                mutex.WaitOne();

                byte[] data = { 120 };
                socket.Send(data);

                byte[] row_size = BitConverter.GetBytes(msg.from.Length * 2);
                socket.Send(row_size);
                byte[] row_from = Encoding.Unicode.GetBytes(msg.from);
                socket.Send(row_from);

                row_size = BitConverter.GetBytes(msg.data.Length);
                socket.Send(row_size);
                socket.Send(msg.data);

                mutex.ReleaseMutex();
            }
            catch (Exception exp)
            {
                Console.WriteLine("Client {0} error: {1}", name, exp.Message);
                alive = false;
                mutex.ReleaseMutex();

                return;
            }
        }

        public void SendNoExist(string name)
        {
            try
            {
                mutex.WaitOne();

                byte[] data = { 220 };
                socket.Send(data);

                byte[] row_size = BitConverter.GetBytes(name.Length * 2);
                socket.Send(row_size);
                byte[] row_name = Encoding.Unicode.GetBytes(name);
                socket.Send(row_name);

                mutex.ReleaseMutex();
            }
            catch (Exception exp)
            {
                Console.WriteLine("Client {0} error: {1}", name, exp.Message);
                alive = false;
                mutex.ReleaseMutex();

                return;
            }
        }
    }
}
