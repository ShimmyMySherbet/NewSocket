using NewSocket.Interfaces;
using NewSocket.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC
{
    public class RPCDown : IMessageDown
    {
        public byte MessageType => 1;

        public bool WantsToDispatch { get; private set; } = false;

        public ulong MessageID { get; }

        public bool Complete { get; private set; } = false;
        public string LocalMethod { get; private set; }

        public List<string> ParameterJson => m_Parameters;
        
        private int m_ParamCount;
        public bool IsResponse { get; private set; }
        public ulong OriginMessage { get; private set; }

        private bool m_WantsResponse;


        private bool m_Init = true;

        private List<string> m_Parameters = new List<string>();
        private MarshalAllocMemoryStream m_CurrentObject;
        private int m_ParamPosititon = 0;
        private byte[] m_Buffer = new byte[1024 * 6];

        public RPCDown(ulong messageID)
        {
            MessageID = MessageID;
        }

        public async Task<bool> Read(Stream stream, CancellationToken token)
        {
            if (m_Init)
            {
                LocalMethod = await stream.NetReadString();
                m_ParamCount = await stream.NetReadInt32();
                m_WantsResponse = await stream.NetReadBool();
                IsResponse = await stream.NetReadBool();
                if (IsResponse)
                {
                    OriginMessage = await stream.NetReadUInt64();
                }
                m_Init = false;

                if (m_ParamCount == 0)
                {
                    WantsToDispatch = true;
                    Complete = true;
                    return true;
                }
            }

            if (m_CurrentObject == null)
            {
                m_ParamPosititon++;
                var paramLength = await stream.NetReadInt64();
                m_CurrentObject = new MarshalAllocMemoryStream((int)paramLength);
            }

            var transferSize = await stream.NetReadInt64();
            var remaining = transferSize;
            while (remaining > 0)
            {
                var nextRead = remaining < m_Buffer.Length ? remaining : m_Buffer.Length;
                var read = await stream.ReadAsync(m_Buffer, 0, (int)nextRead);
                await m_CurrentObject.WriteAsync(m_Buffer, 0, read, token);
                remaining -= read;
            }

            if (m_CurrentObject.Position == m_CurrentObject.Length)
            {
                string json;
                m_CurrentObject.Position = 0;
                using (var reader = new StreamReader(m_CurrentObject))
                    json = await reader.ReadToEndAsync();
                m_Parameters.Add(json);
                m_CurrentObject.Dispose();
                m_CurrentObject = null;

                if (m_ParamPosititon == m_ParamCount)
                {
                    WantsToDispatch = true;
                    Complete = true;
                    return true;
                }
            }
            return false;
        }

        public Task Dispatch()
        {
            Console.WriteLine($"Param Count: {m_ParamCount}");
            Console.WriteLine($"IsResponse: {IsResponse}");
            Console.WriteLine($"OrigonalMessage: {OriginMessage}");
            Console.WriteLine($"Wants Response: {m_WantsResponse}");
            Console.WriteLine($"Method: {LocalMethod}");
            foreach (var p in m_Parameters)
            {
                Console.WriteLine($"[param]\n{p}");
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            m_CurrentObject?.Dispose();
            m_Buffer = null;
            m_Parameters = null;
        }
    }
}