using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SocketLite.Services;

namespace SocketLite.UWP.Tests
{
    [TestClass]
    public class TcpListenerTests
    {
        readonly SocketLite.Services.TcpSocketListener _tcpListener = new TcpSocketListener();

        [TestMethod]
        public async Task TestMethod1()
        {
            //Arrange
            _tcpListener.ConnectionReceived += async (sender, args) =>
            {
                var client = args.SocketClient;

                while (true)
                {
                    // read from the 'ReadStream' property of the socket client to receive data
                    var nextByte = await Task.Run(() => client.ReadStream.ReadByte());
                    Debug.Write(nextByte);
                }
            };

            //Act
            await _tcpListener.StartListeningAsync(8000);

            await Task.Delay(TimeSpan.FromMinutes(1));
            //Assert

            Assert.AreEqual(true, true);
        }
    }
}
