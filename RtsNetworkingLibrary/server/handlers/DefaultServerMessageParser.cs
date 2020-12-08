using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.parser;
using Logger = RtsNetworkingLibrary.utils.Logger;

namespace RtsNetworkingLibrary.server.handlers
{
    public class DefaultServerMessageParser : BaseMessageParser
    {
        private Logger _logger;
        private NetworkManager _networkManager;
        private static ulong _entityCounter = 1;

        public DefaultServerMessageParser()
        {
            _logger = new Logger(this.GetType().Name);
        }

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
        }


        protected override void HandleBuildMessage(BuildMessage buildMessage)
        {
           _logger.Debug("Server received handle build message");
           buildMessage.entityId = _entityCounter++;
           _networkManager.TcpServerSendBroadcast(buildMessage);
        }

        protected override void HandleDestroyMessage(DestroyMessage destroyMessage)
        {
            _logger.Debug("Handling Destroy message");
            _networkManager.TcpServerSendBroadcast(destroyMessage);
        }

        protected override void HandleTransformUpdateMessage(TransformUpdate transformUpdate)
        {
            _logger.Debug("Handling Transform Update message" + transformUpdate.entityId + " | " + transformUpdate.position + " | " + transformUpdate.rotation);
            _networkManager.TcpServerSendBroadcast(transformUpdate, transformUpdate.userId);
        }

        protected override void HandleDisconnectMessage(DisconnectMessage disconnectMessage)
        {
            _logger.Debug("Received disconnect from: " + disconnectMessage.username + " (" + disconnectMessage.userId + ")");
            _networkManager.Server.DisconnectClient(disconnectMessage.userId);
        }
    }
}