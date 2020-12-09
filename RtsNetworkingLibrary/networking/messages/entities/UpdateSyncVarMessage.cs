using System;
using System.Collections.Generic;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class UpdateSyncVarMessage : NetworkMessage
    {
        public readonly SyncVarData[] data;
        public readonly ulong entityId;

        [Serializable]
        public class SyncVarData
        {
            public readonly string name;
            public readonly object value;

            public SyncVarData(string name, object value)
            {
                this.name = name;
                this.value = value;
            }
        }

        public UpdateSyncVarMessage(SyncVarData[] data, ulong entityId)
        {
            this.data = data;
            this.entityId = entityId;
        }
    }
}