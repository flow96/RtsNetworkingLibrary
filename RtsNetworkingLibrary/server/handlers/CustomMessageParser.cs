using System;
using RtsNetworkingLibrary.networking.messages.@base;
using UnityEngine.Events;

namespace RtsNetworkingLibrary.server.handlers
{
    [Serializable]
    public class CustomMessageParser : UnityEvent<InboundMessage>
    {
    }
}