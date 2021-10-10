using NewSocket.Interfaces;
using NewSocket.Models;
using NewSocket.Protocals.RPC.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
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

        public bool IsResponse { get; private set; }

        public string? LocalMethod { get; private set; }

        public List<string> ParameterJson => m_Parameters;

        public int ObjectCount { get; private set; }

        public ulong RPCID { get; private set; }

        public RPCProtocal Protocal { get; }

        private bool m_Init = true;
        private List<string> m_Parameters = new List<string>();
        private MarshalAllocMemoryStream? m_CurrentObject;
        private int m_ParamPosititon = 0;
        private byte[]? m_Buffer = new byte[1024 * 6];

        public RPCDown(ulong messageID, RPCProtocal protocal)
        {
            MessageID = messageID;
            Protocal = protocal;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public async Task<bool> Read(Stream stream, CancellationToken token)
        {
            if (m_Init)
            {
                IsResponse = await stream.NetReadBool();
                ObjectCount = await stream.NetReadInt32();
                RPCID = await stream.NetReadUInt64();

                m_Init = false;

                if (!IsResponse)
                {
                    LocalMethod = await stream.NetReadString();
                }

                if (ObjectCount == 0)
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
            if (m_Buffer == null)
            {
                throw new InvalidOperationException("Buffer was null");
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

                if (m_ParamPosititon == ObjectCount)
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
            if (IsResponse)
            {
                var response = new RPCData(ParameterJson);
                Protocal.RequestRegistry.ReleaseRequest(RPCID, response);
            }
            else
            {
                Protocal.DispatchRPC(RPCID, LocalMethod, new RPCData(ParameterJson));
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            m_CurrentObject?.Dispose();
            m_Buffer = null;
            m_Parameters.Clear();
        }
    }
}