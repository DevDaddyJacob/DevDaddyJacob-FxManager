using System.Diagnostics;
using System.Reflection.Emit;

namespace DevDaddyJacob.FxManager.Shared.Logger
{
    internal abstract class BaseLogger
    {
        public abstract bool CanLog(LogLevel level);

        public abstract void Log(LogLevel level);
        public abstract void Log(LogLevel level, string message);
        public abstract void Log(LogLevel level, object value);
        public abstract void Log(LogLevel level, Exception exception);
        public abstract void Log(LogLevel level, string message, Exception exception);
        public abstract void Log(LogLevel level, string format, params object[] args);

        public virtual string GetNameForLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Emergency => "EMERGENCY",
                LogLevel.Alert => "ALERT",
                LogLevel.Critical => "CRITICAL",
                LogLevel.Error => "ERROR",
                LogLevel.Warning => "WARNING",
                LogLevel.Notice => "NOTICE",
                LogLevel.Informational => "INFO",
                LogLevel.Debug => "DEBUG",
                _ => "UNKNWON"
            };
        }

        #region Emergency Log Level

        public virtual void Emergency()
            => Log(LogLevel.Emergency);

        public virtual void Emergency(string message)
            => Log(LogLevel.Emergency, message);

        public virtual void Emergency(object value)
            => Log(LogLevel.Emergency, value);

        public virtual void Emergency(Exception exception)
            => Log(LogLevel.Emergency, exception);

        public virtual void Emergency(string message, Exception exception)
            => Log(LogLevel.Emergency, message, exception);

        public virtual void Emergency(string format, params object[] args)
            => Log(LogLevel.Emergency, format, args);

        #endregion Emergency Log Level

        #region Alert Log Level

        public virtual void Alert()
            => Log(LogLevel.Alert);

        public virtual void Alert(string message)
            => Log(LogLevel.Alert, message);

        public virtual void Alert(object value)
            => Log(LogLevel.Alert, value);

        public virtual void Alert(Exception exception)
            => Log(LogLevel.Alert, exception);

        public virtual void Alert(string message, Exception exception)
            => Log(LogLevel.Alert, message, exception);

        public virtual void Alert(string format, params object[] args)
            => Log(LogLevel.Alert, format, args);

        #endregion Alert Log Level

        #region Critical Log Level

        public virtual void Critical()
            => Log(LogLevel.Critical);

        public virtual void Critical(string message)
            => Log(LogLevel.Critical, message);

        public virtual void Critical(object value)
            => Log(LogLevel.Critical, value);

        public virtual void Critical(Exception exception)
            => Log(LogLevel.Critical, exception);

        public virtual void Critical(string message, Exception exception)
            => Log(LogLevel.Critical, message, exception);

        public virtual void Critical(string format, params object[] args)
            => Log(LogLevel.Critical, format, args);

        #endregion Critical Log Level

        #region Error Log Level

        public virtual void Error()
            => Log(LogLevel.Error);

        public virtual void Error(string message)
            => Log(LogLevel.Error, message);

        public virtual void Error(object value)
            => Log(LogLevel.Error, value);

        public virtual void Error(Exception exception)
            => Log(LogLevel.Error, exception);

        public virtual void Error(string message, Exception exception)
            => Log(LogLevel.Error, message, exception);

        public virtual void Error(string format, params object[] args)
            => Log(LogLevel.Error, format, args);

        #endregion Error Log Level

        #region Warning Log Level

        public virtual void Warning()
            => Log(LogLevel.Warning);

        public virtual void Warning(string message)
            => Log(LogLevel.Warning, message);

        public virtual void Warning(object value)
            => Log(LogLevel.Warning, value);

        public virtual void Warning(Exception exception)
            => Log(LogLevel.Warning, exception);

        public virtual void Warning(string message, Exception exception)
            => Log(LogLevel.Warning, message, exception);

        public virtual void Warning(string format, params object[] args)
            => Log(LogLevel.Warning, format, args);

        #endregion Warning Log Level

        #region Notice Log Level

        public virtual void Notice()
            => Log(LogLevel.Notice);

        public virtual void Notice(string message)
            => Log(LogLevel.Notice, message);

        public virtual void Notice(object value)
            => Log(LogLevel.Notice, value);

        public virtual void Notice(Exception exception)
            => Log(LogLevel.Notice, exception);

        public virtual void Notice(string message, Exception exception)
            => Log(LogLevel.Notice, message, exception);

        public virtual void Notice(string format, params object[] args)
            => Log(LogLevel.Notice, format, args);

        #endregion Notice Log Level

        #region Info Log Level

        public virtual void Info()
            => Log(LogLevel.Informational);

        public virtual void Info(string message)
            => Log(LogLevel.Informational, message);

        public virtual void Info(object value)
            => Log(LogLevel.Informational, value);

        public virtual void Info(Exception exception)
            => Log(LogLevel.Informational, exception);

        public virtual void Info(string message, Exception exception)
            => Log(LogLevel.Informational, message, exception);

        public virtual void Info(string format, params object[] args)
            => Log(LogLevel.Informational, format, args);

        #endregion Info Log Level

        #region Debug Log Level

        public virtual void Debug()
            => Log(LogLevel.Debug);

        public virtual void Debug(string message)
            => Log(LogLevel.Debug, message);

        public virtual void Debug(object value)
            => Log(LogLevel.Debug, value);

        public virtual void Debug(Exception exception)
            => Log(LogLevel.Debug, exception);

        public virtual void Debug(string message, Exception exception)
            => Log(LogLevel.Debug, message, exception);

        public virtual void Debug(string format, params object[] args)
            => Log(LogLevel.Debug, format, args);

        #endregion Debug Log Level
    }
}