using System;
using System.Runtime.Serialization;

namespace OpenBootCamp.Config
{
    /// <summary>
    /// The exception thrown when an invalid <see cref="YAMDCC_Config"/> is loaded.
    /// </summary>
    public sealed class InvalidConfigException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigException" /> class.
        /// </summary>
        public InvalidConfigException()
            : base("The config was not in an expected format.") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigException"/>
        /// class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidConfigException(string message)
            : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigException"/> class
        /// with a specified error message and a reference to the inner exception that
        /// is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null
        /// reference (<see langword="Nothing"/> in Visual Basic) if no inner
        /// exception is specified.
        /// </param>
        public InvalidConfigException(string message, Exception innerException)
            : base(message, innerException) { }

        private InvalidConfigException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}

