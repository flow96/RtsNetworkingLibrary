using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.manager;
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


        protected override void HandleClientConnectMessage(ConnectMessage connectMessage)
        {
            _logger.Debug("Received Connect message");
            _networkManager.TcpServerSendBroadcast(connectMessage, connectMessage.playerInfo.userId);
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

        protected override void HandleTransformUpdateMessage(TransformUpdateMessage transformUpdateMessage)
        {
            _logger.Debug("Handling Transform Update message id: " + transformUpdateMessage.entityId + " | position: " + transformUpdateMessage.position + " | rotation: " + transformUpdateMessage.rotation);
            _networkManager.TcpServerSendBroadcast(transformUpdateMessage, transformUpdateMessage.playerInfo.userId);
        }

        protected override void HandleTransformUpdateListMessage(TransformUpdateListMessage transformUpdateListMessage)
        {
            _logger.Debug("Handling Transform Update List Message with: " + transformUpdateListMessage.updates.Length + " changes");
            _networkManager.TcpServerSendBroadcast(transformUpdateListMessage, transformUpdateListMessage.playerInfo.userId);
        }

        protected override void HandleUpdateSyncVarMessage(UpdateSyncVarMessage updateSyncVarMessage)
        {
            _logger.Debug("Handling Update Sync Var Message with: " + updateSyncVarMessage.data.Length + " changes");
            _networkManager.TcpServerSendBroadcast(updateSyncVarMessage, updateSyncVarMessage.playerInfo.userId);
        }

        protected override void HandleDisconnectMessage(DisconnectMessage disconnectMessage)
        {
            _logger.Debug("Received disconnect from: " + disconnectMessage.playerInfo.username + " (" + disconnectMessage.playerInfo.userId + ")");
            _networkManager.Server.DisconnectClient(disconnectMessage.playerInfo.userId);
        }
    }
}