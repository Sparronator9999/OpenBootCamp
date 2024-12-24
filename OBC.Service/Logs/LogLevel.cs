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
        None = 0,

        /// <summary>
        /// Only log Fatal events.
        /// </summary>
        Fatal = 1,

        /// <summary>
        /// Log Errors and Fatal events.
        /// </summary>
        Error = 2,

        /// <summary>
        /// Log Warnings, Errors, and Fatal events.
        /// </summary>
        Warn = 3,

        /// <summary>
        /// Log all events, except for Debug events.
        /// </summary>
        Info = 4,

        /// <summary>
        /// Log all events.
        /// </summary>
        Debug = 5,
    }
}
