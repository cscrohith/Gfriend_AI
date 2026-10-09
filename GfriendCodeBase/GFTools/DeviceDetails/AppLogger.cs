using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Tool
{
    public static class AppLogger
    {
        public static void Debug(string message)
        {
            // Existing HP logger
            HP.GFriend.GFLogger.Logger.Debug(message);

            // Your file logger
            FileLogger.Debug(message);
        }
        public static void Error(string message)
        {
            FileLogger.Error(message);
        }

        public static void Error(string message, Exception ex)
        {
            HP.GFriend.GFLogger.Logger.Error(message, ex);
            FileLogger.Error(message, ex);
        }
    }
}
