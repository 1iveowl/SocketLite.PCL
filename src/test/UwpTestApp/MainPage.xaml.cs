using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
                await tcpClient.ConnectAsync("spc.1iveowl.dk", "8088", secure: true, ignoreServerCertificateErrors:true);
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

            var tcpListenerSubscribe = tcpListener
                .ObservableTcpSocket
                .ObserveOnDispatcher()
                .Subscribe(
                tcpClient =>
                {
                    RemoteClient.Text = tcpClient.RemoteAddress + ":" + tcpClient.RemotePort.ToString();
                });

            var udpListenerSubscribe = udpMulticastListener.ObservableMessages
                .ObserveOnDispatcher().Subscribe(
                    msg =>
                    {
                        RemoteUdpClient.Text = msg.RemoteAddress + ":" + msg.RemoteAddress;
                        Data.Text = System.Text.Encoding.UTF8.GetString(msg.ByteData);
                    });

            await tcpListener.StartListeningAsync(8000, allowMultipleBindToSamePort: true);
            await udpMulticastListener.JoinMulticastGroupAsync("239.255.255.250", 1900, allowMultipleBindToSamePort: true);

            // Testing that subscription can "survive" a disconnect and connect again.
            tcpListener.StopListening();
            udpMulticastListener.Disconnect();

            await tcpListener.StartListeningAsync(8000, allowMultipleBindToSamePort: true);
            await udpMulticastListener.JoinMulticastGroupAsync("239.255.255.250", 1900, allowMultipleBindToSamePort: true);
        }
    }
}
