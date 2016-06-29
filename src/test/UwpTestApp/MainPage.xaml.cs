using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SocketLite.Model;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpTestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            StartTest();
            //StartTlsClient();
        }

        private async void StartTlsClient()
        {

            var tcpClient = new SocketLite.Services.TcpSocketClient();
            try
            {
                await tcpClient.ConnectAsync("abc.123.test", "8088", secure: true, ignoreServerCertificateErrors:true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void StartTest()
        {
            var comm = new CommunicationsInterface();
            var all = comm.GetAllInterfaces();
            var one = all.FirstOrDefault(x => x.GatewayAddress != null);

            var tcpListener = new SocketLite.Services.TcpSocketListener();
            var udpMulticastListener = new SocketLite.Services.UdpSocketMulticastClient();



            var udpListenerSubscribe = udpMulticastListener.ObservableMessages
                .ObserveOnDispatcher().Subscribe(
                    msg =>
                    {
                        RemoteUdpClient.Text = msg.RemoteAddress + ":" + msg.RemoteAddress;
                        Data.Text = System.Text.Encoding.UTF8.GetString(msg.ByteData);
                    });


            var tcpListenerSubscribe = StartTcpListener(tcpListener);

            await tcpListener.StartListeningAsync(8000, allowMultipleBindToSamePort: true);

            



            await udpMulticastListener.JoinMulticastGroupAsync("239.255.255.250", 1900, allowMultipleBindToSamePort: true);

            //await Task.Delay(TimeSpan.FromSeconds(1));

            //// Testing that subscription can "survive" a disconnect and connect again.
            //tcpListener.StopListening();
            //tcpListenerSubscribe.Dispose();
            //udpMulticastListener.Disconnect();

            //await tcpListener.StartListeningAsync(8000, allowMultipleBindToSamePort: true);
            //tcpListenerSubscribe = StartTcpListener(tcpListener);

            //await udpMulticastListener.JoinMulticastGroupAsync("239.255.255.250", 1900, allowMultipleBindToSamePort: true);
        }

        private IDisposable StartTcpListener(SocketLite.Services.TcpSocketListener tcpListener)
        {
            return tcpListener
                .ObservableTcpSocket
                .ObserveOnDispatcher()
                .Subscribe(
                tcpClient =>
                {
                    RemoteClient.Text = tcpClient.RemoteAddress + ":" + tcpClient.RemotePort.ToString();
                });
        }
    }
}
