using System.Threading;
using Android.App;
using Android.Widget;
using Android.OS;
using ISocketLite.PCL.Model;
using SocketLite.Services;

namespace SocketLite.Android.Test
{
    [Activity(Label = "SocketLite.Android.Test", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            StartTest();

            // Set our view from the "main" layout resource
            // SetContentView (Resource.Layout.Main);
        }

        private async void StartTest()
        {
            var tcpClient = new TcpSocketClient();
            await tcpClient.ConnectAsync(
                "echo.websocket.org", 
                "443", 
                true, 
                CancellationToken.None, 
                ignoreServerCertificateErrors: true, 
                tlsProtocolVersion:TlsProtocolVersion.Tls10);
        }
    }
}

