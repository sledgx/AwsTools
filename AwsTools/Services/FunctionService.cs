using Amazon.Lambda;
using Amazon.Lambda.Model;
using SledGX.Tools.AWS.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SledGX.Tools.AWS
{
    /// <summary>
    /// Wrapper for accessing Amazon Lambda.
    /// </summary>
    public class FunctionService
    {
        private readonly string functionName;
        private readonly AmazonLambdaClient client;

        /// <summary>
        /// Notification service initialization.
        /// </summary>
        /// <param name="setting">Notification service setting.</param>
        public FunctionService(FunctionSetting setting)
        {
            functionName = setting.FunctionName;
            client = new AmazonLambdaClient(setting.GetCredentials(), setting.GetRegionEndpoint());
        }

        /// <summary>
        /// Invoke a Lambda function.
        /// </summary>
        /// <param name="data">The data you want to send.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool Invoke<T>(T? data = default, string encoder = "UTF-8")
        {
            var request = new InvokeRequest
            {
                FunctionName = functionName,
                InvocationType = InvocationType.Event,
                Payload = data != null ? JsonSerializer.Serialize(data) : null
            };

            var response = client.InvokeAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Invoke a Lambda function and wait for a response.
        /// </summary>
        /// <param name="data">The data you want to send.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>The function result object.</returns>
        public TOutput? InvokeWithResponse<TInput, TOutput>(TInput? data = default, string encoder = "UTF-8")
        {
            var request = new InvokeRequest
            {
                FunctionName = functionName,
                InvocationType = InvocationType.RequestResponse,
                Payload = data != null ? JsonSerializer.Serialize(data) : null
            };

            var response = client.InvokeAsync(request).Result;

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return default;

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                response.Payload.CopyTo(stream);
                bytes = stream.ToArray();
            }

            var encoding = Encoding.GetEncoding(encoder);
            string json = encoding.GetString(bytes);

            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<TOutput>(json);
        }
    }
}
