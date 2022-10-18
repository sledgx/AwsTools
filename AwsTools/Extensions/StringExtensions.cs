using System.Text;

namespace AwsTools.Extensions
{
    internal static class StringExtensions
    {
        public static string FromZippedBase64(this string text, string encoder = "UTF-8")
        {
            Encoding encoding = Encoding.GetEncoding(encoder);
            byte[] bytes = Convert.FromBase64String(text);
            byte[] unzipBytes = bytes.FromZip();
            return encoding.GetString(unzipBytes);
        }

        public static string ToZippedBase64(this string text, string encoder = "UTF-8")
        {
            Encoding encoding = Encoding.GetEncoding(encoder);
            byte[] bytes = encoding.GetBytes(text);
            byte[] zipBytes = bytes.ToZip();
            return Convert.ToBase64String(zipBytes);
        }
    }
}
