using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Model;
using SocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services.Base
{
    public abstract class UdpSocketBase : UdpSendBase
    {
        private ISubject<IUdpMessage> ObsMsg { get; } = new Subject<IUdpMessage>();

        public IObservable<IUdpMessage> ObservableMessages => ObsMsg.AsObservable();
 
        protected void RunMessageReceiver(CancellationToken cancelToken)
        {
            var observeUdpReceive = Observable.While(
                () => !cancelToken.IsCancellationRequested,
                Observable.FromAsync(BackingUdpClient.ReceiveAsync))
                .Select(msg =>
                {
                    var message = new UdpMessage
                    {
                        ByteData = msg.Buffer,
                        RemotePort = msg.RemoteEndPoint.Port.ToString(),
                        RemoteAddress = msg.RemoteEndPoint.Address.ToString()
                    };

                    return message;
                }).SubscribeOn(Scheduler.Default);

            observeUdpReceive.Subscribe(
                // Message Received Args (OnNext)
                args =>
                {
                    ObsMsg.OnNext(args);
                },
                // Exception (OnError)
                ex =>
                {
                    throw (NativeSocketExceptions.Contains(ex.GetType()))
                        ? new SocketException(ex)
                        : ex;
                }, cancelToken);
        }

        protected void InitializeUdpClient(IPEndPoint ipEndPoint, bool allowMultipleBindToSamePort)
        {
            BackingUdpClient = new UdpClient
            {
                EnableBroadcast = true,
            };

            if (allowMultipleBindToSamePort)
            {
                BackingUdpClient.ExclusiveAddressUse = false;
                BackingUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }

            try
            {
                BackingUdpClient.Client.Bind(ipEndPoint);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                throw new SocketException(ex);
            }
        }

        public void Dispose()
        {
            BackingUdpClient.Close();
            ObsMsg.OnCompleted();
        }
    }
}
