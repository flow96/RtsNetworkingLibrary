

namespace RtsNetworkingLibrary.networking.messages.@base
{
    public class InboundMessage
    {
        public readonly NetworkMessage networkMessage;
        public readonly int userId;

        public InboundMessage(NetworkMessage networkMessage, int userId)
        {
            this.networkMessage = networkMessage;
            this.userId = userId;
        }
    }
}