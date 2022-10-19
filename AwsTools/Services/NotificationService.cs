using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using SledGX.Tools.AWS.Models;
using System.Net;
using System.Text.Json;

namespace SledGX.Tools.AWS
{
    /// <summary>
    /// Wrapper for accessing Amazon Simple Notification Service (SNS).
    /// </summary>
    public class NotificationService
    {
        private readonly string topicArn;
        private readonly AmazonSimpleNotificationServiceClient client;

        /// <summary>
        /// Notification service initialization.
        /// </summary>
        /// <param name="setting">Notification service setting.</param>
        public NotificationService(NotificationSetting setting)
        {
            topicArn = setting.TopicArn;
            client = new AmazonSimpleNotificationServiceClient(setting.GetCredentials(), setting.GetRegionEndpoint());
        }

        /// <summary>
        /// Sends a text message to an Amazon SNS topic.
        /// </summary>
        /// <param name="message">The message you want to send.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PushMessage(string message)
        {
            var request = new PublishRequest
            {
                TopicArn = topicArn,
                Message = message
            };

            var response = client.PublishAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Sends an object to an Amazon SNS topic. The object is automatically transformed into json.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="data">The object you want to send.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PushObject<T>(T data)
        {
            string json = JsonSerializer.Serialize(data);
            return PushMessage(json);
        }

        /// <summary>
        /// Sends multiple text messages to an Amazon SNS topic.
        /// </summary>
        /// <param name="messages">The list of messages you want to send.</param>
        /// <param name="chunkSize">The quantity of messages to be sent at the same time.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PushMessages(List<string> messages, int chunkSize = 500)
        {
            var result = new List<HttpStatusCode>();
            foreach (var chunk in messages.Chunk(chunkSize))
            {
                var request = new PublishBatchRequest
                {
                    TopicArn = topicArn,
                    PublishBatchRequestEntries = messages
                        .Select(x => new PublishBatchRequestEntry
                        {
                            Message = x
                        })
                        .ToList()
                };

                var response = client.PublishBatchAsync(request).Result;
                result.Add(response.HttpStatusCode);
            }

            return result.All(x => x == HttpStatusCode.OK);
        }

        /// <summary>
        /// Sends multiple objects to an Amazon SNS topic. Objects are automatically transformed into json.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="data">The list of objects you want to send.</param>
        /// <param name="chunkSize">The quantity of messages to be sent at the same time.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PushObjects<T>(List<T> data, int chunkSize = 500)
        {
            var messages = data
                .Select(x => JsonSerializer.Serialize(x))
                .ToList();

            return PushMessages(messages, chunkSize);
        }
    }
}
