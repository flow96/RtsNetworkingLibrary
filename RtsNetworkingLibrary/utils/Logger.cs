using System;

namespace RtsNetworkingLibrary.utils
{
    public class Logger
    {
        public static LoggerType LoggerType { get; set; } = LoggerType.UNITY;

        private readonly string prefix;

        public Logger(string prefix)
        {
            this.prefix = prefix;
        }

        public void Debug(object msg)
        {
            if(LoggerType == LoggerType.UNITY)
                UnityEngine.Debug.Log(GetFormattedMessage(msg));
            else if(LoggerType == LoggerType.DEDICATED)
                Console.WriteLine(GetFormattedMessage(msg));
        }


        private string GetFormattedMessage(object msg)
        {
            return DateTime.Now.Millisecond + " >> " + prefix + ": " + msg;
        }

    }

    public enum LoggerType
    {
        UNITY,
        DEDICATED
    }
}