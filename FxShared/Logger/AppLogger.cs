namespace DevDaddyJacob.FxManager.Shared.Logger
{
    internal class AppLogger : BaseLogger
    {
        private TextWriter _writer;
        private LogLevel _maxLevel;

        public AppLogger(TextWriter writer, LogLevel maxLevel)
        {
            _writer = writer;
            _maxLevel = maxLevel;
        }

        public AppLogger(TextWriter writer) : this(writer, LogLevel.Informational) { }

        public AppLogger(LogLevel maxLevel) : this(Console.Out, maxLevel) { }

        public AppLogger() : this(Console.Out, LogLevel.Informational) { }

        private void WriteLine(string message)
        {
            _writer.WriteLine(message);
        }

        public override bool CanLog(LogLevel level)
        {
            return level <= _maxLevel;
        }

        public override void Log(LogLevel level)
        {
            if (!CanLog(level)) { return; }
            WriteLine($"[{GetNameForLevel(level)}]");
        }

        public override void Log(LogLevel level, string message)
        {
            if (!CanLog(level)) { return; }
            WriteLine($"[{GetNameForLevel(level)}] {message}");
        }

        public override void Log(LogLevel level, object value)
        {
            if (!CanLog(level)) { return; }
            WriteLine($"[{GetNameForLevel(level)} {value.ToString()}]");
        }

        public override void Log(LogLevel level, Exception exception)
        {
            if (!CanLog(level)) { return; }
            WriteLine($"[{GetNameForLevel(level)}] Exception: {exception.Message}\n{exception.StackTrace}");
        }

        public override void Log(LogLevel level, string message, Exception exception)
        {
            if (!CanLog(level)) { return; }
            WriteLine($"[{GetNameForLevel(level)}] {message} Exception: {exception.Message}\n{exception.StackTrace}");
        }

        public override void Log(LogLevel level, string format, params object[] args)
        {
            if (!CanLog(level)) { return; }
            WriteLine($"[{GetNameForLevel(level)}] {string.Format(format, args)}");
        }
    }
}