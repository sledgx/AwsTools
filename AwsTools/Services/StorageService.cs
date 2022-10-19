using Amazon.S3;
using Amazon.S3.Model;
using AwsTools.Extensions;
using AwsTools.Models;
using System.Net;
using System.Text.Json;

namespace AwsTools.Services
{
    /// <summary>
    /// Storage compression format
    /// </summary>
    public enum StorageFormat
    {
        PLAIN,
        GZIP,
        AUTO
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
        /// Writes a text into an Amazon S3 bucket.
        /// </summary>
        /// <param name="key">The path to the object.</param>
        /// <param name="text">The text to load.</param>
        /// <param name="format">The format of the content encoding; PLAIN for plain text, GZIP for compressed text or AUTO for automatic choice between the previous two.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool UploadText(string key, string text, StorageFormat format = StorageFormat.PLAIN, string encoder = "UTF-8")
        {
            string data = text;
            if (format == StorageFormat.GZIP || format == StorageFormat.AUTO)
            {
                data = text.ToZippedBase64(encoder);

                if (format == StorageFormat.AUTO && data.Length > text.Length)
                    data = text;
            }

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentBody = data
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
        /// <param name="format">The format of the content encoding; PLAIN for plain text, GZIP for compressed text or AUTO for automatic choice between the previous two.</param>
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
        /// <param name="tryUnzip">Tries to automatically decompress the contents of the object.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>The downloaded text.</returns>
        public string? GetText(string key, bool tryUnzip = false, string encoder = "UTF-8")
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = client.GetObjectAsync(request).Result;

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return null;

            using var reader = new StreamReader(response.ResponseStream);
            var text = reader.ReadToEnd();

            if (tryUnzip)
            {
                try { text = text.FromZippedBase64(encoder); }
                catch { }
            }

            return text;
        }

        /// <summary>
        /// Retrieves an object from Amazon S3 bucket.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="key">The path to the object.</param>
        /// <param name="tryUnzip">Tries to automatically decompress the contents of the object.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>The downloaded object.</returns>
        public T? GetObject<T>(string key, bool tryUnzip = false, string encoder = "UTF-8")
        {
            var json = GetText(key, tryUnzip, encoder);

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
}
