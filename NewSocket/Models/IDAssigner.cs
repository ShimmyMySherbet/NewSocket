namespace NewSocket.Models
{
    public class IDAssigner
    {
        private object m_Lock = new object();
        private ulong m_Index = 0;
        private ulong m_Current = 0;

        public ulong AssignID()
        {
            lock (m_Lock)
            {
                m_Current = m_Index;
                m_Index++;
                return m_Current;
            }
        }
    }
}