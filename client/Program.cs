using System.Threading;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread mThread = new Thread(Messager.StartClient);
            Thread lThread = new Thread(Listener.Listen);

            mThread.Start();
            lThread.Start();
        }
    }
}
