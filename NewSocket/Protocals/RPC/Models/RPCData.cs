using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NewSocket.Protocals.RPC.Models
{
    public class RPCData
    {
        public readonly ReadOnlyCollection<string> Objects;

        public RPCData(List<string> objects)
        {
            Objects = objects.AsReadOnly();
        }

        public T? ReadObject<T>(int index)
        {
            return JsonConvert.DeserializeObject<T>(Objects[index]);
        }

        public object? ReadObject(int index, Type type)
        {
            return JsonConvert.DeserializeObject(Objects[index], type);
        }
    }
}