using System;
using ISocketLite.PCL.Interface;

namespace SocketLite.Services.Base
{
    public abstract class CommonSocketBase
    {
        protected void CheckCommunicationInterface(ICommunicationEntity communicationEntity)
        {
            if (communicationEntity != null && !communicationEntity.IsUsable)
            {
                throw new InvalidOperationException("Cannot listen on an unusable communication interface.");
            }
        }
    }
}
