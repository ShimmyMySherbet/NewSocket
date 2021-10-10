using NewSocket.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket.Protocals.OTP
{
    public static class Extensions
    {
        public static void OTPSend(this BaseSocketClient client, string channel, object obj)
        {
            var otp = client.GetProtocal<ObjectTransferProtocal>();
            if (otp == null)
            {
                throw new InvalidOperationException("The specified client does not support teh OTP protocal.");
            }
            var msg = otp.CreateUp(client.MessageIDAssigner.AssignID(), client, channel, obj);
            client.Enqueue(msg);
        }

        public static void OTPSend(this BaseSocketClient client, string channel, Stream stream)
        {
            var otp = client.GetProtocal<ObjectTransferProtocal>();
            if (otp == null)
            {
                throw new InvalidOperationException("The specified client does not support teh OTP protocal.");
            }
            var msg = otp.CreateUp(client.MessageIDAssigner.AssignID(), client, channel, stream: stream);
            client.Enqueue(msg);
        }
    }
}
