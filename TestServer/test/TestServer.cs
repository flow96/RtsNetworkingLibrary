using System.Threading;
using RtsNetworkingLibrary.server;
using RtsNetworkingLibrary.server.utils;

namespace TestServer.test
{
    
    
    
    public class TestServer
    {
        private Server _server;
        
        /**
         * Sets up and automatically starts a server
         */
        public TestServer()
        {
            ServerSettings settings = new ServerSettings();
            settings.port = 4045;
            settings.maxPlayers = 2;
            settings.headerBufferByteSze = 4;
            _server = new Server();
            _server.StartServer(settings);
        }
    }
}