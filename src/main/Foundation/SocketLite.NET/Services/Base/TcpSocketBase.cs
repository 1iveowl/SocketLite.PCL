namespace SocketLite.Services.Base
{
    public abstract class TcpSocketBase : CommonSocketBase
    {
        protected readonly int BufferSize;

        protected TcpSocketBase(int bufferSize)
        {
            BufferSize = bufferSize;
        }
    }
}
