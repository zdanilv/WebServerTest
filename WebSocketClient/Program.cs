using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

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
            Thread.Sleep(1000);
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
                byte[] buffer;

                Console.WriteLine("Write some to send over to server...");
                string stringSend = Console.ReadLine();

                switch (stringSend)
                {
                    case "update":
                        buffer = Update();
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                        break;
                    case "signup":
                        buffer = SignUp();
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                        break;
                    case "signin":
                        buffer = SignIn();
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                        break;
                    case "changeprofile":
                        buffer = ChangeProfile();
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                        break;
                    case "deleteuser":
                        buffer = DeleteUser();
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                        break;
                    case "logout":
                        buffer = Logout();
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                        break;
                    default:
                        buffer = encoding.GetBytes(stringSend);
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);
                        Console.WriteLine("Send:" + stringSend);
                        break;
                }

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

                    try
                    {
                        MainRequest mainRequest = JsonSerializer.Deserialize<MainRequest>(Encoding.UTF8.GetString(buffer));
                        string request = mainRequest.RequestName;

                        switch (request)
                        {
                            case "update":
                                RequestUpdate requestUpdate = JsonSerializer.Deserialize<RequestUpdate>(Encoding.UTF8.GetString(buffer));
                                Console.WriteLine("Deserialize: \nName :{0}\nLasname: {1}\nAge: {2}\nCity: {3}\nLanguage: {4}\nBio: {5}\nBirthday: {6}", 
                                    requestUpdate.Name, requestUpdate.Lastname, requestUpdate.Age, requestUpdate.City,
                                    requestUpdate.Language, requestUpdate.Bio, requestUpdate.Birthday);
                                break;
                            case "signup":
                                MainRequest requestSignUp = JsonSerializer.Deserialize<MainRequest>(Encoding.UTF8.GetString(buffer));
                                Console.WriteLine("Deserialize: " + requestSignUp.Message);
                                break;
                            case "signin":
                                MainRequest requestSignIn = JsonSerializer.Deserialize<MainRequest>(Encoding.UTF8.GetString(buffer));
                                Console.WriteLine("Deserialize: " + requestSignIn.Message);
                                break;
                            case "changeprofile":
                                MainRequest requestChangeProfile = JsonSerializer.Deserialize<MainRequest>(Encoding.UTF8.GetString(buffer));
                                Console.WriteLine("Deserialize: " + requestChangeProfile.Message);
                                break;
                            case "deleteuser":
                                MainRequest requestDeleteUser = JsonSerializer.Deserialize<MainRequest>(Encoding.UTF8.GetString(buffer));
                                Console.WriteLine("Deserialize: " + requestDeleteUser.Message);
                                break;
                            case "logout":
                                MainRequest requestLogout = JsonSerializer.Deserialize<MainRequest>(Encoding.UTF8.GetString(buffer));
                                Console.WriteLine("Deserialize: " + requestLogout.Message);
                                break;
                            default:
                                Console.WriteLine("Receive: " + Encoding.UTF8.GetString(buffer));
                                break;
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine("Exception: " + ex);
                        continue;
                    }
                }
            }
        }
        private static byte[] Update()
        {
            try
            {
                RequestUpdate requestUpdate = new RequestUpdate();
                requestUpdate.RequestName = "update";
                string json = JsonSerializer.Serialize<RequestUpdate>(requestUpdate);
                return encoding.GetBytes(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
        }
        private static byte[] SignUp()
        {
            try
            {
                RequestSignUp requestSignUp = new RequestSignUp();
                requestSignUp.RequestName = "signup";

                Console.WriteLine("Enter login: ");
                requestSignUp.Login = Console.ReadLine();
                Console.WriteLine("Enter password: ");
                requestSignUp.Password = Console.ReadLine();
                Console.WriteLine("Enter name: ");
                requestSignUp.Name = Console.ReadLine();
                Console.WriteLine("Enter last name: ");
                requestSignUp.Lastname = Console.ReadLine();
                Console.WriteLine("Enter age: ");
                requestSignUp.Age = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Enter city: ");
                requestSignUp.City = Console.ReadLine();
                Console.WriteLine("Enter laguage: ");
                requestSignUp.Language = Console.ReadLine();
                Console.WriteLine("Enter bio: ");
                requestSignUp.Bio = Console.ReadLine();
                Console.WriteLine("Enter birthday: ");
                requestSignUp.Birthday = DateTime.Parse(Console.ReadLine());

                string json = JsonSerializer.Serialize<RequestSignUp>(requestSignUp);
                return encoding.GetBytes(json);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
        }

        private static byte[] SignIn()
        {
            try
            {
                RequestSignIn requestSignIn = new RequestSignIn();
                requestSignIn.RequestName = "signin";

                Console.WriteLine("Enter login: ");
                requestSignIn.Login = Console.ReadLine();

                Console.WriteLine("Enter password: ");
                requestSignIn.Password = Console.ReadLine();

                string json = JsonSerializer.Serialize<RequestSignIn>(requestSignIn);
                return encoding.GetBytes(json);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
        }
        private static byte[] ChangeProfile()
        {
            try
            {
                RequestChangeProfile requestChangeProfile = new RequestChangeProfile();
                requestChangeProfile.RequestName = "changeprofile";

                Console.WriteLine("Enter name: ");
                requestChangeProfile.Name = Console.ReadLine();
                Console.WriteLine("Enter last name: ");
                requestChangeProfile.Lastname = Console.ReadLine();
                Console.WriteLine("Enter age: ");
                requestChangeProfile.Age = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Enter city: ");
                requestChangeProfile.City = Console.ReadLine();
                Console.WriteLine("Enter laguage: ");
                requestChangeProfile.Language = Console.ReadLine();
                Console.WriteLine("Enter bio: ");
                requestChangeProfile.Bio = Console.ReadLine();
                Console.WriteLine("Enter birthday: ");
                requestChangeProfile.Birthday = DateTime.Parse(Console.ReadLine());

                string json = JsonSerializer.Serialize<RequestChangeProfile>(requestChangeProfile);
                return encoding.GetBytes(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
        }
        private static byte[] DeleteUser()
        {
            try
            {
                MainRequest requestDeleteUser = new MainRequest();
                requestDeleteUser.RequestName = "deleteuser";

                string json = JsonSerializer.Serialize<MainRequest>(requestDeleteUser);
                return encoding.GetBytes(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
        }
        private static byte[] Logout()
        {
            try
            {
                MainRequest requestLogout = new MainRequest();
                requestLogout.RequestName = "logout";

                string json = JsonSerializer.Serialize<MainRequest>(requestLogout);
                return encoding.GetBytes(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
        }
    }
    internal class MainRequest
    {
        public string RequestName { get; set; }
        public string Message { get; set; }
    }
    internal class RequestSignIn
    {
        public string RequestName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
    internal class RequestSignUp
    {
        public string RequestName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
        public string Language { get; set; }
        public string Bio { get; set; }
        public DateTime Birthday { get; set; }
    }
    internal class RequestUpdate
    {
        public string RequestName { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
        public string Language { get; set; }
        public string Bio { get; set; }
        public DateTime Birthday { get; set; }
    }
    internal class RequestChangeProfile
    {
        public string RequestName { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
        public string Language { get; set; }
        public string Bio { get; set; }
        public DateTime Birthday { get; set; }
    }
}
