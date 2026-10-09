using System;

namespace HP.GFriend.GFLogger
{
    public static class Logger
    {
        /// <summary>
        /// Occurs when a Trace event is logged.
        /// Raised for individual network calls to devices.
        /// </summary>
        public static event EventHandler<LogEventArgs> OnTrace;

        /// <summary>
        /// Occurs when a Debug event is logged.
        /// Raised for higher-level calls describing an action
        /// that is being performed on a device.
        /// </summary>
        public static event EventHandler<LogEventArgs> OnDebug;

        /// <summary>
        /// Occurs when a Warn event is logged.
        /// Raised for device interaction that was not directly initiated
        /// by the consumer, e.g. reestablishing a device connection that was lost.
        /// </summary>
        public static event EventHandler<LogEventArgs> OnWarn;

        /// <summary>
        /// Occurs when an Error event is logged.
        /// Raised when attempted device interaction fails, typically
        /// because the device responded with an error or did not respond at all.
        /// </summary>
        public static event EventHandler<LogEventArgs> OnError;

        /// <summary>
        /// Logs a trace message.  Should be used for individual network calls to devices.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Trace(object message)
        {
            OnTrace?.Invoke(null, new LogEventArgs(message));
        }

        /// <summary>
        /// Logs a debug message.  Should be used for higher-level calls
        /// describing an action that is being performed on a device.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(object message)
        {
            OnDebug?.Invoke(null, new LogEventArgs(message));
        }

        /// <summary>
        /// Logs a warning message.  Should be used for device interaction that
        /// was not directly initiated by the consumer, e.g. reestablishing a lost connection.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Warn(object message)
        {
            OnWarn?.Invoke(null, new LogEventArgs(message));
        }

        /// <summary>
        /// Logs an error message.  Should be used for failed attempts to interact with a device.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Error(object message)
        {
            OnError?.Invoke(null, new LogEventArgs(message));
        }

        /// <summary>
        /// Logs an error message.  Should be used for failed attempts to interact with a device.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log with the message.</param>
        public static void Error(object message, Exception exception)
        {
            OnError?.Invoke(null, new LogEventArgs(message, exception));
        }
    }
}
