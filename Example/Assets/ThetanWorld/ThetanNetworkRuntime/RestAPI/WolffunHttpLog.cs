using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wolffun.RestAPI
{
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        All = 255,
    }
    
    public class WolffunHttpLog
    {
        private LogLevel _logLevel;

        public LogLevel LogLevel => _logLevel;
        
        public WolffunHttpLog(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public bool CheckCanShowLog(LogLevel logLevel)
        {
            return logLevel <= _logLevel;
        }

        public void Log(LogLevel logLevel, string message)
        {
            if (logLevel > _logLevel)
                return;

            switch (logLevel)
            {
                case LogLevel.Error:
                    Debug.LogError(message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Info:
                    Debug.Log(message);
                    break;
            }
        }
    }
}
