using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Numerics;
using System.IO;
using System.Reflection.Metadata;

namespace KeyManagmentServer
{
    class Server
    {
        private TcpListener Listener;
        private ushort port;
        private long timestamp;

        private Mutex mutex = new Mutex();

        public List<Client> unconfirmed;
        public List<Client> clients;
        public BigInteger P, G;

        public Server(int Port)
        {
            port = (ushort)Port;
            Listener = new TcpListener(IPAddress.Any, port);
            clients = new List<Client>();
            unconfirmed = new List<Client>();
            DateTime currentTime = DateTime.Now;
            timestamp = currentTime.Second;
            //Listener.Start();
        }

        public void StartServer()
        {
            Listener.Start();
            Console.WriteLine("Server started at port {0}", port);
        }

        private bool RegisterNewUser(TcpClient newClient, string login)
        {
            StreamReader db = new StreamReader(File.OpenRead("D:/keys/database"));

            while (!db.EndOfStream)
            {
                string text = db.ReadLine();
                if (text == login)
                {
                    byte[] data = { 254 };
                    newClient.Client.Send(data);
                    newClient.Close();
                    return false;
                }
            }

            mutex.WaitOne();
            Client client = new Client(newClient, login, false);
            unconfirmed.Add(client);
            mutex.ReleaseMutex();

            db.Close();

            return true;
        }

        private void StartAuthUser(TcpClient newClient, string login)
        {
            mutex.WaitOne();
            Client client = new Client(newClient, login, true);
            unconfirmed.Add(client);
            mutex.ReleaseMutex();
        }

        public void ClientsHandler()
        {
            while (true)
            {
                TcpClient newClient = Listener.AcceptTcpClient();
                byte[] actionType = new byte[1];
                newClient.Client.Receive(actionType);
                byte[] loginSize = new byte[4];
                newClient.Client.Receive(loginSize);
                int log_size = BitConverter.ToInt32(loginSize);
                byte[] loginRow = new byte[log_size];
                newClient.Client.Receive(loginRow);
                string login = Encoding.Unicode.GetString(loginRow);

                if (clients.Exists(x => x.Name == login))
                {
                    loginRow = new byte[1];
                    loginRow[0] = 255;
                    newClient.Client.Send(loginRow);
                    newClient.Close();
                }
                else if (unconfirmed.Exists(x => x.Name == login))
                {
                    loginRow = new byte[1];
                    loginRow[0] = 255;
                    newClient.Client.Send(loginRow);
                    newClient.Close();
                }
                else
                {
                    if (actionType[0] == 100)
                    {
                        if (!RegisterNewUser(newClient, login))
                        {
                            return;
                        }
                    }
                    if (actionType[0] == 200)
                    {
                        StartAuthUser(newClient, login);
                    }
                    /*
                    Client client = new Client(newClient, login);
                    client.UpdateTimestamp(timestamp);
                    client.SendDH(P, G);
                    for(int i=0; i<clients.Count; i++)
                    {
                        client.SendPK(clients[i].Name, clients[i].A);
                        clients[i].SendPK(client.Name, client.A);
                    }
                    clients.Add(client);
                    */
                }
            }
        }

        public void CryptographicalExchange()
        {
            while(true)
            {
                foreach (Client client in unconfirmed)
                {
                    if (client.Status == "registration")
                    {
                        if(client.RecvFFSKey())
                        {
                            StreamWriter db = new StreamWriter(File.Open("D:/keys/database", FileMode.Append));
                            db.WriteLine(client.Name);
                            db.WriteLine(client.V.ToString());
                            db.WriteLine(client.N.ToString());
                            db.Flush();
                            db.Close();

                            Console.WriteLine("Key of client {0} saved in DB");
                        }
                        continue;
                    }
                    if (client.Status == "authorization")
                    {
                        client.Authorization();
                        continue;
                    }
                    if (client.Status == "DH exchange")
                    {
                        if (client.SendDH(P, G))
                        {
                            for (int i = 0; i < clients.Count; i++)
                            {
                                client.SendPK(clients[i].Name, clients[i].A);
                                clients[i].SendPK(client.Name, client.A);
                            }
                            client.UpdateTimestamp(timestamp);
                            clients.Add(client);
                        }
                        continue;
                    }
                }

                mutex.WaitOne();


                unconfirmed.RemoveAll(x => !x.IsAlive);
                unconfirmed.RemoveAll(x => x.Status == "ready");

                mutex.ReleaseMutex();

                Thread.Sleep(100);
            }
        }

        public void ServerTimer()
        {
            while (true)
            {
                DateTime currentTime = DateTime.Now;
                timestamp = currentTime.Ticks;
                Console.WriteLine("New server timestamp = {0}", timestamp);

                mutex.WaitOne();
                foreach (Client client in clients)
                {
                    client.UpdateTimestamp(timestamp);
                }

                clients.RemoveAll(x => !x.IsAlive);
                mutex.ReleaseMutex();

                Thread.Sleep(60000);
            }
        }

        public void Messenger()
        {
            while (true)
            {
                foreach (Client client in clients)
                {
                    Message msg = client.RecvData();
                    if (msg != null)
                    {
                        Console.WriteLine("Message from {0} to {1}", msg.from, msg.address);

                        int id_address = -1;
                        id_address = clients.FindIndex(x => x.Name == msg.address);
                        if (id_address == -1)
                        {
                            client.SendNoExist(msg.address);
                            continue;
                        }

                        clients[id_address].SendData(msg);
                    }
                }

                mutex.WaitOne();
                clients.RemoveAll(x => !x.IsAlive);
                mutex.ReleaseMutex();

                Thread.Sleep(100);
            }
        }

        ~Server()
        {
            if (Listener != null)
            {
                Listener.Stop();
            }
        }
    }
}
