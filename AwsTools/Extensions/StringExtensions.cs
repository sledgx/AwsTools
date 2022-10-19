using System.Text;

namespace SledGX.Tools.AWS.Extensions
{
    /// <summary>
    /// String object extensions
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Unzip a compressed Base64 string into plain text.
        /// </summary>
        /// <param name="text">The compressed Base64 string.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>The decompressed string.</returns>
        public static string FromZippedBase64(this string text, string encoder = "UTF-8")
        {
            Encoding encoding = Encoding.GetEncoding(encoder);
            byte[] bytes = Convert.FromBase64String(text);
            byte[] unzipBytes = bytes.FromZip();
            return encoding.GetString(unzipBytes);
        }

        /// <summary>
        /// Unzip a plain text into compressed Base64 string.
        /// </summary>
        /// <param name="text">The plain text.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>The compressed Base64 string.</returns>
        public static string ToZippedBase64(this string text, string encoder = "UTF-8")
        {
            Encoding encoding = Encoding.GetEncoding(encoder);
            byte[] bytes = encoding.GetBytes(text);
            byte[] zipBytes = bytes.ToZip();
            return Convert.ToBase64String(zipBytes);
        }
    }
}
