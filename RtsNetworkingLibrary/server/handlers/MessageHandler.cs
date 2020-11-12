using System;
using System.Collections.Generic;
using RtsNetworkingLibrary.networking.messages;
using UnityEngine;

namespace RtsNetworkingLibrary.server.handlers
{
    public class MessageHandler : MonoBehaviour
    {

        public delegate void MessageParser(NetworkMessage networkMessage);
        
        
        public MessageParser[] parsers = new MessageParser[10];
    }
}