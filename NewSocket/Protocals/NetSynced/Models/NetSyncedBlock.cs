using NewSocket.Models;

namespace NewSocket.Protocals.NetSynced.Models
{
    public class NetSyncedBlock
    {
        public bool Disposal = false;

        public MarshalAllocMemoryStream? MemoryBlock;

        public static NetSyncedBlock Block(MarshalAllocMemoryStream memory)
        {
            return new NetSyncedBlock()
            {
                Disposal = false,
                MemoryBlock = memory
            };
        }

        public static NetSyncedBlock Dispose()
        {
            return new NetSyncedBlock()
            {
                Disposal = true,
                MemoryBlock = null
            };
        }

        private NetSyncedBlock()
        {
        }
    }
}