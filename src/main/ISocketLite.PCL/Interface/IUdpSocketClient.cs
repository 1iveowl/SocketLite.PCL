using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketClient : IDisposable
    {
        //event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;
        ISubject<IUdpMessage> ObservableMessages { get; }

        Task ConnectAsync(string address, int port);

        Task SendAsync(byte[] data);

        Task SendAsync(byte[] data, int length);

        Task SendToAsync(byte[] data, string address, int port);

        Task SendToAsync(byte[] data, int length, string address, int port);
        void Disconnect();
    }
}
