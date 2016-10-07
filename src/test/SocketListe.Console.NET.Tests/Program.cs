using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketLite.Model;
using SocketLite.Services;

namespace SocketListe.Console.NET.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //var comm = new CommunicationsInterface();

            //var allComm = comm.GetAllInterfaces();

            //StartTcpListener();
            StartTcpClient();

            //StartUdpMulticastClient();

            System.Console.ReadKey();
        }

        private static async void StartTcpClient()
        {
            var tcpClient = new TcpSocketClient();
            await tcpClient.ConnectAsync("www.abc.dk", "8088", secure: true, ignoreServerCertificateErrors: true);

            var helloWorld = "Hello World!";

            var bytes = Encoding.UTF8.GetBytes(helloWorld);
            await tcpClient.WriteStream.WriteAsync(bytes, 0, bytes.Length);
            tcpClient.Disconnect();
            tcpClient.Dispose();
        }

        private static async void UdpListener()
        {
            var udpReceived = new UdpSocketReceiver();
            await udpReceived.StartListeningAsync(1234, allowMultipleBindToSamePort: true);

            var udpMessageSubscriber = udpReceived.ObservableMessages.Subscribe(
                msg =>
                {
                    System.Console.WriteLine($"Remote adrres: {msg.RemoteAddress}");
                    System.Console.WriteLine($"Remote port: {msg.RemotePort}");

                    var str = System.Text.Encoding.UTF8.GetString(msg.ByteData);
                    System.Console.WriteLine($"Messsage: {str}");
                },
                ex =>
                {
                    // Exceptions received here;
                });

            udpMessageSubscriber.Dispose();
        }

        private static async void UdpClient()
        {
            var udpClient = new UdpSocketClient();

            var helloWorld = "Hello World!";

            var bytes = Encoding.UTF8.GetBytes(helloWorld);
            await udpClient.SendToAsync(bytes, bytes.Length, address:"192.168.1.5", port:1234);
        }


        private static async void StartTcpListener()
        {
            var tcpListener = new SocketLite.Services.TcpSocketListener();

            await tcpListener.StartListeningAsync(8000, allowMultipleBindToSamePort: true);

            var tcpSubscriber = tcpListener.ObservableTcpSocket.Subscribe(
                x =>
                {
                    System.Console.WriteLine($"Remote Address: {x.RemoteAddress}");
                    System.Console.WriteLine($"Remote Port: {x.RemotePort}");
                    System.Console.WriteLine("--------------***-------------");
                },
                ex =>
                {
                    // Exceptions received here;
                });


            //tcpSubscriber.Dispose();
        }

        private static async void StartUdpMulticastClient()
        {
            var udpMulticast = new SocketLite.Services.UdpSocketMulticastClient();
            await udpMulticast.JoinMulticastGroupAsync("239.255.255.250", 1900, allowMultipleBindToSamePort:true); //Listen for UPnP activity on local network.

            // Listen part
            var tcpSubscriber = udpMulticast.ObservableMessages.Subscribe(
                x =>
                {
                    System.Console.WriteLine($"Remote Address: {x.RemoteAddress}");
                    System.Console.WriteLine($"Remote Port: {x.RemotePort}");
                    System.Console.WriteLine($"Date: {Encoding.UTF8.GetString(x.ByteData)}");
                    System.Console.WriteLine("--------------***-------------");
                });

            //tcpSubscriber.Dispose();

            // Send part
            //var msg = "Hello everyone";
            //var bytes = Encoding.UTF8.GetBytes(msg);

            //await udpMulticast.SendMulticastAsync(bytes);

            //udpMulticast.Disconnect();
        }
    }
}


//var msg = "HELLO MULTIVERSE";
//var msgBytes = Encoding.UTF8.GetBytes(msg);

//// send a message that will be received by all listening in
//// the same multicast group. 
//await receiver.SendMulticastAsync(msgBytes);

//System.Text.Encoding.UTF8.GetString(byteArray);