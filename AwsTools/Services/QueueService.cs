using Amazon.SQS;
using Amazon.SQS.Model;
using AwsTools.Models;
using System.Net;
using System.Text.Json;

namespace AwsTools.Services
{
    public class QueueService
    {
        private readonly string queueName;
        private readonly AmazonSQSClient client;

        public QueueService(QueueSetting setting)
        {
            queueName = setting.QueueName;
            client = new AmazonSQSClient(setting.GetCredentials(), setting.GetRegionEndpoint());
        }

        public int CountMessages()
        {
            var request = new GetQueueAttributesRequest
            {
                QueueUrl = queueName,
                AttributeNames = new List<string>
                {
                    "ApproximateNumberOfMessages"
                }
            };

            var response = client.GetQueueAttributesAsync(request).Result;
            return response.ApproximateNumberOfMessages;
        }

        public bool PushMessage(string message)
        {
            var request = new SendMessageRequest
            {
                QueueUrl = queueName,
                MessageBody = message
            };

            var response = client.SendMessageAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        public bool PushObject<T>(T data)
        {
            string json = JsonSerializer.Serialize(data);
            return PushMessage(json);
        }

        public bool PushMessages(List<string> messages, int chunkSize = 500)
        {
            var result = new List<HttpStatusCode>();
            foreach (var chunk in messages.Chunk(chunkSize))
            {
                var request = new SendMessageBatchRequest
                {
                    QueueUrl = queueName,
                    Entries = messages
                        .Select(x => new SendMessageBatchRequestEntry
                        {
                            MessageBody = x
                        })
                        .ToList()
                };

                var response = client.SendMessageBatchAsync(request).Result;
                result.Add(response.HttpStatusCode);
            }

            return result.All(x => x == HttpStatusCode.OK);
        }

        public bool PushObjects<T>(List<T> data, int chunkSize = 500)
        {
            var messages = data
                .Select(x => JsonSerializer.Serialize(x))
                .ToList();

            return PushMessages(messages, chunkSize);
        }

        public (string body, string receiptHandle)? ReceiveMessage()
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueName,
                WaitTimeSeconds = 1,
                MaxNumberOfMessages = 1
            };

            var response = client.ReceiveMessageAsync(request).Result;

            if (response?.Messages != null && response.Messages.Count > 0)
                return (response.Messages[0].Body, response.Messages[0].ReceiptHandle);

            return null;
        }

        public bool DeleteMessage(string receiptHandle)
        {
            var request = new DeleteMessageRequest
            {
                QueueUrl = queueName,
                ReceiptHandle = receiptHandle
            };

            var response = client.DeleteMessageAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }
    }
}
