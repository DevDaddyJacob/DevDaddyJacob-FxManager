namespace DevDaddyJacob.FxManager.Shared.Logger
{
    internal enum LogLevel : int
    {
        /// <summary>
        /// indicates that the system is unusable and requires immediate attention.
        /// </summary>
        Emergency = 1,

        /// <summary>
        /// indicates that immediate action is necessary to resolve a critical issue.
        /// </summary>
        Alert = 2,

        /// <summary>
        /// signifies critical conditions in the program that demand intervention to prevent system failure.
        /// </summary>
        Critical = 3,

        /// <summary>
        /// indicates error conditions that impair some operation but are less severe than critical situations.
        /// </summary>
        Error = 4,

        /// <summary>
        /// signifies potential issues that may lead to errors or unexpected behavior in the future if not addressed.
        /// </summary>
        Warning = 5,

        /// <summary>
        /// applies to normal but significant conditions that may require monitoring.
        /// </summary>
        Notice = 6,

        /// <summary>
        /// includes messages that provide a record of the normal operation of the system.
        /// </summary>
        Informational = 7,

        /// <summary>
        /// intended for logging detailed information about the system for debugging purposes.
        /// </summary>
        Debug = 8,
    }
}