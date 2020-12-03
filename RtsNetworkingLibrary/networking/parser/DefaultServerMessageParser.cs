using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using Logger = RtsNetworkingLibrary.utils.Logger;

namespace RtsNetworkingLibrary.networking.parser
{
    public class DefaultServerMessageParser : BaseMessageParser
    {
        private Logger _logger;
        private NetworkManager _networkManager;

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
           _networkManager.TcpServerSendBroadcast(buildMessage);
        }

        protected override void HandleDestroyMessage(DestroyMessage destroyMessage)
        {
            _logger.Debug("Handling Destroy message");
            _networkManager.TcpServerSendBroadcast(destroyMessage);
        }
        
        protected override void HandleDisconnectMessage(DisconnectMessage disconnectMessage)
        {
            _logger.Debug("Received disconnect from: " + disconnectMessage.username + " (" + disconnectMessage.userId + ")");
            _networkManager.Server.DisconnectClient(disconnectMessage.userId);
        }
    }
}