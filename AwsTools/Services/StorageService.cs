using Amazon.S3;
using Amazon.S3.Model;
using SledGX.Tools.AWS.Extensions;
using SledGX.Tools.AWS.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SledGX.Tools.AWS
{
    /// <summary>
    /// Storage compression format
    /// </summary>
    public enum StorageFormat
    {
        /// <summary>
        /// Storing objects as a plain text string.
        /// </summary>
        PLAIN,
        /// <summary>
        /// Storing objects as a Base64 string.
        /// </summary>
        BASE64,
        /// <summary>
        /// Storing objects as zipped data.
        /// </summary>
        GZIP,
        /// <summary>
        /// Storing objects as a Base64 zipped string.
        /// </summary>
        GZIP_BASE64,
        /// <summary>
        /// Storing objects as deflated data.
        /// </summary>
        DEFLATE,
        /// <summary>
        /// Storing objects as a Base64 deflated data.
        /// </summary>
        DEFLATE_BASE64
    }

    /// <summary>
    /// Wrapper for accessing Amazon Simple Storage Service (S3).
    /// </summary>
    public class StorageService
    {
        private readonly string bucketName;
        private readonly AmazonS3Client client;

        /// <summary>
        /// Storage service initialization.
        /// </summary>
        /// <param name="setting">Storage service setting.</param>
        public StorageService(StorageSetting setting)
        {
            bucketName = setting.BucketName;
            client = new AmazonS3Client(setting.GetCredentials(), setting.GetRegionEndpoint());
        }

        /// <summary>
        /// Checks if an object is present inside the Amazon S3 bucket.
        /// </summary>
        /// <param name="key">The path to the object.</param>
        /// <returns>True if the object exists, false otherwise.</returns>
        public bool ObjectExists(string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                var response = client.GetObjectMetadataAsync(request).Result;
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is not null and AmazonS3Exception)
                {
                    AmazonS3Exception aex = (AmazonS3Exception)ex.InnerException;

                    if (aex.StatusCode == HttpStatusCode.NotFound)
                        return false;
                }

                throw;
            }
        }

        /// <summary>
        /// Retrieve the list of objects inside the Amazon S3 bucket.
        /// </summary>
        /// <param name="prefixFilter">Limits the response to keys that begin with the specified prefix.</param>
        /// <returns></returns>
        public List<ObjectInfo> GetObjectList(string? prefixFilter = null)
        {
            var items = new List<ObjectInfo>();

            ListObjectsResponse response;
            do
            {
                var request = new ListObjectsRequest
                {
                    BucketName = bucketName,
                    Prefix = prefixFilter
                };
                response = client.ListObjectsAsync(request).Result;

                foreach (S3Object obj in response.S3Objects)
                {
                    items.Add(new ObjectInfo
                    {
                        Key = obj.Key,
                        Size = obj.Size,
                        LastModified = obj.LastModified,
                        StorageClass = obj.StorageClass
                    });
                }

                request.Marker = response.NextMarker;
            }
            while (response.IsTruncated);

            return items;
        }

        /// <summary>
        /// Writes a text into an Amazon S3 bucket.
        /// </summary>
        /// <param name="key">The path to the object.</param>
        /// <param name="text">The text to load.</param>
        /// <param name="format">The format of the content encoding.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool UploadText(string key, string text, StorageFormat format = StorageFormat.PLAIN, string encoder = "UTF-8")
        {
            var encoding = Encoding.GetEncoding(encoder);
            byte[] bytes = encoding.GetBytes(text);

            if (format == StorageFormat.GZIP || format == StorageFormat.GZIP_BASE64)
                bytes = bytes.Zip();
            else if (format == StorageFormat.DEFLATE || format == StorageFormat.DEFLATE_BASE64)
                bytes = bytes.Deflate();

            if (format == StorageFormat.BASE64 || format == StorageFormat.GZIP_BASE64 || format == StorageFormat.DEFLATE_BASE64)
            {
                var base64 = Convert.ToBase64String(bytes);
                bytes = encoding.GetBytes(base64);
            }

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = new MemoryStream(bytes)
            };

            var response = client.PutObjectAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Writes an object into an Amazon S3 bucket. The object is automatically transformed into json.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="key">The path to the object.</param>
        /// <param name="data">The object to load.</param>
        /// <param name="format">The format of the content encoding.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool UploadObject<T>(string key, T data, StorageFormat format = StorageFormat.PLAIN, string encoder = "UTF-8")
        {
            string json = JsonSerializer.Serialize(data);
            return UploadText(key, json, format, encoder);
        }

        /// <summary>
        /// Writes a file to an Amazon S3 bucket.
        /// </summary>
        /// <param name="key">The path to the object.</param>
        /// <param name="filePath">The path of the file to upload.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool UploadFile(string key, string filePath)
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                FilePath = filePath
            };

            var response = client.PutObjectAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Retrieves a text from Amazon S3 bucket.
        /// </summary>
        /// <param name="key">The path to the object.</param>
        /// <param name="format">The format of the content encoding.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>The downloaded text.</returns>
        public string? GetText(string key, StorageFormat format = StorageFormat.PLAIN, string encoder = "UTF-8")
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = client.GetObjectAsync(request).Result;

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return null;

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                response.ResponseStream.CopyTo(stream);
                bytes = stream.ToArray();
            }

            var encoding = Encoding.GetEncoding(encoder);

            if (format == StorageFormat.BASE64 || format == StorageFormat.GZIP_BASE64 || format == StorageFormat.DEFLATE_BASE64)
            {
                var base64 = encoding.GetString(bytes);
                bytes = Convert.FromBase64String(base64);
            }

            if (format == StorageFormat.GZIP || format == StorageFormat.GZIP_BASE64)
                bytes = bytes.Unzip();
            else if (format == StorageFormat.DEFLATE || format == StorageFormat.DEFLATE_BASE64)
                bytes = bytes.Flate();

            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Retrieves an object from Amazon S3 bucket.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="key">The path to the object.</param>
        /// <param name="format">The format of the content encoding.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>The downloaded object.</returns>
        public T? GetObject<T>(string key, StorageFormat format = StorageFormat.PLAIN, string encoder = "UTF-8")
        {
            var json = GetText(key, format, encoder);

            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Downloads the file from Amazon S3 bucket.
        /// </summary>
        /// <param name="key">The path to the object.</param>
        /// <param name="filePath">The path to the local file to create.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool DownloadFile(string key, string filePath)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = client.GetObjectAsync(request).Result;

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return false;

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                response.ResponseStream.CopyTo(stream);
            }

            return true;
        }

        /// <summary>
        /// Deletes an object from the Amazon S3 bucket.
        /// </summary>
        /// <param name="key">The path of the object to deleted.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool DeleteObject(string key)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = client.DeleteObjectAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }
    }

    /// <summary>
    /// Storage Object Information.
    /// </summary>
    public class ObjectInfo
    {
        /// <summary>
        /// Object key.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Object size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Object last modified date.
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// Object storage class.
        /// </summary>
        public string? StorageClass { get; set; }
    }
}
