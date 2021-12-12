using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace WebServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            WebSocketServer webSocketServer = new WebSocketServer();
            webSocketServer.Start("http://localhost:80/Web/");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

    }
}
