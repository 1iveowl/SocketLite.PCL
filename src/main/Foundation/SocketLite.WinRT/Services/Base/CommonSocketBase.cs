using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
