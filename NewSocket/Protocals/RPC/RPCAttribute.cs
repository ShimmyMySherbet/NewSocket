using System;
using System.ComponentModel;

namespace NewSocket.Protocals.RPC
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true), DisplayName("RPC")]
    public class RPCAttribute : Attribute
    {
        public string? MethodName { get; }

        public RPCAttribute(string name)
        {
            MethodName = name;
        }

        public RPCAttribute()
        {
            MethodName = null;
        }
    }
}