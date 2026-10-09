using System;

namespace HP.GFriend.GFLogger
{
    public static class GFriendLoggerServices
    {
        
        public static GFriendTraceLogger GFLogger { get; set; }
        private static bool _attached = false;

        public static void InitLogger(string outputDir, string logfileName = null)
        {
            if (string.IsNullOrEmpty(outputDir))
            {
                throw new ArgumentNullException("Output directory should not be null or empty");
            }

            GFLogger = new GFriendTraceLogger(typeof(GFriendTraceLogger), "HP.GFriend", outputDir, logfileName);

            if(!_attached)
            {
                Logger.OnTrace += (s, e) => GFLogger.LogTrace(e.Message);
                Logger.OnDebug += (s, e) => GFLogger.LogDebug(e.Message);
                Logger.OnWarn += (s, e) => GFLogger.LogWarn(e.Message);
                Logger.OnError += (s, e) => GFLogger.LogError(e.Message, e.Exception);
                _attached = true;
            }
            
        }

        public static void Dispose()
        {
            if(GFLogger != null)
            {
                GFLogger.Dispose();
            }
        }
    }
}
