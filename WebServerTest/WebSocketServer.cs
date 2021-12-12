using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace WebServerTest
{
    class WebSocketServer
    {
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
        internal class RequestSignIn
        {
            public string RequestName { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }
        internal class MainRequest
        {
            public string RequestName { get; set; }
            public string Message { get; set; }
        }

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
                string identifier = httpListenerContext.Request.RequestTraceIdentifier.ToString();

                Console.WriteLine("Connected: IPAddress {0}", ipAddress);
                Console.WriteLine("Connected: Identifier {0}", identifier);
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
                    byte[] receiveBuffer = new byte[1024];
                    
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    else
                    {
                        try
                        {
                            for (int i = receiveBuffer.Length - 1; i > 0; i--)
                                if (receiveBuffer[i] == 0)
                                    Array.Resize(ref receiveBuffer, receiveBuffer.Length - 1);

                            MainRequest mainRequest = JsonSerializer.Deserialize<MainRequest>(Encoding.UTF8.GetString(receiveBuffer));
                            string request = mainRequest.RequestName;

                            switch (request)
                            {
                                case "update":
                                    await webSocket.SendAsync(new ArraySegment<byte>(Update(httpListenerContext.Request.RequestTraceIdentifier.ToString())), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    break;
                                case "signup":
                                    if (SignUp(receiveBuffer))
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("signup", "Registration was successful")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    else
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("signup", "Registration failed")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    break;
                                case "signin":
                                    if (SignIn(receiveBuffer, httpListenerContext.Request.RequestTraceIdentifier.ToString()))
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("signin", "Login completed Successfully")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    else
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("signin", "Login failed")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    break;
                                case "changeprofile":
                                    if(ChangeProfile(receiveBuffer, httpListenerContext.Request.RequestTraceIdentifier.ToString()))
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("changeprofile", "Profile changed Successfully")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    else
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("changeprofile", "Profile changed failed")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    break;
                                case "deleteuser":
                                    if(DeleteProfile(httpListenerContext.Request.RequestTraceIdentifier.ToString()))
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("deleteuser", "Profile is deleted")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    else
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("deleteuser", "Profile is not deleted")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    break;
                                case "logout":
                                    if (Logout(httpListenerContext.Request.RequestTraceIdentifier.ToString()))
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("logout", "User is logout")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    else
                                        await webSocket.SendAsync(new ArraySegment<byte>(MainRequestByte("logout", "User is not logout")), WebSocketMessageType.Binary, false, CancellationToken.None);
                                    break;
                                default:
                                    await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);
                                    break;
                            }
                        }
                        catch(JsonException ex)
                        {
                            Console.WriteLine("Unknown command!");
                            continue;
                        }
                    }
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

        private byte[] MainRequestByte(string _request, string _message)
        {
            try
            {
                MainRequest mainRequest = new MainRequest();
                mainRequest.RequestName = _request;
                mainRequest.Message = _message;

                string json = JsonSerializer.Serialize<MainRequest>(mainRequest);
                return Encoding.UTF8.GetBytes(json);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
        }
        private bool SignIn(byte[] _buffer, string _identifier)
        {
            try
            {
                bool ret = false;
                RequestSignIn requestSignIn = JsonSerializer.Deserialize<RequestSignIn>(Encoding.UTF8.GetString(_buffer));

                using (WebServerDataBase serverData = new WebServerDataBase())
                {
                    foreach (User user in serverData.Users)
                    {
                        if (user.login == requestSignIn.Login && user.password == requestSignIn.Password)
                        {
                            ret = true;
                            user.Identifier = _identifier;
                            serverData.Users.Update(user);
                            serverData.SaveChanges();

                            Console.WriteLine("User: {0}, Identifier: {1}", requestSignIn.Login, _identifier);
                            break;
                        }
                        else
                            ret = false;
                    }
                }
                return ret;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return false;
            }

        }
        private bool SignUp(byte[] _buffer)
        {
            try
            {
                RequestSignUp requestSignUp = JsonSerializer.Deserialize<RequestSignUp>(Encoding.UTF8.GetString(_buffer));
                using (WebServerDataBase serverData = new WebServerDataBase())
                {
                    User user = new User
                    {
                        login = requestSignUp.Login,
                        password = requestSignUp.Password
                    };
                    serverData.Users.Add(user);

                    UserProfile userProfile = new UserProfile
                    {
                        Name = requestSignUp.Name,
                        Lastname = requestSignUp.Lastname,
                        Age = requestSignUp.Age,
                        City = requestSignUp.City,
                        Language = requestSignUp.Language,
                        Bio = requestSignUp.Bio,
                        Birthday = requestSignUp.Birthday,
                        User = user
                    };
                    serverData.UserProfiles.Add(userProfile);
                    serverData.SaveChanges();

                    Console.WriteLine("Registration was successful: " + user.login);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return false;
            }
        }
        private byte[] Update(string _identifier)
        {
            try
            {
                byte[] buffer = null;
                using (WebServerDataBase serverData = new WebServerDataBase())
                {
                    foreach (User user in serverData.Users)
                    {
                        if (user.Identifier == _identifier)
                        {
                            UserProfile userProfile = serverData.UserProfiles.Find(user.Id);
                            RequestUpdate requestUpdate = new RequestUpdate()
                            {
                                RequestName = "update",
                                Id = userProfile.Id,
                                Name = userProfile.Name,
                                Lastname = userProfile.Lastname,
                                Age = userProfile.Age,
                                City = userProfile.City,
                                Language = userProfile.Language,
                                Bio = userProfile.Bio,
                                Birthday = userProfile.Birthday
                            };
                            string json = JsonSerializer.Serialize<RequestUpdate>(requestUpdate);
                            buffer = Encoding.UTF8.GetBytes(json);

                            Console.WriteLine("User Profile: " + json);
                            break;
                        }
                    }
                }
               return buffer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
        }
        private bool ChangeProfile(byte[] _buffer, string _identifier)
        {
            try
            {
                bool ret = false;
                RequestChangeProfile requestChangeProfile = JsonSerializer.Deserialize<RequestChangeProfile>(Encoding.UTF8.GetString(_buffer));

                using (WebServerDataBase serverData = new WebServerDataBase())
                {
                    foreach (User user in serverData.Users)
                    {
                        if (user.Identifier == _identifier)
                        {
                            ret = true;
                            UserProfile userProfile = serverData.UserProfiles.FirstOrDefault(p => p.User.Identifier == _identifier);

                            if(userProfile != null)
                            {
                                userProfile.Name = requestChangeProfile.Name;
                                userProfile.Lastname = requestChangeProfile.Lastname;
                                userProfile.Age = requestChangeProfile.Age;
                                userProfile.Language = requestChangeProfile.Language;
                                userProfile.City = requestChangeProfile.City;
                                userProfile.Bio = requestChangeProfile.Bio;
                                userProfile.Birthday = requestChangeProfile.Birthday;
                                userProfile.User = user;

                                serverData.SaveChanges();
                            }

                            Console.WriteLine("User {0} profile update", user.login);
                            break;
                        }
                        else
                            ret = false;
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return false;
            }

        }
        private bool DeleteProfile(string _identifier)
        {
            try
            {
                bool ret = false;
                using (WebServerDataBase serverData = new WebServerDataBase())
                {
                    foreach (User user in serverData.Users)
                    {
                        if (user.Identifier == _identifier)
                        {
                            serverData.Remove(user);
                            serverData.SaveChanges();
                            ret = true;
                            Console.WriteLine("User {0} is deleted", user.login);
                            break;
                        }
                        else
                            ret = false;
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return false;
            }

        }
        private bool Logout(string _identifier)
        {
            try
            {
                bool ret = false;
                using (WebServerDataBase serverData = new WebServerDataBase())
                {
                    User user = serverData.Users.FirstOrDefault(p => p.Identifier == _identifier);
                    user.Identifier = null;
                    serverData.SaveChanges();
                    ret = true;
                    Console.WriteLine("User {0} is logout", user.login);
                }
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return false;
            }

        }
    }
}
