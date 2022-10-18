using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AwsTools.Models;
using System.Net;
using System.Text.Json;

namespace AwsTools.Services
{
    public class NotificationService
    {
        private readonly string topicName;
        private readonly AmazonSimpleNotificationServiceClient client;

        public NotificationService(NotificationSetting setting)
        {
            topicName = setting.TopicName;
            client = new AmazonSimpleNotificationServiceClient(setting.GetCredentials(), setting.GetRegionEndpoint());
        }

        public bool PushMessage<T>(T message)
        {
            var request = new PublishRequest
            {
                TopicArn = topicName,
                Message = JsonSerializer.Serialize(message)
            };

            var response = client.PublishAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        public bool PushMessages<T>(List<T> messages, int chunkSize = 500)
        {
            var result = new List<HttpStatusCode>();
            foreach (var chunk in messages.Chunk(chunkSize))
            {
                var request = new PublishBatchRequest
                {
                    TopicArn = topicName,
                    PublishBatchRequestEntries = messages
                    .Select(x => new PublishBatchRequestEntry
                    {
                        Message = JsonSerializer.Serialize(x)
                    })
                    .ToList()
                };

                var response = client.PublishBatchAsync(request).Result;
                result.Add(response.HttpStatusCode);
            }

            return result.All(x => x == HttpStatusCode.OK);
        }
    }
}
