using Microsoft.VisualStudio.TestTools.UnitTesting;
using client;

namespace clientTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestListen()
        {
            try
            {
                client.Listener.Listen();
                return;
            }
            catch (System.Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        [TestMethod]
        public void TestMessager()
        {
            try
            {
                client.Messager.StartClient();
                return;
            }
            catch (System.Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
