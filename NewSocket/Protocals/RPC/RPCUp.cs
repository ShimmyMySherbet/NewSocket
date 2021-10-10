using NewSocket.Interfaces;
using NewSocket.Models;
using NewSocket.Protocals.RPC.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC
{
    public class RPCUp : IMessageUp
    {
        public ulong MessageID { get; }

        public byte MessageType => 1;

        public bool Complete { get; private set; } = false;

        public bool IsResponse { get; }

        public object?[] Parameters { get; }

        public string? RemoteMethod { get; }

        public bool WantsResponse { get; } = false;

        public ulong RPCMessageID { get; } = 0;

        public RPCHandle Handle { get; }

        private MarshalAllocMemoryStream? m_CurrentObject;
        private IEnumerator? m_ParameterSource;
        private int m_Sent = 0;
        private long? m_CurrentObjectRemainingBytes => m_CurrentObject?.Length - m_CurrentObject?.Position;

        private int m_MaxTransferSize => 1024 * 8;

        private byte[] m_Buffer;

        public RPCUp(ISocketClient client, RPCHandle handle, string remoteMethod, params object?[] parameters)
        {
            MessageID = handle.MessageID;
            RPCMessageID = handle.RPCID;
            RemoteMethod = remoteMethod;
            Parameters = parameters;
            IsResponse = false;
            m_Buffer = new byte[client.UpBufferSize];
            m_ParameterSource = parameters.GetEnumerator();
            IsResponse = false;
            Handle = handle;
        }

        public RPCUp(ISocketClient client, RPCHandle handle, object? response)
        {
            MessageID = handle.MessageID;
            RPCMessageID = handle.RPCID;
            RemoteMethod = null;
            Parameters = new object?[] { response };
            IsResponse = true;
            m_Buffer = new byte[client.UpBufferSize];
            m_ParameterSource = Parameters.GetEnumerator();
            Handle = handle;
        }

        public RPCUp(ISocketClient client, RPCHandle handle)
        {
            MessageID = handle.MessageID;
            RPCMessageID = handle.RPCID;
            RemoteMethod = null;
            Parameters = new object[0];
            IsResponse = true;
            m_Buffer = new byte[client.UpBufferSize];
            m_ParameterSource = Parameters.GetEnumerator();
            Handle = handle;
        }

        private bool m_Init = true;

        /* <Init>
         *     [bool]    IsResponse
         *     [int]     Parameter Count
         *     [Ulong]  RPC ID       <Request: New ID|Response: Origonal ID>
         *
         *     <if Not Response>
         *          [String] Method
         *
         * <Parameter>
         *  <init>
         *      [Long] Parameter Length
         *
         *  <segment>
         *      [Long]  Segment Length
         *      [Bytes] Segment Data
         */

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public async Task<bool> Write(Stream stream)
        {
            if (m_Init)
            {
                var length = Parameters.Length;

                await stream.Write(IsResponse);
                await stream.Write(length);
                await stream.Write(RPCMessageID);
                if (!IsResponse)
                {
                    await stream.Write(RemoteMethod);
                }
                m_Init = false;
            }

            if (m_CurrentObject == null)
            {
                if (m_ParameterSource == null)
                {
                    throw new InvalidOperationException();
                }
                if (m_ParameterSource.MoveNext())
                {
                    var newParameter = m_ParameterSource.Current;
                    m_CurrentObject = new MarshalAllocMemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newParameter)));
                }
                else
                {
                    Complete = true;
                    return true;
                }
            }

            if (m_CurrentObject.Position == 0)
            {
                await stream.Write(m_CurrentObject.Length);
            }

            var transferSize = (m_CurrentObjectRemainingBytes ?? 0) < m_MaxTransferSize ? (m_CurrentObjectRemainingBytes ?? 0) : m_MaxTransferSize;

            await stream.Write(transferSize);
            var remaining = transferSize;
            while (remaining > 0)
            {
                var segmentSize = remaining < m_Buffer.Length ? remaining : m_Buffer.Length;
                var read = await m_CurrentObject.ReadAsync(m_Buffer, 0, (int)segmentSize, CancellationToken.None);
                await stream.WriteAsync(m_Buffer, 0, read, CancellationToken.None);
                remaining -= read;
            }

            if (m_CurrentObjectRemainingBytes == 0)
            {
                m_Sent++;
                m_CurrentObject.Dispose();
                m_CurrentObject = null;
            }

            if (m_Sent == Parameters.Length)
            {
                Complete = true;
                return true;
            }

            return false;
        }

        public void Dispose()
        {
        }
    }
}