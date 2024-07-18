using System;
using UnityEngine;

namespace Singular
{
    class SingularUnityLogger
    {
        // based on android Logger.java log levels enum: android.util.Log
        public enum LogLevel
        {
            Verbose = 2,
            Debug   = 3,
            Info    = 4,
            Warn    = 5,
            Error   = 6,
            Assert  = 7
        }
        
        private static bool _enableLogging;
        private static LogLevel _logLevel;
        private const string LogTag = "[SingularLog]";

        public static void SetLogLevel(int level)
        {
            if (Enum.IsDefined(typeof(LogLevel), level))
            {
                _logLevel = (LogLevel)level;
            }
            else
            {
                Debug.Log("invalid log level value - fallback to level = Debug.");
                _logLevel = LogLevel.Debug;
            }
        }

        public static void EnableLogging(bool enable)
        {
            _enableLogging = enable;
        }

        #region Logging methods
        
        public static void LogVerbose(string message)
        {
            TryLog(message, LogLevel.Verbose, Debug.Log);
        }

        public static void LogDebug(string message)
        {
            TryLog(message, LogLevel.Debug, Debug.Log);
        }

        public static void LogInfo(string message)
        {
            TryLog(message, LogLevel.Info, Debug.Log);
        }

        public static void LogWarn(string message)
        {
            TryLog(message, LogLevel.Warn, Debug.LogWarning);
        }

        public static void LogError(string message)
        {
            TryLog(message, LogLevel.Error, Debug.LogError);
        }

        public static void LogAssert(string message)
        {
            TryLog(message, LogLevel.Assert, Debug.LogError); // uses LogError also for Assert levels.
        }

        #endregion // end region Logging methods
        
        // private method does conditional logging
        private static void TryLog(string message, LogLevel level, Action<string> logAction)
        {
            try
            {
                if (_enableLogging && _logLevel <= level)
                {
                    logAction(LogTag + ": " + message);
                }
            }
            catch (Exception)
            {
                // ignored...
            }
        }
    }
}