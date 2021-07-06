using System;
using System.Threading;


namespace KeyManagmentServer
{
    class Program
    {
        private static Server server;
        private static Thread clientsHandler;
        private static Thread timestampUpdater;
        private static Thread Messenger;
        private static Thread cryptoMeneger;
        private static NumberGenerator DH;

        static void Main(string[] args)
        {
            server = new Server(10200);

            DH = new NumberGenerator();
            server.G = DH.G;
            server.P = DH.P;
            server.StartServer();

            clientsHandler = new Thread(server.ClientsHandler);
            clientsHandler.Start();

            cryptoMeneger = new Thread(server.CryptographicalExchange);
            cryptoMeneger.Start();

            timestampUpdater = new Thread(server.ServerTimer);
            timestampUpdater.Start();

            Messenger = new Thread(server.Messenger);
            Messenger.Start();
        }
    }
}
