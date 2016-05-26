#SocketLite Xamarin PCL: iOS, UWP, Android and .NET 4.5+

This project is a fork that build upon the fantastic work done one [Socket for PCL](https://github.com/rdavisau/sockets-for-pcl). 

Why this fork? Two reasons:

 1. The original Socket for PCL delivers great broad cross-platform support. SocketLite PCL only covers .NET 4.5+, UWP, iOS and Android, making it simpler but also more narrow in scope.
 2. SocketLite has been refactored to use Reactive Extensions (Rx) internally as well as externally - i.e. this PCL requires Rx.

To overarching purpose of a PCL like this is to make it easie to write socket code in PCL, simplifying cross-platform usage of socket.. 

This library is based on "Bait and Switch" pattern. I would strongly recommend to read this short and great blog post to get an understanding of this pattern before trying to contribute or edit in the SocketLite PCL code-base: [The Bait and Switch PCL Trick](http://log.paulbetts.org/the-bait-and-switch-pcl-trick/)

Get SocketLite.PCL in NuGet: ````Install-Package SocketLite.PCL````

### Classes
The plugin currently provides the following socket abstractions:

Class|Description|.NET|Windows 10 / UWP
-----|-----------|:--------------:|:---------------:
**TcpSocketListener** | Bind to a port and accept TCP socket connections. | TcpListener | StreamSocketListener 
**TcpSocketClient** | Connect to a TCP endpoint with bi-directional communication. | TcpClient | StreamSocket
**UdpSocketReceiver** | Bind to a port and receive UDP messages. | UdpClient | DatagramSocket
**UdpSocketClient** | Send messages to arbitrary endpoints over UDP. | UdpClient | DatagramSocket
**UdpSocketMulticastClient** | Send and receive UDP messages within a multicast group. | UdpClient | DatagramSocket


### Examples Usage

##### A TCP listener
```cs
var tcpListener = new SocketLite.Services.TcpSocketListener();
await tcpListener.StartListeningAsync(80, allowMultipleBindToSamePort: true);

var tcpSubscriber = tcpListener.ObservableTcpSocket.Subscribe(
	x =>
	{
	    System.Console.WriteLine($"Remote Address: {x.RemoteAddress}");
        System.Console.WriteLine($"Remote Port: {x.RemotePort}");
        System.Console.WriteLine("---***---");
	ex =>
    {
	    // Exceptions received here;
    }););
	
tcpSubscriber.Dispose();
```


##### A TCP client
```cs
var tcpClient = new TcpSocketClient();
await tcpClient.ConnectAsync("192.168.1.100", 1234);

var helloWorld = "Hello World!";

var bytes = Encoding.UTF8.GetBytes(helloWorld);
await tcpClient.WriteStream.WriteAsync(bytes, 0, bytes.Length);
tcpClient.Disconnect();
```

    
##### A UDP receiver
```cs
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

// When done dispose
//udpMessageSubscriber.Dispose();
```

##### A UDP client
```cs
var udpClient = new UdpSocketClient();

var helloWorld = "Voyager 1";

var bytes = Encoding.UTF8.GetBytes(helloWorld);

// Fire datagram into the great void
await udpClient.SendToAsync(bytes, bytes.Length, address:"192.168.1.5", port:1234);
```

##### A multicast UDP client
```cs
var udpMulticast = new SocketLite.Services.UdpSocketMulticastClient();
await udpMulticast.JoinMulticastGroupAsync("239.255.255.250", 1900, allowMultipleBindToSamePort:true); //Listen for UPnP activity on local network.

// Listen part
var tcpSubscriber = udpMulticast.ObservableMessages.Subscribe(
    x =>
    {
        System.Console.WriteLine($"Remote Address: {x.RemoteAddress}");
        System.Console.WriteLine($"Remote Port: {x.RemotePort}");
        System.Console.WriteLine($"Data/string: {Encoding.UTF8.GetString(x.ByteData)}");
        System.Console.WriteLine("***");
    });

// When Done Dispose
//tcpSubscriber.Dispose();

// Send part
//var msg = "Hello everyone";
//var bytes = Encoding.UTF8.GetBytes(msg);

//await udpMulticast.SendMulticastAsync(bytes);

//udpMulticast.Disconnect();
```
