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
            switch (message)
            {
                case BuildMessage b:
                    HandleBuildMessage(b);
                    break;
                case DestroyMessage d:
                    HandleDestroyMessage(d);
                    break;
                case DisconnectMessage d:
                    HandleDisconnectMessage(d);
                    break;
                case TransformUpdate t:
                    HandleTransformUpdateMessage(t);
                    break;
                default:
                    handled = false;
                    break;
            }
            return handled;
        }
        protected abstract void HandleBuildMessage(BuildMessage buildMessage);
        
        protected  abstract void HandleDestroyMessage(DestroyMessage message);

        protected abstract void HandleTransformUpdateMessage(TransformUpdate transformUpdate);
        
        protected  abstract void HandleDisconnectMessage(DisconnectMessage message);
    }
}