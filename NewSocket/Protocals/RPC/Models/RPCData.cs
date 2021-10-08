using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC.Models
{
    public class RPCData
    {
        public List<string> Objects { get; } = new List<string>();

        public T ReadObject<T>(int index)
        {
            return JsonConvert.DeserializeObject<T>(Objects[index]);
        }

        public object ReadObject(int index, Type type)
        {
            return JsonConvert.DeserializeObject(Objects[index], type);
        }

        public RPCData(List<string> objects)
        {
            Objects = objects;
        }
    }
}
