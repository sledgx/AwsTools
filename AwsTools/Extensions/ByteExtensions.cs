using System.IO.Compression;

namespace SledGX.Tools.AWS.Extensions
{
    /// <summary>
    /// Byte object extensions
    /// </summary>
    internal static class ByteExtensions
    {
        /// <summary>
        /// Decompresses an array of bytes
        /// </summary>
        /// <param name="bytes">The byte array to decompress.</param>
        /// <returns>The decompressed byte array.</returns>
        public static byte[] FromZip(this byte[] bytes)
        {
            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            return mso.ToArray();
        }

        /// <summary>
        /// Compresses an array of bytes
        /// </summary>
        /// <param name="bytes">The byte array to compress.</param>
        /// <returns>The compressed byte array.</returns>
        public static byte[] ToZip(this byte[] bytes)
        {
            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }

            return mso.ToArray();
        }
    }
}
