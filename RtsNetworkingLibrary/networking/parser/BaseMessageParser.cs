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
                case ConnectMessage c:
                    HandleClientConnectMessage(c);
                    break;
                case BuildMessage b:
                    HandleBuildMessage(b);
                    break;
                case DestroyMessage d:
                    HandleDestroyMessage(d);
                    break;
                case DisconnectMessage d:
                    HandleDisconnectMessage(d);
                    break;
                case TransformUpdateMessage t:
                    HandleTransformUpdateMessage(t);
                    break;
                case UpdateSyncVarMessage us:
                    HandleUpdateSyncVarMessage(us);
                    break;
                case TransformUpdateListMessage tUpdate:
                    HandleTransformUpdateListMessage(tUpdate);
                    break;
                default:
                    handled = false;
                    break;
            }
            return handled;
        }

        protected abstract void HandleClientConnectMessage(ConnectMessage connectMessage);
        
        protected abstract void HandleBuildMessage(BuildMessage buildMessage);
        
        protected  abstract void HandleDestroyMessage(DestroyMessage message);

        protected abstract void HandleTransformUpdateMessage(TransformUpdateMessage transformUpdateMessage);

        protected abstract void HandleTransformUpdateListMessage(TransformUpdateListMessage transformUpdateListMessage);

        protected abstract void HandleUpdateSyncVarMessage(UpdateSyncVarMessage updateSyncVarMessage);
        
        protected  abstract void HandleDisconnectMessage(DisconnectMessage message);
    }
}