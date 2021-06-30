using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace client
{
    public class Listener
    {

        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);


        public static void Listen()
        {
            IPEndPoint localEndPoint = new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 1151);
            Socket client = new Socket(localEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            client.BeginConnect(localEndPoint, new AsyncCallback(ConnectCB), client);
            connectDone.WaitOne();
            while (true)
            {
                Receive(client);
            }
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
        private static void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCB), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCB(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCB), state);
                }
                else
                {
                    if (state.sb.Length > 1)
                    {
                        string response = state.sb.ToString();
                        Console.WriteLine(response);
                    }
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
