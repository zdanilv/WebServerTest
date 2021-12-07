using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebServerTest
{
    class WebSocketServer
    {
        public async void Start(string httpListenerPrefix)
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(httpListenerPrefix);
            httpListener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext httpListenerContext = await httpListener.GetContextAsync();
                if (httpListenerContext.Request.IsWebSocketRequest)
                {
                    ProcessRequest(httpListenerContext);
                }
                else
                {
                    httpListenerContext.Response.StatusCode = 400;
                    httpListenerContext.Response.Close();
                    Console.WriteLine("CODE 400 - CLOSE");
                }
            }
        }

        private async void ProcessRequest(HttpListenerContext httpListenerContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await httpListenerContext.AcceptWebSocketAsync(subProtocol: null);
                string ipAddress = httpListenerContext.Request.RemoteEndPoint.Address.ToString();
                Console.WriteLine("Connected: IPAddress {0}", ipAddress);
            }
            catch(Exception ex)
            {
                httpListenerContext.Response.StatusCode = 400;
                httpListenerContext.Response.Close();
                Console.WriteLine("Exception: " + ex);
                return;
            }

            WebSocket webSocket = webSocketContext.WebSocket;
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    string stringSend;
                    byte[] buffer;
                    byte[] receiveBuffer = new byte[1024];

                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    else
                    {
                        for (int i = receiveBuffer.Length - 1; i > 0; i--)
                            if (receiveBuffer[i] == 0)
                                Array.Resize(ref receiveBuffer, receiveBuffer.Length - 1);

                        switch (Encoding.UTF8.GetString(receiveBuffer)){
                            case "1":
                                stringSend = "CODE 1";
                                buffer = Encoding.UTF8.GetBytes(stringSend);
                                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                                break;
                            case "2":
                                stringSend = "CODE 2";
                                buffer = Encoding.UTF8.GetBytes(stringSend);
                                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                                break;
                            case "3":
                                stringSend = "CODE 3";
                                buffer = Encoding.UTF8.GetBytes(stringSend);
                                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                                break;
                            case "4":
                                stringSend = "CODE 4";
                                buffer = Encoding.UTF8.GetBytes(stringSend);
                                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                                break;
                            default:
                                await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);
                                break;
                        }

                        /*if (Encoding.UTF8.GetString(receiveBuffer) == "1")
                        {
                            string stringSend = "CODE 1";
                            byte[] buffer = Encoding.UTF8.GetBytes(stringSend);

                            Console.WriteLine(Encoding.UTF8.GetString(buffer));
                            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                            //Array.Clear(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);
                        }
                        */
                    }
                    //Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
            }
            finally
            {
                if (webSocket != null)
                    webSocket.Dispose();
            }
        }
    }
}
