using Amazon.S3;
using Amazon.S3.Model;
using AwsTools.Extensions;
using AwsTools.Models;
using System.Net;
using System.Text.Json;

namespace AwsTools.Services
{
    public enum StorageFormat
    {
        PLAN,
        GZIP,
        AUTO
    }

    public class StorageService
    {
        private readonly string bucketName;
        private readonly AmazonS3Client client;

        public StorageService(StorageSetting setting)
        {
            bucketName = setting.BucketName;
            client = new AmazonS3Client(setting.GetCredentials(), setting.GetRegionEndpoint());
        }

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

        public bool UploadText(string key, string text, StorageFormat format = StorageFormat.PLAN, string encoder = "UTF-8")
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

        public bool UploadObject<T>(string key, T data, StorageFormat format = StorageFormat.PLAN, string encoder = "UTF-8")
        {
            string json = JsonSerializer.Serialize(data);
            return UploadText(key, json, format, encoder);
        }

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

        public T? GetObject<T>(string key, bool tryUnzip = false, string encoder = "UTF-8")
        {
            var json = GetText(key, tryUnzip, encoder);

            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }

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

        public bool Delete(string key)
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
