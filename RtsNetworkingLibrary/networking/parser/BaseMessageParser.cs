using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using UnityEngine;

namespace RtsNetworkingLibrary.networking.parser
{
    public abstract class BaseMessageParser : MonoBehaviour
    {

        public bool ParseMessage(NetworkMessage message)
        {
            bool handled = true;
            if (message is BuildMessage)
            {
                HandleBuildMessage((BuildMessage) message);
            }
            else if (message is DestroyMessage)
            {
                HandleDestroyMessage((DestroyMessage) message);
            }
            else if (message is DisconnectMessage)
            {
                HandleDisconnectMessage((DisconnectMessage)message);
            }
            else
                handled = false;

            return handled;
        }
        protected abstract void HandleBuildMessage(BuildMessage buildMessage);
        
        protected  abstract void HandleDestroyMessage(DestroyMessage message);
        
        protected  abstract void HandleDisconnectMessage(DisconnectMessage message);
    }
}