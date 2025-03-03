using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RPGCreator.core
{
    static class Logger
    {
        public enum LogLevel
        {
            info,
            normal,
            warning,
            error,
            success,
            debug
        }

        public static void Log(string from, string message, LogLevel level = LogLevel.normal)
        {
            
        }

        public static void Info(string from, string message)
        {
            Log(from, message, LogLevel.info);
        }

        public static void Success(string from, string message)
        {
            Log(from, message, LogLevel.success);
        }

        public static void Error(string from, string message)
        {
            Log(from, message, LogLevel.error);
        }

        public static void Debug(string from, string message)
        {
            Log(from, message, LogLevel.debug);
        }

        public static void Warning(string from, string message)
        {
            Log(from, message, LogLevel.warning);
        }
    }
}
