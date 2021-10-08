using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NewSocket.Protocals.RPC.Models
{
    public class RPCParameters
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

        public RPCParameters(List<string> objects)
        {
            Objects = objects;
        }
    }
}