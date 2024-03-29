﻿using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketClient
{
    class Program
    {
        private static UTF8Encoding encoding = new UTF8Encoding();
        static void Main(string[] args)
        {
            Connect("ws://localhost:80/Web/").Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static async Task Connect(string uri)
        {
            Thread.Sleep(5000);
            ClientWebSocket webSocket = null;
            webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
            await Task.WhenAll(Receive(webSocket), Send(webSocket));
            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                await Task.WhenAll(Receive(webSocket), Send(webSocket));
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            finally
            {
                if (webSocket != null)
                    webSocket.Dispose();
                Console.WriteLine();
                Console.WriteLine("WebSocket closed.");
            }
        }

        private static async Task Send(ClientWebSocket webSocket)
        {
            while(webSocket.State == WebSocketState.Open)
            {
                Console.WriteLine("Write some to send over to server...");
                string stringSend = Console.ReadLine();
                byte[] buffer = encoding.GetBytes(stringSend);

                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                Console.WriteLine("Send:" + stringSend);

                await Task.Delay(1000);
            }
        }
        private static async Task Receive(ClientWebSocket webSocket)
        {
            while (webSocket.State == WebSocketState.Open)
            {
                byte[] buffer = new byte[1024];

                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if(result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    for (int i = buffer.Length - 1; i > 0; i--)
                        if (buffer[i] == 0)
                            Array.Resize(ref buffer, buffer.Length - 1);
                    Console.WriteLine("Receive: " + Encoding.UTF8.GetString(buffer));
                }
            }
        }
    }
}
