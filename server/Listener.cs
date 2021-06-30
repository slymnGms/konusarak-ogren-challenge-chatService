using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace server
{
    public class Listener
    {
        public static Hashtable clientsList = new Hashtable();
        public static Hashtable clientLastTime { get; set; }
        public static ManualResetEvent mre = new ManualResetEvent(false);

        public static async void Listen()
        {
            IPAddress ia = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            IPEndPoint iep = new IPEndPoint(ia, 1163);
            Socket sc = new Socket(ia.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sc.Bind(iep);
                sc.Listen(100);
                while (true)
                {
                    mre.Reset();
                    Console.WriteLine("Chat Server Started ....");
                    sc.BeginAccept(
                        new AsyncCallback(AcceptCB), sc);
                    mre.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error Occured" + Environment.NewLine + ex.ToString());
                Console.ResetColor();
            }
        }

        private static void AcceptCB(IAsyncResult ar)
        {
            mre.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject so = new StateObject
            {
                workSocket = handler
            };
            handler.BeginReceive(so.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCB), so);
        }

        private static void ReadCB(IAsyncResult ar)
        {
            String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                content = state.sb.ToString();
                if (content.IndexOf("$") > -1)
                {
                    bool newArrived = false;
                    Socket cl = handler;
                    if (clientsList.ContainsKey(cl.RemoteEndPoint))
                    {
                        bool canSend = checkLastMessage(cl);
                        if (canSend) Console.WriteLine("{0} : {1}", (string)clientsList[cl.RemoteEndPoint], content);
                        else warnUser(cl);
                    }
                    else
                    {
                        newArrived = true;
                        Console.WriteLine("{0} has connected", content.Substring(0, content.Length - 1));
                        clientsList.Add(cl.RemoteEndPoint, content);
                    }
                    if (newArrived) content = content.Substring(0, content.Length - 1) + " has connected";
                    else content = (string)clientsList[cl.RemoteEndPoint] + " : " + content.Substring(0, content.Length - 1);
                    BroadCast(content);
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCB), state);
                }
            }
        }

        private static void BroadCast(string content)
        {
            foreach (DictionaryEntry item in clientsList)
            {
                EndPoint ep = (EndPoint)item.Key;
                Socket sc = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sc.Connect(ep.ToString(), 1151);
                Send(sc, content);
            }
        }

        private static void warnUser(Socket cl)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("User {0} has reach out", (string)clientsList[cl]);
            Console.ResetColor();
            Socket sc = new Socket(cl.RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sc.Connect(cl.RemoteEndPoint.ToString(), 1151);
            Send(sc, "You have reach out time limit");
        }

        private static bool checkLastMessage(Socket cl)
        {
            if (clientLastTime.ContainsKey(cl))
            {
                DateTime dt = (DateTime)clientLastTime[cl];
                if (DateTime.Now.Subtract(dt).TotalMinutes < 2) return false;
                clientLastTime[cl] = DateTime.Now;
                return true;
            }
            else
            {
                clientLastTime.Add(cl, DateTime.Now);
                return true;
            }
        }

        private static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCB), handler);
        }

        private static void SendCB(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
