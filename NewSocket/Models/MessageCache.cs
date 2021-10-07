using NewSocket.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NewSocket.Models
{
    public class MessageCache : IDisposable
    {
        private IDictionary<ulong, IMessageDown> m_Downloads = new ConcurrentDictionary<ulong, IMessageDown>();
        private IDictionary<ulong, IMessageUp> m_Uploads = new ConcurrentDictionary<ulong, IMessageUp>();
        private ulong m_DownIDIndex = 0;
        private bool m_DownStarted = false;

        public bool TryGetDownload(ulong messageID, out IMessageDown down)
        {
            return m_Downloads.TryGetValue(messageID, out down);
        }

        public bool TryGetUpload(ulong messageID, out IMessageUp up)
        {
            return m_Uploads.TryGetValue(messageID, out up);
        }

        public void Register(IMessageDown down)
        {
            if (!m_DownStarted || down.MessageID > m_DownIDIndex)
            {
                m_DownIDIndex = down.MessageID;
            }
            m_DownStarted = true;
            m_Downloads[down.MessageID] = down;
        }

        public void Register(IMessageUp up)
        {
            m_Uploads[up.MessageID] = up;
        }

        public void Destroy(IMessageDown down)
        {
            if (m_Downloads.ContainsKey(down.MessageID))
            {
                m_Downloads.Remove(down.MessageID);
                down.Dispose();
            }
        }

        public void Destroy(IMessageUp up)
        {
            if (m_Uploads.ContainsKey(up.MessageID))
            {
                m_Uploads.Remove(up.MessageID);
                up.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var msg in m_Downloads)
            {
                msg.Value.Dispose();
            }
            m_Downloads.Clear();
            foreach (var msg in m_Uploads)
            {
                msg.Value.Dispose();
            }
            m_Uploads.Clear();
        }

        public bool IsNewMessage(ulong messageID)
        {
            return m_DownStarted ? messageID > m_DownIDIndex : true;
        }
    }
}