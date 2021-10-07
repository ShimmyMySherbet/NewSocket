using System.IO;
using System.Threading.Tasks;

namespace NewSocket.Protocals.OTP
{
    public delegate Task OTPMessageRecivedArgs(string channel, Stream content);
}