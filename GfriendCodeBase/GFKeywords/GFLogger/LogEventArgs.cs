using System;

namespace HP.GFriend.GFLogger
{
    public sealed class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the object representing the log message.
        /// </summary>
        public object Message { get; }

        /// <summary>
        /// Gets the <see cref="System.Exception" /> associated with this log event, if any.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventArgs" /> class.
        /// </summary>
        /// <param name="message">The object representing the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is null.</exception>
        public LogEventArgs(object message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Message = message;
            Exception = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventArgs" /> class.
        /// </summary>
        /// <param name="message">The object representing the message.</param>
        /// <param name="exception">The <see cref="System.Exception" /> associated with this log event.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is null.</exception>
        public LogEventArgs(object message, Exception exception)
            : this(message)
        {
            Exception = exception;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this <see cref="LogEventArgs" />.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this <see cref="LogEventArgs" />.</returns>
        public override string ToString() => Message.ToString();
    }
}
