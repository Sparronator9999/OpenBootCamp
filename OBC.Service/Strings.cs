using System.Globalization;
using System.Resources;

namespace OBC.Service
{
    /// <summary>
    /// A resource class for retrieving strings.
    /// </summary>
    internal static class Strings
    {
        private static ResourceManager resMan;

        /// <summary>
        /// Gets a string from the underlying resource file, and replaces format
        /// items with the specified object's string representation.
        /// </summary>
        /// <remarks>
        /// This function internally calls
        /// <see cref="ResourceManager.GetString(string)"/> to retrieve the string.
        /// </remarks>
        /// <param name="name">
        /// The name of the string to find.
        /// </param>
        /// <param name="args">
        /// The objects to format the string with.
        /// </param>
        /// <returns>
        /// <para>The formatted string corresponding to the specified string name, if found.</para>
        /// <para><see langword="null"/> if the string couldn't be found.</para>
        /// </returns>
        public static string GetString(string name, params object[] args)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            resMan ??= new ResourceManager(typeof(Strings));

            string temp = resMan.GetString(name, ci);
            return temp is null
                ? null
                : string.Format(ci, temp, args);
        }
    }
}
