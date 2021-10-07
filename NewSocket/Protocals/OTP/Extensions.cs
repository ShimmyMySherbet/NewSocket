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
            var msg = otp.CreateUp(client.UpIDAssigner.AssignID(), client, channel, obj);
            client.Enqueue(msg);
        }

        public static void OTPSend(this BaseSocketClient client, string channel, Stream stream)
        {
            var otp = client.GetProtocal<ObjectTransferProtocal>();
            var msg = otp.CreateUp(client.UpIDAssigner.AssignID(), client, channel, stream: stream);
            client.Enqueue(msg);
        }
    }
}
