using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace client
{
    public class Messager
    {
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);


        public static void StartClient()
        {
            try
            {
                Console.WriteLine("Type username");
                string userName = Console.ReadLine();
                Transmitter(userName);
                Console.WriteLine("Type to send message...");
                Console.WriteLine("* type '.exit' to terminate");
                while (true)
                {
                    Console.WriteLine(">");
                    string message = Console.ReadLine();
                    if (message == ".exit")
                    {
                        break;
                    }
                    Transmitter(message);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Transmitter(string message)
        {
            IPEndPoint localEndPoint = new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 1163);
            Socket client = new Socket(localEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            client.BeginConnect(localEndPoint, new AsyncCallback(ConnectCB), client);
            connectDone.WaitOne();
            Send(client, message + "$");
            sendDone.WaitOne();

            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static void ConnectCB(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



        private static void Send(Socket client, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
