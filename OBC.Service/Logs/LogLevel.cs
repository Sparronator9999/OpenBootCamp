namespace OBC.Service.Logs
{
    /// <summary>
    /// The verbosity of logs
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Do not log anything.
        /// </summary>
        NONE,

        /// <summary>
        /// Only log Fatal events.
        /// </summary>
        FATAL,

        /// <summary>
        /// Log Errors and Fatal events.
        /// </summary>
        ERROR,

        /// <summary>
        /// Log Warnings, Errors, and Fatal events.
        /// </summary>
        WARN,

        /// <summary>
        /// Log all events, except for Debug events.
        /// </summary>
        INFO,

        /// <summary>
        /// Log all events.
        /// </summary>
        DEBUG,
    }
}
