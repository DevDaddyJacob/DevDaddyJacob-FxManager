using System.IO;

namespace DevDaddyJacob.FxShared.Logger
{

    internal static class OldAppLogger
    {
        //private class InternalFileLogger : FileLogger
        //{
        //    public InternalFileLogger(string logDir) : base(logDir, false, true)
        //    {
        //    }

        //    public void WriteLine(string line) => Write(line);
        //}

        //private static InternalFileLogger? _FileLogger;

        //private static InternalFileLogger FileLogger
        //{
        //    get
        //    {
        //        if (_FileLogger == null)
        //        {
        //            _FileLogger = new InternalFileLogger(Path.Combine(Directory.GetCurrentDirectory(), "logs\\"));
        //        }

        //        return _FileLogger;
        //    }
        //}

        private const bool IsDebug = 
#if DEBUG
                true;
#else
                false;
#endif

        private static void InternalLog(string level, string data)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss UTCz");
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] {level.ToUpper()}: {data}");
#else
            Console.WriteLine($"[{timestamp}] {level.ToUpper()}: {data}");
#endif

            //FileLogger.WriteLine($"{level.ToUpper()}: {data}");
        }

        #region Debug Logs
        public static void Debug()
        {
            if (IsDebug)
            {
                InternalLog("DEBUG", "");
            }
        }

        public static void Debug(string message)
        {
            if (IsDebug)
            {
                InternalLog("DEBUG", message);
            }
        }

        public static void Debug(object value)
        {
            if (IsDebug)
            {
                InternalLog("DEBUG", value.ToString());
            }
        }

        public static void Debug(string format, params object[] args)
        {
            if (IsDebug)
            {
                InternalLog("DEBUG", string.Format(format, args));
            }
        }
        #endregion

        #region Log Logs
        public static void Log()
            => InternalLog("LOG", "");

        public static void Log(string message)
            => InternalLog("LOG", message);

        public static void Log(object value)
            => InternalLog("LOG", value.ToString());

        public static void Log(string format, params object[] args)
            => InternalLog("LOG", string.Format(format, args));
        #endregion

        #region Info Logs
        public static void Info()
            => InternalLog("INFO", "");

        public static void Info(string message)
            => InternalLog("INFO", message);

        public static void Info(object value)
            => InternalLog("INFO", value.ToString());

        public static void Info(string format, params object[] args)
            => InternalLog("INFO", string.Format(format, args));
        #endregion

        #region Warn Logs
        public static void Warn()
            => InternalLog("WARN", "");

        public static void Warn(string message)
            => InternalLog("WARN", message);

        public static void Warn(object value)
            => InternalLog("WARN", value.ToString());

        public static void Warn(string format, params object[] args)
            => InternalLog("WARN", string.Format(format, args));
        #endregion

        #region Error Logs
        public static void Error()
            => InternalLog("ERROR", "");

        public static void Error(string message)
            => InternalLog("ERROR", message);

        public static void Error(object value)
            => InternalLog("ERROR", value.ToString());

        public static void Error(Exception exception)
            => InternalLog("ERROR", $"Exception: {exception.Message}\n{exception.StackTrace}");

        public static void Error(string message, Exception exception)
            => InternalLog("ERROR", $"{message} Exception: {exception.Message}\n{exception.StackTrace}");

        public static void Error(string format, params object[] args)
            => InternalLog("ERROR", string.Format(format, args));
        #endregion

    }
}
