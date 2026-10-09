using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System;
using System.IO;
using System.Linq;

namespace HP.GFriend.GFLogger
{

    public sealed class GFriendTraceLogger
    {
        private readonly ILog _logger;
        private readonly Type _callerStackBoundaryType;

        private string _outputDir;
        
        /// <summary>
        /// Gets the default logger name used if no other logger name is specified.
        /// </summary>
        public static string DefaultLoggerName { get; } = "HP.GFriend";

        /// <summary>
        /// Sets a global property that can be referenced in log4net configuration.
        /// This property is shared by all threads in the current AppDomain.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        public static void SetGlobalProperty(string name, string value)
        {
            GlobalContext.Properties[name] = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GFriendTraceLogger" /> class.
        /// </summary>
        /// <param name="callerStackBoundary">The type that will be used as the boundary for the call stack.</param>
        /// <param name="loggerName">The logger name to look for in the configuration data.</param>
        public GFriendTraceLogger(Type callerStackBoundary, string loggerName, string outputDir, string logfileName = null)
        {
            _callerStackBoundaryType = callerStackBoundary;
            _outputDir = outputDir;

            //ILoggerRepository loggerRepository = LogManager.GetRepository();
            ILoggerRepository loggerRepository;
            if(LogManager.GetAllRepositories().Where(r => r.Name.Equals("GFriend")).FirstOrDefault() != null)
            {
                loggerRepository = LogManager.GetRepository("GFriend");
            }
            else
            {
                loggerRepository = LogManager.CreateRepository("GFriend");
            }
            
            ConfigureDefault(loggerRepository, loggerName, _outputDir, logfileName);
            _logger = LogManager.GetLogger("GFriend", loggerName);
            XmlConfigurator.Configure();
        }

        private static void ConfigureDefault(ILoggerRepository loggerRepository, string loggerName, string outputDir, string logfileName = null)
        {
            PatternLayout layout = new PatternLayout("%date{yyyy-MM-dd HH:mm:ss.fff} [%property{charlevel}] %class{1}::%method::%message%newline");
            layout.ActivateOptions();
            if (string.IsNullOrEmpty(logfileName))
            {
                logfileName = "log.txt";
            }
            RollingFileAppender fileAppender = new RollingFileAppender
            {
                Name = loggerName,
                Encoding = System.Text.Encoding.UTF8,
                File = $"{outputDir}/{logfileName}",
                StaticLogFileName = true,
                AppendToFile = false,
                PreserveLogFileNameExtension = true,
                MaxSizeRollBackups = -1,
                MaximumFileSize = "2MB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                Layout = layout
            };
            fileAppender.ActivateOptions();

            Hierarchy hierarchy = (Hierarchy)loggerRepository;
            
            
            hierarchy.Root.AddAppender(fileAppender);      
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
            loggerRepository.Configured = true;

            
        }
        
        /// <summary>
        /// Logs a Trace message.
        /// Used for fine-grained messages typically only logged during development.
        /// </summary>
        /// <param name="message">The object representing the message to log.</param>
        public void LogTrace(object message)
        {
            Log(Level.Trace, message);
        }

        /// <summary>
        /// Logs a Debug message.
        /// Used for general-purpose messages to show program flow.
        /// </summary>
        /// <param name="message">The object representing the message to log.</param>
        public void LogDebug(object message)
        {
            Log(Level.Debug, message);
        }

        /// <summary>
        /// Logs a Warn message.
        /// Used for conditions that might be a problem but will not prevent the current operation from continuing.
        /// </summary>
        /// <param name="message">The object representing the message to log.</param>
        public void LogWarn(object message)
        {
            Log(Level.Warn, message);
        }

        /// <summary>
        /// Logs a Warn message with an exception.
        /// Used for conditions that might be a problem but will not prevent the current operation from continuing.
        /// </summary>
        /// <param name="message">The object representing the message to log.</param>
        /// <param name="ex">The exception to log with the message.</param>
        public void LogWarn(object message, Exception ex)
        {
            Log(Level.Warn, message, ex);
        }

        /// <summary>
        /// Logs an Error message.
        /// Used for conditions that prevent an operation from succeeding/continuing.
        /// </summary>
        /// <param name="message">The object representing the message to log.</param>
        public void LogError(object message)
        {
            Log(Level.Error, message);
        }

        /// <summary>
        /// Logs an Error message with an exception.
        /// Used for conditions that prevent an operation from succeeding/continuing.
        /// </summary>
        /// <param name="message">The object representing the message to log.</param>
        /// <param name="ex">The exception to log with the message.</param>
        public void LogError(object message, Exception ex)
        {
            Log(Level.Error, message, ex);
        }

        private void Log(Level level, object message, Exception ex = null)
        {
            LoggingEvent loggingEvent = new LoggingEvent(_callerStackBoundaryType, _logger.Logger.Repository, _logger.Logger.Name, level, message, ex);
            loggingEvent.Properties["charlevel"] = level.ToString()[0];
            _logger.Logger.Log(loggingEvent);           
        }

        public void Dispose()
        {
            LogManager.ShutdownRepository("GFriend");
        }
    }
}
