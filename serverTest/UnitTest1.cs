using Microsoft.VisualStudio.TestTools.UnitTesting;
using server;
namespace serverTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestListen()
        {
            try
            {
                server.Listener.Listen();
                return;
            }
            catch (System.Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
