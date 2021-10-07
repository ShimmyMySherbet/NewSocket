using NewSocket.Interfaces;
using NewSocket.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
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

        public object[] Parameters { get; }

        public string RemoteMethod { get; }

        public ulong RespondingTo { get; }

        public bool WantsResponse { get; } = false;

        private MarshalAllocMemoryStream m_CurrentObject;
        private IEnumerator m_ParameterSource;

        private long m_CurrentObjectRemainingBytes => m_CurrentObject.Length - m_CurrentObject.Position;

        private int m_MaxTransferSize => 1024 * 8;

        private byte[] m_Buffer;



        public RPCUp(ulong messageID, string method, object[] parameters, int bufferSize)
        {
            RemoteMethod = method;
            Parameters = parameters;
            IsResponse = false;
            RespondingTo = 0;
            MessageID = messageID;
            m_ParameterSource = parameters.GetEnumerator();
            m_Buffer = new byte[bufferSize];
        }

        public RPCUp(ulong messageID, string method, ulong replyingTo, object response, int bufferSize)
        {
            RemoteMethod = method;
            Parameters = new object[] { response };
            IsResponse = true;
            RespondingTo = replyingTo;
            MessageID = messageID;
            m_ParameterSource = Parameters.GetEnumerator();
            m_Buffer = new byte[bufferSize];
        }


        private bool m_Init = true;

        /* <Init>
         *     [bool]    IsResponse
         *     [int]     Parameter Count
         *     <if Response>
         *         [ulong]  Origonal RPC ID
         *         
         *     <if Request>
         *          [Ulong]  RPC ID
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


        public async Task<bool> Write(Stream stream)
        {
            if (m_Init)
            {
                await stream.Write(RemoteMethod);
                await stream.Write(Parameters.Length);
                await stream.Write(WantsResponse);
                await stream.Write(IsResponse);
                if (IsResponse)
                    await stream.Write(RespondingTo);
                m_Init = false;
            }

            if (m_CurrentObject == null)
            {
                if (m_ParameterSource.MoveNext())
                {
                    var newParameter = m_ParameterSource.Current;
                    m_CurrentObject = new MarshalAllocMemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newParameter)));
                }
                else
                {
                    return true;
                }
            }
            
            if (m_CurrentObject.Position == 0)
            {
                await stream.Write(m_CurrentObject.Length);
            }

            var transferSize = m_CurrentObjectRemainingBytes < m_MaxTransferSize ? m_CurrentObjectRemainingBytes : m_MaxTransferSize;


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
                m_CurrentObject.Dispose();
                m_CurrentObject = null;
            }

            return false;
        }

     
        public void Dispose()
        {
        }
    }
}